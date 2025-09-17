using System;
using System.Linq;
using System.Collections.Generic;
using Maui.GoogleMaps;
using Microsoft.Maui.Graphics;
using TerraMarcadaV2.Models;
using Map = Maui.GoogleMaps.Map;
using TerraMarcadaV2.ViewModels;
using TerraMarcadaV2.Helpers;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Creation
{
    public enum CreateMode { None, Point, Polyline, Polygon, Hole, Circle }

    public sealed class CreationManager : IDisposable
    {
        private readonly Map _map;
        private readonly MapDataViewModel _vm;

        public Action<string> OnStatus { get; set; } = _ => { };
        public Action<string> OnStatusSize { get; set; } = _ => { };

        public bool Active => _mode != CreateMode.None;
        public bool SnapEnabled { get; set; } = true;
        public double SnapThresholdMeters { get; set; } = 15;
        public double CloseThresholdMeters { get; set; } = 15;

        private CreateMode _mode = CreateMode.None;
        public bool IsInHoleCreating() => _mode == CreateMode.Hole;

        private List<Position> _tempVerts = new();
        private Polyline _tempPolyline;
        private Polygon _tempPolygon;
        private Circle _tempCircle;

        private bool _polylinePreviewAdded;
        private bool _polygonPreviewAdded;
        private bool _circlePreviewAdded;

        private Polygon _parentPolygonForHole;

        private Position? _circleCenter;

        private bool _wired;

        private Pin _tempPointPin;
        private bool _pointPreviewAdded;

        public bool PreferSnapToPins { get; set; } = true;

        public double PinSnapThresholdMeters { get; set; } = 20;


        public CreationManager(Map map, MapDataViewModel vm)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            Wire();
        }

        public void Dispose()
        {
            Unwire();
            Cancel();
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;
            _map.MapClicked += OnMapClicked;
            _map.MapLongClicked += OnMapLongClicked;
            _map.PinClicked += OnPinClickedForCreation; 
        }

        private void Unwire()
        {
            if (!_wired) return;
            _wired = false;
            _map.MapClicked -= OnMapClicked;
            _map.MapLongClicked -= OnMapLongClicked;
            _map.PinClicked -= OnPinClickedForCreation; 
        }


        public void StartPoint()
        {
            Reset();
            _mode = CreateMode.Point;
            BuildTempPoint();                 
            OnStatus("Criação de PONTO: toque no mapa.");
        }

        public async void StartTimedPoint()
        {
            Reset();
            OnStatus("Iniciando ponto de precisão...");
            var loc = await GetAveragedLocationAsync();

            if(loc == null)
            {
                OnStatus("Falha ao obter localização. Tente novamente.");
                return;
            }

            var data = new MapData
            {
                Type = MapDataTypes.Pin,
                Name = "Ponto Temporizado",
                IsDraggable = true
            };
            var pos = new Position(loc.Latitude, loc.Longitude);
            data.SetCoordinates(new List<Position> { pos });
            await _vm.AddMapData(data, _map);
        }

        public void StartPolyline() { Reset(); _mode = CreateMode.Polyline; BuildTempPolyline(); OnStatus("Criação de LINHA: adicione vértices."); }
        public void StartPolygon() { Reset(); _mode = CreateMode.Polygon; BuildTempPolygon(); OnStatus("Criação de POLÍGONO: adicione vértices."); }
        public void StartHole() { Reset(); _mode = CreateMode.Hole; OnStatus("Criação de BURACO: 1º toque dentro do polígono-alvo."); }
        public void StartCircle() { Reset(); _mode = CreateMode.Circle; BuildTempCircle(); OnStatus("Criação de CÍRCULO: toque para centro, depois para raio."); }

        public void Undo()
        {
            if (!Active) { OnStatus("Nada para desfazer."); return; }

            if (_mode == CreateMode.Circle)
            {
                if (_circleCenter != null)
                {
                    _circleCenter = null;
                    UpdateTempCircle();
                    OnStatus("Centro removido.");
                }
                else OnStatus("Nada para desfazer.");
                return;
            }

            if (_tempVerts.Count > 0)
            {
                _tempVerts.RemoveAt(_tempVerts.Count - 1);
                UpdatePreview();
            }
            else OnStatus("Nada para desfazer.");
        }

        public async System.Threading.Tasks.Task<bool> FinishAsync()
        {
            if (!Active) return false;

            try
            {
                switch (_mode)
                {
                    case CreateMode.Point:
                        {
                            if (_tempVerts.Count != 1)
                            {
                                OnStatus("Toque no mapa para posicionar o ponto.");
                                return false;
                            }
                            var data = new MapData
                            {
                                Type = MapDataTypes.Pin,
                                Name = "Ponto",
                                IsDraggable = true
                            };
                            data.SetCoordinates(new List<Position> { _tempVerts[0] });

                            await _vm.AddMapData(data, _map);

                            _map.FocusOn(_tempVerts[0], 500);
                            EnsurePreviewOnMapForPoint(false);

                            Reset();
                            OnStatus("Ponto criado.");
                            return true;
                        }

                    case CreateMode.Polyline:
                        {
                            if (_tempVerts.Count < 2) { OnStatus("Adicione pelo menos 2 vértices."); return false; }
                            var data = new MapData
                            {
                                Type = MapDataTypes.Polyline,
                                Name = "Linha",
                                StrokeWidth = 5f
                            };
                            data.SetCoordinates(_tempVerts);
                            await _vm.AddMapData(data, _map);
                            _map.FocusOn(_tempVerts[0], 500);
                            Reset();
                            OnStatus("Linha criada.");
                            return true;
                        }

                    case CreateMode.Polygon:
                        {
                            var verts = NormalizePolygonVerts(_tempVerts);
                            if (verts.Count < 3) { OnStatus("Adicione pelo menos 3 vértices."); return false; }

                            var data = new MapData
                            {
                                Type = MapDataTypes.Polygon,
                                Name = "Polígono",
                                StrokeWidth = 3f,
                                FillColor = Color.FromRgba(0, 255, 0, 32)
                            };
                            data.SetCoordinates(verts);
                            await _vm.AddMapData(data, _map);
                            _map.FocusOn(_tempVerts[0], 500);
                            Reset();
                            OnStatus("Polígono criado.");
                            return true;
                        }

                    case CreateMode.Hole:
                        {
                            if (_parentPolygonForHole == null)
                            {
                                OnStatus("Toque dentro do polígono-alvo para começar o buraco.");
                                return false;
                            }
                            var verts = NormalizePolygonVerts(_tempVerts);
                            if (verts.Count < 3) { OnStatus("Buraco precisa de pelo menos 3 vértices."); return false; }

                            var data = new MapData
                            {
                                Type = MapDataTypes.Hole,
                                Name = "Buraco",
                                HolePolygon = _parentPolygonForHole
                            };
                            data.SetCoordinates(verts);

                            await _vm.AddMapData(data, _map);
                            _map.FocusOn(_tempVerts[0], 500);
                            Reset();
                            OnStatus("Buraco criado.");
                            return true;
                        }

                    case CreateMode.Circle:
                        {
                            if (_circleCenter == null) { OnStatus("Defina o centro e depois o raio."); return false; }
                            if (_tempCircle == null || _tempCircle.Radius.Meters <= 0)
                            { OnStatus("Defina o raio com o segundo toque."); return false; }

                            var data = new MapData
                            {
                                Type = MapDataTypes.Circle,
                                Name = "Círculo",
                                StrokeWidth = 4f,
                                Radius = (float)_tempCircle.Radius.Meters
                            };
                            data.SetCoordinates(new List<Position> { _tempCircle.Center });
                            await _vm.AddMapData(data, _map);
                            _map.FocusOn(_tempVerts[0], 500);
                            Reset();
                            OnStatus("Círculo criado.");
                            return true;
                        }
                }
            }
            catch (Exception ex)
            {
                OnStatus($"Erro ao concluir: {ex.Message}");
                return false;
            }

            return false;
        }

        public void Cancel() => Reset();

        private void Reset()
        {
            _mode = CreateMode.None;
            _tempVerts.Clear();
            _parentPolygonForHole = null;
            _circleCenter = null;
            RemovePreview();
            OnStatus("Pronto.");
            OnStatusSize("");
        }
        private void BuildTempPoint()
        {
            _tempPointPin = new Pin
            {
                Label = "Prévia",
                IsDraggable = false,
                Icon = BitmapDescriptorFactory.DefaultMarker(Colors.Magenta),
                ZIndex = 10000
            };
            _pointPreviewAdded = false; // só adiciona quando tiver posição
        }

        private void EnsurePreviewOnMapForPoint(bool hasPosition)
        {
            if (hasPosition && !_pointPreviewAdded)
            {
                _map.Pins.Add(_tempPointPin);
                _pointPreviewAdded = true;
            }
            else if (!hasPosition && _pointPreviewAdded)
            {
                _map.Pins.Remove(_tempPointPin);
                _pointPreviewAdded = false;
            }
        }


        private void BuildTempPolyline()
        {
            _tempPolyline = new Polyline
            {
                StrokeColor = Colors.Red,   
                StrokeWidth = 6f,
                IsClickable = false,
                ZIndex = 10000
            };
            _polylinePreviewAdded = false;
        }

        private void BuildTempPolygon()
        {
            _tempPolygon = new Polygon
            {
                StrokeColor = Colors.Yellow, 
                StrokeWidth = 4f,
                FillColor = Color.FromRgba(255, 255, 0, 64), 
                IsClickable = false,
                ZIndex = 10000
            };
            _polygonPreviewAdded = false;
        }

        private void BuildTempCircle()
        {
            _tempCircle = new Circle
            {
                Center = new Position(0, 0),
                Radius = Distance.FromMeters(0),
                StrokeColor = Colors.Cyan,
                StrokeWidth = 5f,
                FillColor = Color.FromRgba(0, 255, 255, 48),
                IsClickable = false,
                ZIndex = 10000
            };
            _circlePreviewAdded = false;
        }

        private void RemovePreview()
        {
            if (_tempPolyline != null && _polylinePreviewAdded) { _map.Polylines.Remove(_tempPolyline); }
            if (_tempPolygon != null && _polygonPreviewAdded) { _map.Polygons.Remove(_tempPolygon); }
            if (_tempCircle != null && _circlePreviewAdded) { _map.Circles.Remove(_tempCircle); }
            if (_tempPointPin != null && _pointPreviewAdded) { _map.Pins.Remove(_tempPointPin); }

            _tempPolyline = null; _polylinePreviewAdded = false;
            _tempPolygon = null; _polygonPreviewAdded = false;
            _tempCircle = null; _circlePreviewAdded = false;
            _tempPointPin = null; _pointPreviewAdded = false;

            OnStatusSize("");
        }

        private void EnsurePreviewOnMapForPolyline(int count)
        {
            if (count >= 2 && !_polylinePreviewAdded)
            {
                _map.Polylines.Add(_tempPolyline);
                _polylinePreviewAdded = true;
            }
            else if (count < 2 && _polylinePreviewAdded)
            {
                _map.Polylines.Remove(_tempPolyline);
                _polylinePreviewAdded = false;
            }

            if (_tempPolyline != null && _tempPolyline.Positions.Count < 2)
            {
                OnStatusSize("Distancia: N/A");
                return;
            }

            var posList = _tempPolyline?.Positions.ToList() ?? new List<Position>();
            double DistanceInMeters = GeoMath.CalculatePolylineDistance(posList);

            OnStatusSize($": Distância: {DistanceInMeters:F2} m");
        }

        private void EnsurePreviewOnMapForPolygon(int count)
        {
            if (count >= 3 && !_polygonPreviewAdded)
            {
                _map.Polygons.Add(_tempPolygon);
                _polygonPreviewAdded = true;
            }
            else if (count < 3 && _polygonPreviewAdded)
            {
                _map.Polygons.Remove(_tempPolygon);
                _polygonPreviewAdded = false;
            }

            if(_tempPolygon != null && _tempPolygon.Positions.Count < 3)
            {
                OnStatusSize("Área: N/A");
                return;
            }

            var posList = _tempPolygon?.Positions.ToList() ?? new List<Position>();
            double AreaInMetersSquared = GeoMath.ComputePolygonAreaSquareMeters(posList);

            OnStatusSize($"Área: {GeoMath.FormatAreaHa(AreaInMetersSquared)} hectares, m² {AreaInMetersSquared:F2}");
        }

        private void EnsurePreviewOnMapForCircle(double radiusMeters)
        {
            if (radiusMeters > 0 && !_circlePreviewAdded)
            {
                _map.Circles.Add(_tempCircle);
                _circlePreviewAdded = true;
            }
            else if (radiusMeters <= 0 && _circlePreviewAdded)
            {
                _map.Circles.Remove(_tempCircle);
                _circlePreviewAdded = false;
            }
        }

        private void UpdatePreview()
        {
            switch (_mode)
            {
                case CreateMode.Polyline:
                    if (_tempPolyline == null) BuildTempPolyline();
                    _tempPolyline.Positions.Clear();
                    foreach (var p in _tempVerts) _tempPolyline.Positions.Add(p);
                    EnsurePreviewOnMapForPolyline(_tempVerts.Count);
                    break;

                case CreateMode.Polygon:
                case CreateMode.Hole:
                    if (_tempPolygon == null) BuildTempPolygon();
                    var verts = NormalizePolygonVerts(_tempVerts);
                    _tempPolygon.Positions.Clear();
                    foreach (var p in verts) _tempPolygon.Positions.Add(p);
                    EnsurePreviewOnMapForPolygon(_tempPolygon.Positions.Count);
                    break;
            }
        }

        private void UpdateTempCircle()
        {
            if (_tempCircle == null) BuildTempCircle();

            if (_circleCenter == null)
            {
                _tempCircle.Center = new Position(0, 0);
                _tempCircle.Radius = Distance.FromMeters(0);
                EnsurePreviewOnMapForCircle(0);
            }
            else
            {
                EnsurePreviewOnMapForCircle(_tempCircle.Radius.Meters);
            }
        }

        private bool TrySnapToPin(Position pt, out Position snapped, out string pinLabel)
        {
            snapped = pt;
            pinLabel = null;

            double best = double.PositiveInfinity;
            Pin bestPin = null;

            foreach (var pin in _map.Pins)
            {
                if (!IsSnappablePin(pin)) continue;

                var d = GeoUtils.DistanceMeters(pin.Position, pt);
                if (d < best)
                {
                    best = d;
                    bestPin = pin;
                }
            }

            if (bestPin != null && best <= PinSnapThresholdMeters)
            {
                snapped = bestPin.Position;
                pinLabel = string.IsNullOrWhiteSpace(bestPin.Label) ? "Pin" : bestPin.Label;
                return true;
            }
            return false;
        }

        // Evita "colar" no pin de prévia e em pinos especiais de edição
        private bool IsSnappablePin(Pin pin)
        {
            if (pin == null) return false;

            if (_tempPointPin != null && ReferenceEquals(pin, _tempPointPin))
                return false;

            // se você usa Tags string para pinos de edição (ex.: "EDIT_*"), ignore-os
            if (pin.Tag is string s && s.StartsWith("EDIT_", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        //public void HandleHoleClick()

        private async void OnMapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (!Active) return;

            if (_mode == CreateMode.Hole && _parentPolygonForHole == null)
            {
                var pg = FindContainingPolygon(e.Point);
                if (pg == null) { OnStatus("Nenhum polígono contém esse ponto. Toque dentro do polígono-alvo."); return; }
                _parentPolygonForHole = pg;
                OnStatus("Polígono selecionado. Agora adicione os vértices do buraco.");
                return;
            }
            Position p = e.Point;
            //if (SnapEnabled) p = SnapToNearest(p) ?? p;
            string snappedFromPin = null;
            if (SnapEnabled && PreferSnapToPins && TrySnapToPin(p, out var pinPos, out var pinLabel))
            {
                p = pinPos;
                snappedFromPin = pinLabel;
            }
            else if (SnapEnabled)
            {
                p = SnapToNearest(p) ?? p;
            }

            switch (_mode)
            {
                case CreateMode.Point:
                    {
                        _tempVerts.Clear();
                        _tempVerts.Add(p);

                        if (_tempPointPin == null) BuildTempPoint();
                        _tempPointPin.Position = p;
                        EnsurePreviewOnMapForPoint(true);

                        if (snappedFromPin != null)
                            OnStatus($"Ponto no pin: {snappedFromPin}. Pressione 'Concluir'.");
                        else
                            OnStatus($"Ponto selecionado ({p.Latitude:0.000000}, {p.Longitude:0.000000}). Pressione 'Concluir'.");

                        break;
                    }

                case CreateMode.Polyline:
                    {
                        _tempVerts.Add(p);
                        UpdatePreview();
                        if (snappedFromPin != null)
                            OnStatus($"Vértice colado no pin: {snappedFromPin}.");
                        break;
                    }


                case CreateMode.Polygon:
                case CreateMode.Hole:
                    {
                        if (_tempVerts.Count >= 1)
                        {
                            if (GeoUtils.DistanceMeters(_tempVerts[0], p) <= CloseThresholdMeters)
                            {
                                if (_tempVerts.Count >= 3)
                                {
                                    _tempVerts.Add(_tempVerts[0]);
                                    UpdatePreview();
                                    OnStatus("Fechado. Pressione 'Concluir' para salvar.");
                                    return;
                                }
                            }
                        }
                        _tempVerts.Add(p);
                        UpdatePreview();
                        if (snappedFromPin != null)
                            OnStatus($"Vértice colado no pin: {snappedFromPin}.");
                        break;
                    }


                case CreateMode.Circle:
                    {
                        if (_circleCenter == null)
                        {
                            _circleCenter = p;
                            if (_tempCircle == null) BuildTempCircle();
                            _tempCircle.Center = p;
                            _tempCircle.Radius = Distance.FromMeters(1);
                            UpdateTempCircle();
                            OnStatus(snappedFromPin != null
                                ? $"Centro no pin: {snappedFromPin}. Toque novamente para definir o raio."
                                : "Centro definido. Toque novamente para definir o raio.");
                        }
                        else
                        {
                            var r = GeoUtils.DistanceMeters(_circleCenter.Value, p);
                            if (_tempCircle == null) BuildTempCircle();
                            _tempCircle.Center = _circleCenter.Value;
                            _tempCircle.Radius = Distance.FromMeters(r);
                            UpdateTempCircle();
                            OnStatus($"Raio: {r:0} m. Pressione 'Concluir' para salvar.");
                        }
                        break;
                    }
                }
            }
        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            if (!Active) return;

            if (_mode == CreateMode.Hole && _parentPolygonForHole == null)
            {
                var pg = FindContainingPolygon(e.Point);
                if (pg == null) { OnStatus("Nenhum polígono contém esse ponto. Toque dentro do polígono-alvo."); return; }
                _parentPolygonForHole = pg;
                OnStatus("Polígono selecionado. Agora adicione os vértices do buraco.");
                return;
            }

            Position p = e.Point;
            //if (SnapEnabled) p = SnapToNearest(p) ?? p;
            string snappedFromPin = null;
            if (SnapEnabled && PreferSnapToPins && TrySnapToPin(p, out var pinPos, out var pinLabel))
            {
                p = pinPos;
                snappedFromPin = pinLabel;
            }
            else if (SnapEnabled)
            {
                p = SnapToNearest(p) ?? p;
            }

            switch (_mode)
            {
                case CreateMode.Point:
                    {
                        _tempVerts.Clear();
                        _tempVerts.Add(p);

                        if (_tempPointPin == null) BuildTempPoint();
                        _tempPointPin.Position = p;
                        EnsurePreviewOnMapForPoint(true);

                        if (snappedFromPin != null)
                            OnStatus($"Ponto no pin: {snappedFromPin}. Pressione 'Concluir'.");
                        else
                            OnStatus($"Ponto selecionado ({p.Latitude:0.000000}, {p.Longitude:0.000000}). Pressione 'Concluir'.");

                        break;
                    }

                case CreateMode.Polyline:
                    {
                        _tempVerts.Add(p);
                        UpdatePreview();
                        if (snappedFromPin != null)
                            OnStatus($"Vértice colado no pin: {snappedFromPin}.");
                        break;
                    }


                case CreateMode.Polygon:
                case CreateMode.Hole:
                    {
                        if (_tempVerts.Count >= 1)
                        {
                            if (GeoUtils.DistanceMeters(_tempVerts[0], p) <= CloseThresholdMeters)
                            {
                                if (_tempVerts.Count >= 3)
                                {
                                    _tempVerts.Add(_tempVerts[0]); // fechamento visual
                                    UpdatePreview();
                                    OnStatus("Fechado. Pressione 'Concluir' para salvar.");
                                    return;
                                }
                            }
                        }
                        _tempVerts.Add(p);
                        UpdatePreview();
                        if (snappedFromPin != null)
                            OnStatus($"Vértice colado no pin: {snappedFromPin}.");
                        break;
                    }


                case CreateMode.Circle:
                    {
                        if (_circleCenter == null)
                        {
                            _circleCenter = p;
                            if (_tempCircle == null) BuildTempCircle();
                            _tempCircle.Center = p;
                            _tempCircle.Radius = Distance.FromMeters(1);
                            UpdateTempCircle();
                            OnStatus(snappedFromPin != null
                                ? $"Centro no pin: {snappedFromPin}. Toque novamente para definir o raio."
                                : "Centro definido. Toque novamente para definir o raio.");
                        }
                        else
                        {
                            var r = GeoUtils.DistanceMeters(_circleCenter.Value, p);
                            if (_tempCircle == null) BuildTempCircle();
                            _tempCircle.Center = _circleCenter.Value;
                            _tempCircle.Radius = Distance.FromMeters(r);
                            UpdateTempCircle();
                            OnStatus($"Raio: {r:0} m. Pressione 'Concluir' para salvar.");
                        }
                        break;
                    }

            }
        }

        // aviso: O Maui.GoogleMaps espera retorno síncrono 
        private void OnPinClickedForCreation(object sender, PinClickedEventArgs e)
        {
            if (!Active) 
                return; 

            var pin = e.Pin;
            if (pin == null) return;

            if (!IsSnappablePin(pin)) return;

            var p = pin.Position;
            var pinName = !string.IsNullOrWhiteSpace(pin.Label) ? pin.Label
                       : (pin.Tag?.ToString() ?? "Pin");

            switch (_mode)
            {
                case CreateMode.Point:
                    _tempVerts.Clear();
                    _tempVerts.Add(p);

                    if (_tempPointPin == null) BuildTempPoint();
                    _tempPointPin.Position = p;
                    EnsurePreviewOnMapForPoint(true);

                    OnStatus($"Ponto no pin: {pinName}. Pressione 'Concluir'.");
                    e.Handled = true; 
                    return;

                case CreateMode.Polyline:
                    _tempVerts.Add(p);
                    UpdatePreview();
                    OnStatus($"Vértice adicionado no pin: {pinName}.");
                    e.Handled = true;
                    return;

                case CreateMode.Polygon:
                case CreateMode.Hole:
                    // permitir fechar tocando num pin "sobre" o primeiro vértice
                    if (_tempVerts.Count >= 1 &&
                        GeoUtils.DistanceMeters(_tempVerts[0], p) <= CloseThresholdMeters &&
                        _tempVerts.Count >= 3)
                    {
                        _tempVerts.Add(_tempVerts[0]); 
                        UpdatePreview();
                        OnStatus("Fechado. Pressione 'Concluir' para salvar.");
                        e.Handled = true;
                        return;
                    }

                    _tempVerts.Add(p);
                    UpdatePreview();
                    OnStatus($"Vértice adicionado no pin: {pinName}.");
                    e.Handled = true;
                    return;

                case CreateMode.Circle:
                    if (_circleCenter == null)
                    {
                        _circleCenter = p;
                        if (_tempCircle == null) BuildTempCircle();
                        _tempCircle.Center = p;
                        _tempCircle.Radius = Distance.FromMeters(0);
                        UpdateTempCircle();
                        OnStatus($"Centro definido no pin: {pinName}. Toque novamente para definir o raio.");
                    }
                    else
                    {
                        var r = GeoUtils.DistanceMeters(_circleCenter.Value, p);
                        if (_tempCircle == null) BuildTempCircle();
                        _tempCircle.Center = _circleCenter.Value;
                        _tempCircle.Radius = Distance.FromMeters(r);
                        UpdateTempCircle();
                        OnStatus($"Raio até o pin '{pinName}': {r:0} m. Pressione 'Concluir'.");
                    }
                    e.Handled = true;
                    return;

                default:
                    return;
            }
        }

        private List<Position> NormalizePolygonVerts(List<Position> verts)
        {
            if (verts == null || verts.Count == 0) return new List<Position>();
            var res = new List<Position>(verts);

            if (res.Count >= 3)
            {
                if (!GeoUtils.SequenceAlmostEqual(new[] { res[0] }, new[] { res[^1] }, 0.05))
                    res.Add(res[0]);
            }
            return res;
        }

        private Polygon FindContainingPolygon(Position pt)
        {
            foreach (var polygon in _map.Polygons)
                if (PointInPolygon(pt, polygon.Positions)) return polygon;
            return null;
        }

        private static bool PointInPolygon(Position p, IList<Position> ring)
        {
            if (ring == null || ring.Count < 3) return false;
            bool inside = false;
            int n = ring.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var pi = ring[i];
                var pj = ring[j];
                bool intersect = ((pi.Latitude > p.Latitude) != (pj.Latitude > p.Latitude)) &&
                                 (p.Longitude < (pj.Longitude - pi.Longitude) * (p.Latitude - pi.Latitude) /
                                  ((pj.Latitude - pi.Latitude) + double.Epsilon) + pi.Longitude);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        private Position? SnapToNearest(Position p)
        {
            double best = double.PositiveInfinity;
            Position? bestPos = null;

            foreach (var pin in _map.Pins)
            {
                var d = GeoUtils.DistanceMeters(pin.Position, p);
                if (d < best) { best = d; bestPos = pin.Position; }
            }
            foreach (var pl in _map.Polylines)
                foreach (var v in pl.Positions)
                {
                    var d = GeoUtils.DistanceMeters(v, p);
                    if (d < best) { best = d; bestPos = v; }
                }
            foreach (var pg in _map.Polygons)
                foreach (var v in pg.Positions)
                {
                    var d = GeoUtils.DistanceMeters(v, p);
                    if (d < best) { best = d; bestPos = v; }
                }

            if (best <= SnapThresholdMeters) return bestPos;
            return null;
        }
        async Task<Location?> GetAveragedLocationAsync(int seconds = 5)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            var samples = new List<(Location loc, double acc)>();

            while (DateTime.UtcNow < end)
            {
                try
                {
                    var fix = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(2)));
                    if (fix != null)
                    {
                        var acc = fix.Accuracy ?? 12; // metros
                        samples.Add((new Location(fix.Latitude, fix.Longitude), Math.Max(1, acc)));
                    }
                }
                catch { /* ignora leitura ruim */ }
                await Task.Delay(800);
            }
            if (samples.Count == 0) return null;

            // média ponderada por 1/acc^2 (mais preciso pesa mais)
            double sw = 0, slat = 0, slon = 0;
            foreach (var (loc, acc) in samples)
            {
                double w = 1.0 / (acc * acc);
                sw += w; slat += loc.Latitude * w; slon += loc.Longitude * w;
            }
            return new Location(slat / sw, slon / sw);
        }
    }
}

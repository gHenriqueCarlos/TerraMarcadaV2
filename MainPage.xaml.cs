using Maui.GoogleMaps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TerraMarcadaV2.Creation;
using TerraMarcadaV2.Editing; 
using TerraMarcadaV2.Helpers;
using TerraMarcadaV2.Models;
using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;

namespace TerraMarcadaV2
{
    public partial class MainPage : ContentPage
    {
        private MapDataViewModel _vm;
        private EditManager _edit;
        private CreationManager _create;

        // Seleção atual (para Edição)
        private Pin _selectedPin;
        private Polygon _selectedPolygon;
        private Polyline _selectedPolyline;
        private Circle _selectedCircle;

        // Modos de seleção (Edição)
        private bool _awaitingSelectPolygon = false;
        private bool _awaitingSelectPolyline = false;

        // Modo escolher buraco (Edição)
        private bool _awaitingHoleTap = false;

        // Binder para cliques em shapes
        private readonly MapShapeClickBinder _binder = new();

        // estilos p/ restaurar destaque
        private (Polygon poly, Color stroke, float width)? _polyOld;
        private (Polyline line, Color stroke, float width)? _lineOld;
        private (Circle circ, Color stroke, float width)? _circOld;

        private readonly DatabaseService db;

        public string StatusText
        {
            get => lblStatus.Text;  
            set
            {
                lblStatus.Text = value;
                lblStatus2.Text = value;  // Label que fica no fim da tela
            }
        }

        public string StatusSizeText
        {
            get => lblArea.Text;  
            set
            {
                lblArea.Text = value;   // Label para informar o tamanho da área
                lblArea.Text = value;  
            }
        }

        private void UpdateStatus(string newStatus)
        {
            StatusText = newStatus;

            if (string.IsNullOrWhiteSpace(newStatus))
                BorderLblStatus2.IsVisible = false;
            else
                BorderLblStatus2.IsVisible = true;
        }

        private void UpdateStatusSize(string newStatus)
        {
            StatusSizeText = newStatus; 
            if (string.IsNullOrWhiteSpace(newStatus))
                BorderLblArea.IsVisible = false;
            else
                BorderLblArea.IsVisible = true;
        }

        public MainPage()
        {
            InitializeComponent();

            UpdateStatus("");
            UpdateStatusSize("");

            db = ServiceHelper.GetService<DatabaseService>();

            _vm = new MapDataViewModel();
            //_edit = new EditManager(map, _vm);
            _edit = new EditManager(map, _vm)
            {
                //OnStatus = s => lblStatus.Text = s
                OnStatusSize = s => UpdateStatusSize(s)
            };
            _create = new CreationManager(map, _vm)
            {
                //OnStatus = s => lblStatus.Text = s
                OnStatus = s => StatusText = s,
                OnStatusSize = s => UpdateStatusSize(s)
            };

            map.MapClicked += OnMapClickedForSelectionOrHole;
            map.MapLongClicked += OnMapLongClicked;

            map.PinClicked += OnMapPinClicked;

            map.MyLocationEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.RotateGesturesEnabled = true;

            //map.MapLongClicked += (sender, e) =>
            //{
            //    var lat = e.Point.Latitude.ToString("0.000");
            //    var lng = e.Point.Longitude.ToString("0.000");
            //    this.DisplayAlert("MapLongClicked", $"{lat}/{lng}", "CLOSE");
            //};
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _vm.LoadAllToMapAsync(map);

            _binder.AttachAll(map, OnPolygonShapeClicked, OnPolylineShapeClicked, OnCircleShapeClicked);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _edit?.CancelEdit();
            _create?.Cancel();
            _binder?.DetachAll();
        }

        // =========================
        // # Destaque / seleção visual #
        // =========================
        private void ClearHighlights()
        {
            if (_polyOld is { } p)
            {
                p.poly.StrokeColor = p.stroke;
                p.poly.StrokeWidth = p.width;
                _polyOld = null;
            }
            if (_lineOld is { } l)
            {
                l.line.StrokeColor = l.stroke;
                l.line.StrokeWidth = l.width;
                _lineOld = null;
            }
            if (_circOld is { } c)
            {
                c.circ.StrokeColor = c.stroke;
                c.circ.StrokeWidth = c.width;
                _circOld = null;
            }
        }

        private void Highlight(Polygon pg)
        {
            ClearHighlights();
            _polyOld = (pg, pg.StrokeColor, pg.StrokeWidth);
            pg.StrokeColor = Colors.White;
            pg.StrokeWidth = Math.Max(3f, pg.StrokeWidth + 2f);
        }

        private void Highlight(Polyline pl)
        {
            ClearHighlights();
            _lineOld = (pl, pl.StrokeColor, pl.StrokeWidth);
            pl.StrokeColor = Colors.White;
            pl.StrokeWidth = Math.Max(4f, pl.StrokeWidth + 2f);
        }

        private void Highlight(Circle c)
        {
            ClearHighlights();
            _circOld = (c, c.StrokeColor, c.StrokeWidth);
            c.StrokeColor = Colors.White;
            c.StrokeWidth = Math.Max(4f, c.StrokeWidth + 2f);
        }

        // =======================================
        // # CRIAÇÃO #
        // =======================================
        private void OnSnapToggled(object sender, ToggledEventArgs e)
        {
            _create.SnapEnabled = e.Value;
            UpdateStatus(e.Value ? "Snap habilitado." : "Snap desabilitado.");
        }

        private void OnCreatePointClicked(object sender, EventArgs e)
        {
            _create.StartPoint();
            UpdateStatus("Criação de PONTO: toque no mapa para posicionar.");
        }

        private async void OnCreatePointTimedClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ponto de Precisão", "Capturando média por 5 a 10 segundos. Mantenha o celular parado.\nClique em 'OK' e aguarde parado e sem mexer no celular.", "OK");
            _create.StartTimedPoint(); 
        }

        private void OnCreatePolylineClicked(object sender, EventArgs e)
        {
            _create.StartPolyline();
            UpdateStatus("Criação de LINHA: toque para adicionar vértices, 'Concluir' para finalizar.");
        }

        private void OnCreatePolygonClicked(object sender, EventArgs e)
        {
            _create.StartPolygon();
            UpdateStatus("Criação de POLÍGONO: toque para adicionar vértices, 'Concluir' ou toque no primeiro vértice para fechar.");
        }

        private void OnCreateHoleClicked(object sender, EventArgs e)
        {
            _create.StartHole();
            UpdateStatus("Criação de BURACO: 1º toque dentro do polígono-alvo; depois adicione os vértices e 'Concluir'.");
        }

        private void OnCreateCircleClicked(object sender, EventArgs e)
        {
            _create.StartCircle();
            UpdateStatus("Criação de CÍRCULO: 1º toque define o centro, 2º toque define o raio.");
        }

        private void OnCreateUndoClicked(object sender, EventArgs e)
        {
            _create.Undo();
        }

        private async void OnCreateFinishClicked(object sender, EventArgs e)
        {
            var ok = await _create.FinishAsync();
            if (!ok)
            {
                // Deixe a mensagem específica do CreationManager no lblStatus
                return;
            }

            // Reatacha cliques para os shapes recém-criados
            _binder.AttachAll(map, OnPolygonShapeClicked, OnPolylineShapeClicked, OnCircleShapeClicked);
        }

        private void OnCreateCancelClicked(object sender, EventArgs e)
        {
            _create.Cancel();
            UpdateStatus("Criação cancelada.");
        }

        private void OnMapPinClicked(object sender, PinClickedEventArgs e)
        {
            if (_create.Active) return; // criação tem prioridade no clique3
            //e.Handled = false;
            _selectedPin = e.Pin;
        }

        private async void OnDeleteSelectedClicked(object sender, EventArgs e)
        {
            if (_create.Active) { UpdateStatus("Finalize ou cancele a criação antes de deletar."); return; }

            if (_selectedPolygon != null)
            {
                _vm.RemoveMapDataAsync(_selectedPolygon.Tag as MapData, map);
                UpdateStatus("Polígono deletado.");
                _selectedPolygon = null;
                return;
            }
            if (_selectedPolyline != null)
            {
                _vm.RemoveMapDataAsync(_selectedPolyline.Tag as MapData, map);
                UpdateStatus("Polilinha deletada.") ;
                _selectedPolyline = null;
                return;
            }
            if (_selectedCircle != null)
            {
                _vm.RemoveMapDataAsync(_selectedCircle.Tag as MapData, map);
                UpdateStatus("Círculo deletado.");
                _selectedCircle = null;
                return;
            }
            if (_selectedPin != null)
            {
                _vm.RemoveMapDataAsync(_selectedPin.Tag as MapData, map);
                UpdateStatus("Ponto deletado.");
                _selectedPin = null;
                return;
            }

            UpdateStatus("Nenhum elemento selecionado para deletar.");
        }


        // =======================================
        // # EDIÇÃO #
        // =======================================
        private void OnInsertToggled(object sender, ToggledEventArgs e)
        {
            _edit.SetInsertMode(e.Value);
            UpdateStatus(e.Value
                ? "Inserção de vértices ATIVA. Toque no mapa para inserir no segmento mais próximo."
                : "Inserção de vértices DESATIVADA.");
        }

        private void OnSelectPolygonClicked(object sender, EventArgs e)
        {
            if (_create.Active) { UpdateStatus("Finalize/cancele a criação antes de selecionar para edição."); return; }
            _awaitingSelectPolygon = true;
            _awaitingSelectPolyline = false;
            _awaitingHoleTap = false;
            UpdateStatus("Toque dentro do polígono para selecioná-lo.");
        }

        private void OnSelectPolylineClicked(object sender, EventArgs e)
        {
            if (_create.Active) { UpdateStatus("Finalize/cancele a criação antes de selecionar para edição."); return; }
            _awaitingSelectPolyline = true;
            _awaitingSelectPolygon = false;
            _awaitingHoleTap = false;
            UpdateStatus("Toque próximo à polilinha para selecioná-la.");
        }

        private void OnEditSelectedClicked(object sender, EventArgs e)
        {
            if (_create.Active) { UpdateStatus("Finalize/cancele a criação antes de editar."); return; }

            if (_selectedPolygon != null)
            {
                _edit.StartEditPolygon(_selectedPolygon);
                UpdateStatus("Editando polígono. Arraste os pinos laranja. Clique no pino para remover.");
                return;
            }
            if (_selectedPolyline != null)
            {
                _edit.StartEditPolyline(_selectedPolyline);
                UpdateStatus("Editando polilinha. Arraste os pinos laranja. Clique no pino para remover.");
                return;
            }
            DisplayAlert("Selecionar", "Selecione primeiro um polígono ou uma polilinha.", "OK");
        }

        private void OnEditHoleClicked(object sender, EventArgs e)
        {
            if (_create.Active) { UpdateStatus("Finalize/cancele a criação antes de editar buraco."); return; }
            if (_selectedPolygon == null)
            {
                DisplayAlert("Editar buraco", "Selecione primeiro o polígono pai.", "OK");
                return;
            }
            _awaitingHoleTap = true;
            _awaitingSelectPolygon = false;
            _awaitingSelectPolyline = false;
            UpdateStatus("Toque dentro do buraco desejado para editar.");
        }

        private void OnFinishEditingClicked(object sender, EventArgs e)
        {
            _edit.CancelEdit();
            UpdateStatus("Edição concluída.");
            ClearHighlights();
        }

        // Menu de Criação
        private void OnCreateClicked(object sender, EventArgs e)
        {
            CreationOptions.IsVisible = !CreationOptions.IsVisible;
            EditOptions.IsVisible = false; // Garantir que o menu de edição não apareça
            MapElementsList.IsVisible = false;
        }

        // Menu de Edição
        private void OnEditClicked(object sender, EventArgs e)
        {
            EditOptions.IsVisible = !EditOptions.IsVisible;
            CreationOptions.IsVisible = false; // Garantir que o menu de criação não apareça
            MapElementsList.IsVisible = false;
        }


        // =======================================
        // # Clique no mapa: seleção (quando NÃO criando) #
        // =======================================

        private async void OnMapLongClicked(object sender, MapLongClickedEventArgs e)

            {
                if (_create.Active) 
                return; 

                if (_awaitingSelectPolygon)
                {
                    var poly = FindContainingPolygon(e.Point);
                    if (poly != null)
                    {
                        _selectedPolygon = poly;
                        _selectedPolyline = null;
                        _selectedCircle = null;
                        Highlight(poly);
                        UpdateStatus($"Polígono selecionado (vértices: {poly.Positions.Count}).");
                    }
                    else
                    {
                        UpdateStatus("Nenhum polígono contém esse ponto.");
                    }
                    _awaitingSelectPolygon = false;
                    return;
                }

                if (_awaitingSelectPolyline)
                {
                    var pl = FindNearestPolyline(e.Point, out var distMeters);
                    if (pl != null && distMeters <= 50)
                    {
                        _selectedPolyline = pl;
                        _selectedPolygon = null;
                        _selectedCircle = null;
                        Highlight(pl);
                        UpdateStatus($"Polilinha selecionada (vértices: {pl.Positions.Count}).");
                    }
                    else
                    {
                        UpdateStatus("Nenhuma polilinha próxima ao toque.");
                    }
                    _awaitingSelectPolyline = false;
                    return;
                }

                if (_awaitingHoleTap && _selectedPolygon != null)
                {
                    int holeIndex = FindNearestHoleIndex(_selectedPolygon, e.Point);
                    if (holeIndex >= 0)
                    {
                        var holeData = await ResolveHoleDataAsync(_selectedPolygon, holeIndex);
                        if (holeData != null)
                        {
                            _edit.StartEditHole(_selectedPolygon, holeIndex, holeData);
                            UpdateStatus($"Editando buraco #{holeIndex + 1}.");
                        }
                        else
                        {
                            await DisplayAlert("Buraco", "Não foi possível localizar o registro do buraco no banco.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Buraco", "Não foi possível identificar um buraco próximo desse toque.", "OK");
                    }
                    _awaitingHoleTap = false;
                }
            }
        private async void OnMapClickedForSelectionOrHole(object sender, MapClickedEventArgs e)
        {
            if (_create.Active) 
                return;

            if (_awaitingSelectPolygon)
            {
                var poly = FindContainingPolygon(e.Point);
                if (poly != null)
                {
                    _selectedPolygon = poly;
                    _selectedPolyline = null;
                    _selectedCircle = null;
                    Highlight(poly);
                    UpdateStatus($"Polígono selecionado (vértices: {poly.Positions.Count}).");
                }
                else
                {
                    UpdateStatus("Nenhum polígono contém esse ponto.");
                }
                _awaitingSelectPolygon = false;
                return;
            }

            if (_awaitingSelectPolyline)
            {
                var pl = FindNearestPolyline(e.Point, out var distMeters);
                if (pl != null && distMeters <= 50)
                {
                    _selectedPolyline = pl;
                    _selectedPolygon = null;
                    _selectedCircle = null;
                    Highlight(pl);
                    UpdateStatus($"Polilinha selecionada (vértices: {pl.Positions.Count})");
                }
                else
                {
                    UpdateStatus("Nenhuma polilinha próxima ao toque.");
                }
                _awaitingSelectPolyline = false;
                return;
            }

            if (_awaitingHoleTap && _selectedPolygon != null)
            {
                int holeIndex = FindNearestHoleIndex(_selectedPolygon, e.Point);
                if (holeIndex >= 0)
                {
                    var holeData = await ResolveHoleDataAsync(_selectedPolygon, holeIndex);
                    if (holeData != null)
                    {
                        _edit.StartEditHole(_selectedPolygon, holeIndex, holeData);
                        UpdateStatus($"Editando buraco #{holeIndex + 1}.");
                    }
                    else
                    {
                        await DisplayAlert("Buraco", "Não foi possível localizar o registro do buraco no banco.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Buraco", "Não foi possível identificar um buraco próximo desse toque.", "OK");
                }
                _awaitingHoleTap = false;
            }
        }

        private void OnPolygonShapeClicked(Polygon pg)
        {
            if (_create.Active) return;

            _selectedPolygon = pg;
            _selectedPolyline = null;
            _selectedCircle = null;
            _selectedPin = null;
            Highlight(pg);
            UpdateStatus($"Polígono selecionado (vértices: {pg.Positions.Count}). Clique em 'Editar selecionado'.");
        }

        private void OnPolylineShapeClicked(Polyline pl)
        {
            if (_create.Active) return;
            _selectedPolyline = pl;
            _selectedPolygon = null;
            _selectedCircle = null;
            _selectedPin = null;
            Highlight(pl);
            UpdateStatus($"Polilinha selecionada (vértices: {pl.Positions.Count}). Clique em 'Editar selecionado'.");
        }

        private void OnCircleShapeClicked(Circle c)
        {
            if (_create.Active) return;
            _selectedCircle = c;
            _selectedPolygon = null;
            _selectedPolyline = null;
            _selectedPin = null;
            Highlight(c);
            UpdateStatus($"Círculo selecionado (raio: {c.Radius.Meters:0} m).");
        }

        private Polygon FindContainingPolygon(Position pt)
        {
            foreach (var polygon in map.Polygons)
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

        private Polyline FindNearestPolyline(Position pt, out double bestDistMeters)
        {
            bestDistMeters = double.PositiveInfinity;
            Polyline best = null;

            foreach (var pl in map.Polylines)
            {
                var d = DistanceToPathMeters(pl.Positions, pt);
                if (d < bestDistMeters) { bestDistMeters = d; best = pl; }
            }
            return best;
        }

        private static double DistanceToPathMeters(IList<Position> verts, Position pt)
        {
            if (verts == null || verts.Count < 2) return double.PositiveInfinity;

            double latRef = pt.Latitude;
            (double px, double py) = ToLocalMeters(pt, latRef);

            double best = double.PositiveInfinity;
            for (int i = 0; i < verts.Count - 1; i++)
            {
                var a = verts[i]; var b = verts[i + 1];
                (double ax, double ay) = ToLocalMeters(a, latRef);
                (double bx, double by) = ToLocalMeters(b, latRef);

                double dist = PointToSegmentDistance(px, py, ax, ay, bx, by);
                if (dist < best) best = dist;
            }
            return best;
        }

        private static (double x, double y) ToLocalMeters(Position p, double latRef)
        {
            const double K = 111320.0;
            double x = p.Longitude * Math.Cos(latRef * Math.PI / 180.0) * K;
            double y = p.Latitude * K;
            return (x, y);
        }

        private static double PointToSegmentDistance(double px, double py, double ax, double ay, double bx, double by)
        {
            double vx = bx - ax, vy = by - ay;
            double wx = px - ax, wy = py - ay;
            double c1 = vx * wx + vy * wy;
            double c2 = vx * vx + vy * vy;
            double t = c2 <= 1e-9 ? 0.0 : Math.Clamp(c1 / c2, 0.0, 1.0);
            double projx = ax + t * vx, projy = ay + t * vy;
            double dx = px - projx, dy = py - projy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private int FindNearestHoleIndex(Polygon parent, Position pt)
        {
            if (parent.Holes == null || parent.Holes.Count == 0) return -1;
            int bestIdx = -1;
            double best = double.PositiveInfinity;
            for (int i = 0; i < parent.Holes.Count; i++)
            {
                var ring = parent.Holes[i];
                if (ring == null || ring.Length < 3) continue;
                var centroid = GetCentroid(ring);
                double d = GeoUtils.DistanceMeters(centroid, pt);
                if (d < best) { best = d; bestIdx = i; }
            }
            return bestIdx;
        }

        private static Position GetCentroid(IList<Position> ring)
        {
            double lat = 0, lon = 0;
            int n = ring.Count;
            for (int i = 0; i < n; i++) { lat += ring[i].Latitude; lon += ring[i].Longitude; }
            return new Position(lat / n, lon / n);
        }

        private async Task<MapData> ResolveHoleDataAsync(Polygon parentPolygon, int holeIndex)
        {
            var db = ServiceHelper.GetService<DatabaseService>();
            var parentData = parentPolygon.Tag as MapData;
            if (parentData == null) return null;

            var all = await db.GetAllMapData();
            var candidates = all.Where(d => d.Type == MapDataTypes.Hole && d.ParentId == parentData.Id).ToList();

            var ring = parentPolygon.Holes[holeIndex];
            var target = GeoUtils.Canonicalize(ring);

            return candidates.FirstOrDefault(h => GeoUtils.Canonicalize(h.GetCoordinates()) == target);
        }

        private void OnTogglePanelClicked(object sender, EventArgs e)
        {
            ToolPanel.IsVisible = !ToolPanel.IsVisible;
            btnTogglePanel.Text = ToolPanel.IsVisible ? "✕" : "☰";
        }

        private void OnShowElementsClicked(object sender, EventArgs e)
        {
            CreationOptions.IsVisible = false;
            EditOptions.IsVisible = false;
            MapElementsList.IsVisible = !MapElementsList.IsVisible;

            if(MapElementsList.IsVisible) PopulateMapElementsListAsync();
        }

        private readonly List<MapData> mapDataList = new List<MapData>(); // Lista para armazenar os dados da lista no menu

        
        private async void PopulateMapElementsListAsync()
        {
            mapDataList.Clear();
            MapElementsListView.ItemsSource = null;

            var all = await db.GetAllMapData();

            foreach (var item in all)
            {
                if (item.Type == MapDataTypes.Polyline)
                {
                    double DistanceInMeters = GeoMath.CalculatePolylineDistance(item.GetCoordinates());
                    item.Name += $": Distância: {DistanceInMeters:F2} m";
                }
                else if (item.Type == MapDataTypes.Polygon)
                {
                    double AreaInMetersSquared = GeoMath.ComputePolygonAreaSquareMeters(item.GetCoordinates());
                    item.Name += $": Área: {GeoMath.FormatAreaHa(AreaInMetersSquared)} hectares, m² {AreaInMetersSquared:F2}";
                }
                else if (item.Type == MapDataTypes.Circle)
                {
                    double AreaInMetersSquared = GeoMath.ComputePolygonAreaSquareMeters(item.GetCoordinates());
                    item.Name += $": Raio: {item.Radius} m";
                }

                mapDataList.Add(item);
            }
            MapElementsListView.ItemsSource = mapDataList;
            MapElementsListView.SelectedItem = null;
            MapElementsList.IsVisible = true;
        }

        private async void OnElementSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (mapDataList.Count == 0)
            {
                await DisplayAlert("Atenção", "A lista de elementos está vazia. Por favor, recarregue a lista.", "OK");
                return;
            }

            if (e.SelectedItem == null)
            {
                return; 
            }

            var selectedItem = e.SelectedItem as MapData;

            if (selectedItem != null && mapDataList.Contains(selectedItem))
            {
                var position = selectedItem.GetCoordinates().FirstOrDefault();
                if (position != null)
                {
                    map.FocusOn(position, 500); 
                }
            }
            else
            {
                MapElementsListView.SelectedItem = null;
            }
        }

        private async void OnConfigClicked(object sender, EventArgs e)
        {

            //TileLayer objTile = null;

            //if (objTile != null) map.TileLayers.Remove(objTile);
            //objTile = TileLayer.FromTileUri((int x, int y, int zoom) =>
            //    new Uri($"https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{zoom}/{y}/{x}"));

            //map.TileLayers.Add(objTile);

            //currentDisable.IsEnabled = true;
            //currentDisable = (Button)sender;
            //currentDisable.IsEnabled = false;
            //map.MapType = MapType.None;  // Defina o tipo de mapa

            //Button currentDisable = buttonRemove;
            //buttonRemove.IsEnabled = false;
            //if (objTile != null) map.TileLayers.Remove(objTile);
            //TileLayer objTile = null;
            //objTile = TileLayer.FromTileUri((int x, int y, int zoom) =>
            //    new Uri($"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png"));
            //objTile.Tag = "OSMTILE"; // Can set any object
            //map.TileLayers.Add(objTile);
            //map.MapType = MapType.None;
            // Mostrar display com algumas configurações simples

            //var pickedCat = await DisplayActionSheet("Ir para…", "Cancelar", null, "Alterar Estilo do Mapa\n");

            //if(pickedCat == "Alterar Estilo do Mapa")
            //{
            //    var style = await DisplayActionSheet("Estilo do Mapa", "Cancelar", null, "Normal", "Satélite", "Terreno", "Híbrido");

            //    if(style == "Normal")
            //    {
            //        map.MapType = MapType.Street;
            //    }
            //    else if(style == "Satélite")
            //    {
            //        map.MapType = MapType.Satellite;
            //    }
            //    else if(style == "Terreno")
            //    {
            //        map.MapType = MapType.Terrain;
            //    }
            //    else if(style == "Híbrido")
            //    {
            //        map.MapType = MapType.Hybrid;
            //    }else map.MapType = MapType.Satellite;

            //    return;
            //}

        }
        private async void OnImportKMLClicked(object sender, EventArgs e)
        {
            var pick = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione um KML",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                { { DevicePlatform.Android, new[] { "application/vnd.google-earth.kml+xml", ".kml" } },
                  { DevicePlatform.iOS, new[] { "public.xml", ".kml" } },
                  { DevicePlatform.WinUI, new[] { ".kml" } } })
            });
            if (pick == null) return;

            //var text = await File.ReadAllTextAsync(pick.FullPath);
            //var groups = GeoParserService.ParseKml(text);

            //var result = await FilePicker.PickAsync(new PickOptions { FileTypes = FilePickerFileType.KML });
            await _vm.ImportKML(pick.FullPath);
            await _vm.LoadAllToMapAsync(map);
        }
    }
}

// File: Editing/EditManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Maui.GoogleMaps;
using Microsoft.Maui.Graphics;
using TerraMarcadaV2.Models;
using TerraMarcadaV2.ViewModels;
using Map = Maui.GoogleMaps.Map;

namespace TerraMarcadaV2.Editing
{
    public enum EditTargetKind { None, Polyline, Polygon, Hole }

    internal sealed class VertexTag
    {
        public bool IsEditHandle { get; init; } = true;
        public int Index { get; set; }
        public EditTargetKind Kind { get; init; }
        public MapData Data { get; init; }                 // Polyline/Polygon/Hole MapData
        public Polygon ParentPolygon { get; init; }        // Para Polygon/Hole
        public Polyline ParentPolyline { get; init; }      // Para Polyline
        public int? HoleIndex { get; init; }               // Para Hole
    }

    public sealed class EditManager : IDisposable
    {
        private readonly Map _map;
        private readonly MapDataViewModel _vm;

        private readonly List<Pin> _handles = new();
        private bool _wired;
        private bool _updating; // evita loops durante drag

        private EditTargetKind _kind = EditTargetKind.None;
        private MapData _data;                 // MapData alvo
        private Polygon _polygon;              // polygon alvo (Polygon ou Hole parent)
        private Polyline _polyline;            // polyline alvo
        private int? _holeIndex;               // se Hole: índice do anel em polygon.Holes
        private bool _closed;                  // polygon/holes com último == primeiro?

        // Modo inserir (clicar no mapa insere vértice no segmento mais próximo)
        public bool InsertMode { get; private set; } = false;

        // Distância limite para permitir inserção no segmento (em metros)
        public double InsertThresholdMeters { get; set; } = 25;

        public EditManager(Map map, MapDataViewModel viewModel)
        {
            _map = map;
            _vm = viewModel;
            WireEvents();
        }

        public void Dispose()
        {
            UnwireEvents();
            ClearHandles();
        }

        #region Public API

        public void StartEditPolygon(Polygon polygon)
        {
            CancelEdit();

            _polygon = polygon;
            _data = polygon?.Tag as MapData ?? throw new InvalidOperationException("Polygon.Tag deve ser MapData.");
            _kind = EditTargetKind.Polygon;

            var list = polygon.Positions.ToList();
            _closed = IsClosed(list);
            BuildHandles(list, mkTag: i => new VertexTag
            {
                Kind = EditTargetKind.Polygon,
                Index = i,
                Data = _data,
                ParentPolygon = _polygon
            });
        }

        public void StartEditPolyline(Polyline polyline)
        {
            CancelEdit();

            _polyline = polyline;
            _data = polyline?.Tag as MapData ?? throw new InvalidOperationException("Polyline.Tag deve ser MapData.");
            _kind = EditTargetKind.Polyline;

            BuildHandles(polyline.Positions.ToList(), mkTag: i => new VertexTag
            {
                Kind = EditTargetKind.Polyline,
                Index = i,
                Data = _data,
                ParentPolyline = _polyline
            });
        }

        /// <summary>
        /// holeIndex = índice do anel em polygon.Holes (0..n-1)
        /// holeData = MapData do buraco (criado ao adicionar o hole, com ParentId preenchido)
        /// </summary>
        public void StartEditHole(Polygon parentPolygon, int holeIndex, MapData holeData)
        {
            CancelEdit();

            _polygon = parentPolygon ?? throw new ArgumentNullException(nameof(parentPolygon));
            _data = holeData ?? throw new ArgumentNullException(nameof(holeData));
            _kind = EditTargetKind.Hole;
            _holeIndex = holeIndex;

            var ring = _polygon.Holes[holeIndex]; // Position[]
            var list = ring.ToList();
            _closed = IsClosed(list);

            BuildHandles(list, mkTag: i => new VertexTag
            {
                Kind = EditTargetKind.Hole,
                Index = i,
                Data = _data,
                ParentPolygon = _polygon,
                HoleIndex = holeIndex
            });
        }

        public void SetInsertMode(bool on) => InsertMode = on;

        public void CancelEdit()
        {
            _kind = EditTargetKind.None;
            _polygon = null;
            _polyline = null;
            _holeIndex = null;
            _data = null;
            _closed = false;
            ClearHandles();
        }

        #endregion

        #region Wiring

        private void WireEvents()
        {
            if (_wired) return;
            _wired = true;

            _map.PinDragging += Map_PinDragging;
            _map.PinDragEnd += Map_PinDragEnd;
            _map.PinClicked += Map_PinClicked;
            _map.MapClicked += Map_MapClicked;
            _map.MapLongClicked += Map_MapLongClicked;
        }

        private void UnwireEvents()
        {
            if (!_wired) return;
            _wired = false;

            _map.PinDragging -= Map_PinDragging;
            _map.PinDragEnd -= Map_PinDragEnd;
            _map.PinClicked -= Map_PinClicked;
            _map.MapClicked -= Map_MapClicked;
            _map.MapLongClicked -= Map_MapLongClicked;
        }

        #endregion

        #region Build / Clear handles

        private void ClearHandles()
        {
            foreach (var h in _handles)
                _map.Pins.Remove(h);
            _handles.Clear();
        }

        private void BuildHandles(List<Position> verts, Func<int, VertexTag> mkTag)
        {
            ClearHandles();

            int logicalCount = verts.Count;
            if (_closed && logicalCount >= 2) logicalCount -= 1; // ignora o último duplicado

            for (int i = 0; i < logicalCount; i++)
            {
                var p = verts[i];
                var pin = new Pin
                {
                    Label = $"v{i + 1}",
                    Position = p,
                    IsDraggable = true,
                    Icon = BitmapDescriptorFactory.DefaultMarker(Colors.Orange),
                    Tag = mkTag(i),
                    ZIndex = 99999
                };
                _handles.Add(pin);
                _map.Pins.Add(pin);
            }
        }

        #endregion

        #region Map events

        private void Map_PinDragging(object sender, PinDragEventArgs e)
        {
            if (_updating) return;
            if (e.Pin?.Tag is not VertexTag v || !v.IsEditHandle) return;

            _updating = true;
            try
            {
                var newPos = e.Pin.Position;

                switch (_kind)
                {
                    case EditTargetKind.Polygon:
                        UpdatePolygonVertex(v.Index, newPos);
                        break;

                    case EditTargetKind.Polyline:
                        UpdatePolylineVertex(v.Index, newPos);
                        break;

                    case EditTargetKind.Hole:
                        if (v.HoleIndex is int hi) UpdateHoleVertex(hi, v.Index, newPos);
                        break;
                }
            }
            finally { _updating = false; }
        }

        private async void Map_PinDragEnd(object sender, PinDragEventArgs e)
        {
            if (e.Pin?.Tag is not VertexTag v || !v.IsEditHandle) return;

            // Persistência no fim do drag
            switch (_kind)
            {
                case EditTargetKind.Polygon:
                    await _vm.SaveFromLiveAsync(_data, _map);
                    break;

                case EditTargetKind.Polyline:
                    await _vm.SaveFromLiveAsync(_data, _map);
                    break;

                case EditTargetKind.Hole:
                    if (v.HoleIndex is int hi)
                    {
                        var ring = _polygon.Holes[hi];
                        await _vm.SaveHoleExplicitAsync(_data, _polygon, ring);
                    }
                    break;
            }
        }

        private async void Map_PinClicked(object sender, PinClickedEventArgs e)
        {
            // Remover vértice ao clicar no pino (com validação de mínimo)
            if (e.Pin?.Tag is not VertexTag v || !v.IsEditHandle) return;

            // Evita que o mapa trate a seleção padrão
            e.Handled = true;

            switch (_kind)
            {
                case EditTargetKind.Polyline:
                    {
                        var verts = _polyline.Positions.ToList();
                        if (verts.Count <= 2) return; // mínimo para polylines
                        verts.RemoveAt(v.Index);
                        _polyline.Positions.Clear();
                        foreach (var p in verts) _polyline.Positions.Add(p);
                        ReindexHandlesAfterRemoval(v.Index);
                        await _vm.SaveFromLiveAsync(_data, _map);
                        break;
                    }
                case EditTargetKind.Polygon:
                    {
                        var verts = _polygon.Positions.ToList();
                        var logical = verts;
                        bool closed = IsClosed(verts);
                        if (closed) logical = verts.Take(verts.Count - 1).ToList();
                        if (logical.Count <= 3) return; // mínimo para polygons (3 vértices únicos)

                        logical.RemoveAt(v.Index);
                        if (closed)
                        {
                            // refaz fechamento
                            verts = new List<Position>(logical);
                            if (verts.Count > 0) verts.Add(verts[0]);
                        }
                        else verts = logical;

                        _polygon.Positions.Clear();
                        foreach (var p in verts) _polygon.Positions.Add(p);

                        RebuildHandlesFromCurrent();
                        await _vm.SaveFromLiveAsync(_data, _map);
                        break;
                    }
                case EditTargetKind.Hole:
                    {
                        if (v.HoleIndex is int hi)
                        {
                            var ring = _polygon.Holes[hi].ToList();
                            var logical = ring;
                            bool closed = IsClosed(ring);
                            if (closed) logical = ring.Take(ring.Count - 1).ToList();
                            if (logical.Count <= 3) return; // mínimo para holes

                            logical.RemoveAt(v.Index);
                            Position[] newRing;
                            if (closed)
                            {
                                var tmp = new List<Position>(logical);
                                tmp.Add(tmp[0]);
                                newRing = tmp.ToArray();
                            }
                            else newRing = logical.ToArray();

                            _polygon.Holes[hi] = newRing;
                            RebuildHandlesFromCurrent(); // reconstrói baseado no novo anel
                            await _vm.SaveHoleExplicitAsync(_data, _polygon, newRing);
                        }
                        break;
                    }
            }
        }

        private async void Map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (!InsertMode) return;
            var pt = e.Point;

            switch (_kind)
            {
                case EditTargetKind.Polyline:
                    {
                        var verts = _polyline.Positions.ToList();
                        var (segIdx, dist) = FindClosestSegment(verts, pt);
                        if (segIdx >= 0 && dist <= InsertThresholdMeters)
                        {
                            verts.Insert(segIdx + 1, pt);
                            _polyline.Positions.Clear();
                            foreach (var p in verts) _polyline.Positions.Add(p);
                            RebuildHandlesFromCurrent();
                            await _vm.SaveFromLiveAsync(_data, _map);
                        }
                        break;
                    }
                case EditTargetKind.Polygon:
                    {
                        var verts = _polygon.Positions.ToList();
                        bool closed = IsClosed(verts);
                        var logical = closed ? verts.Take(verts.Count - 1).ToList() : verts;

                        var (segIdx, dist) = FindClosestSegment(logical, pt);
                        if (segIdx >= 0 && dist <= InsertThresholdMeters)
                        {
                            logical.Insert(segIdx + 1, pt);
                            var final = closed ? logical.Concat(new[] { logical[0] }).ToList() : logical;

                            _polygon.Positions.Clear();
                            foreach (var p in final) _polygon.Positions.Add(p);

                            RebuildHandlesFromCurrent();
                            await _vm.SaveFromLiveAsync(_data, _map);
                        }
                        break;
                    }
                case EditTargetKind.Hole:
                    {
                        if (_holeIndex is int hi)
                        {
                            var ring = _polygon.Holes[hi].ToList();
                            bool closed = IsClosed(ring);
                            var logical = closed ? ring.Take(ring.Count - 1).ToList() : ring;

                            var (segIdx, dist) = FindClosestSegment(logical, pt);
                            if (segIdx >= 0 && dist <= InsertThresholdMeters)
                            {
                                logical.Insert(segIdx + 1, pt);
                                var final = closed ? logical.Concat(new[] { logical[0] }).ToArray() : logical.ToArray();
                                _polygon.Holes[hi] = final;

                                RebuildHandlesFromCurrent();
                                await _vm.SaveHoleExplicitAsync(_data, _polygon, final);
                            }
                        }
                        break;
                    }
            }
        }
        private async void Map_MapClicked(object sender, MapClickedEventArgs e)
        {
            if (!InsertMode) return;
            var pt = e.Point;

            switch (_kind)
            {
                case EditTargetKind.Polyline:
                    {
                        var verts = _polyline.Positions.ToList();
                        var (segIdx, dist) = FindClosestSegment(verts, pt);
                        if (segIdx >= 0 && dist <= InsertThresholdMeters)
                        {
                            verts.Insert(segIdx + 1, pt);
                            _polyline.Positions.Clear();
                            foreach (var p in verts) _polyline.Positions.Add(p);
                            RebuildHandlesFromCurrent();
                            await _vm.SaveFromLiveAsync(_data, _map);
                        }
                        break;
                    }
                case EditTargetKind.Polygon:
                    {
                        var verts = _polygon.Positions.ToList();
                        bool closed = IsClosed(verts);
                        var logical = closed ? verts.Take(verts.Count - 1).ToList() : verts;

                        var (segIdx, dist) = FindClosestSegment(logical, pt);
                        if (segIdx >= 0 && dist <= InsertThresholdMeters)
                        {
                            logical.Insert(segIdx + 1, pt);
                            var final = closed ? logical.Concat(new[] { logical[0] }).ToList() : logical;

                            _polygon.Positions.Clear();
                            foreach (var p in final) _polygon.Positions.Add(p);

                            RebuildHandlesFromCurrent();
                            await _vm.SaveFromLiveAsync(_data, _map);
                        }
                        break;
                    }
                case EditTargetKind.Hole:
                    {
                        if (_holeIndex is int hi)
                        {
                            var ring = _polygon.Holes[hi].ToList();
                            bool closed = IsClosed(ring);
                            var logical = closed ? ring.Take(ring.Count - 1).ToList() : ring;

                            var (segIdx, dist) = FindClosestSegment(logical, pt);
                            if (segIdx >= 0 && dist <= InsertThresholdMeters)
                            {
                                logical.Insert(segIdx + 1, pt);
                                var final = closed ? logical.Concat(new[] { logical[0] }).ToArray() : logical.ToArray();
                                _polygon.Holes[hi] = final;

                                RebuildHandlesFromCurrent();
                                await _vm.SaveHoleExplicitAsync(_data, _polygon, final);
                            }
                        }
                        break;
                    }
            }
        }

        #endregion

        #region Update vertex helpers

        private void UpdatePolylineVertex(int index, Position p)
        {
            var verts = _polyline.Positions;
            if (index < 0 || index >= verts.Count) return;
            verts[index] = p;
        }

        private void UpdatePolygonVertex(int index, Position p)
        {
            var verts = _polygon.Positions.ToList();
            bool closed = IsClosed(verts);
            var logicalCount = closed ? verts.Count - 1 : verts.Count;
            if (index < 0 || index >= logicalCount) return;

            verts[index] = p;
            if (closed)
                verts[verts.Count - 1] = verts[0];

            _polygon.Positions.Clear();
            foreach (var v in verts) _polygon.Positions.Add(v);
        }

        private void UpdateHoleVertex(int holeIndex, int index, Position p)
        {
            var ring = _polygon.Holes[holeIndex];
            if (ring == null || index < 0 || index >= ring.Length) return;

            var list = ring.ToList();
            bool closed = IsClosed(list);
            var logicalCount = closed ? list.Count - 1 : list.Count;

            if (index >= logicalCount) return;
            list[index] = p;
            if (closed)
                list[^1] = list[0];

            _polygon.Holes[holeIndex] = list.ToArray();
        }

        private void ReindexHandlesAfterRemoval(int removedIndex)
        {
            foreach (var h in _handles)
            {
                if (h.Tag is VertexTag v && v.IsEditHandle && v.Index > removedIndex)
                {
                    v.Index -= 1;
                    h.Label = $"v{v.Index + 1}";
                }
            }
            // Atualiza ícones/posições caso necessário
        }

        private void RebuildHandlesFromCurrent()
        {
            switch (_kind)
            {
                case EditTargetKind.Polyline:
                    StartEditPolyline(_polyline);
                    break;
                case EditTargetKind.Polygon:
                    StartEditPolygon(_polygon);
                    break;
                case EditTargetKind.Hole:
                    if (_holeIndex is int hi)
                        StartEditHole(_polygon, hi, _data);
                    break;
            }
        }

        #endregion

        #region Geometry utils

        private static bool IsClosed(IList<Position> verts)
        {
            if (verts == null || verts.Count < 2) return false;
            return GeoUtils.SequenceAlmostEqual(new[] { verts[0] }, new[] { verts[^1] }, epsMeters: 0.01);
        }

        // Converte latitude/longitude para “metros aproximados” num plano local
        private static (double x, double y) ToLocalMeters(Position p, double latRef)
        {
            const double k = 111320.0; // ~ metros por grau
            double x = p.Longitude * Math.Cos(latRef * Math.PI / 180.0) * k;
            double y = p.Latitude * k;
            return (x, y);
        }

        // Retorna índice do segmento mais próximo e distância (m)
        private static (int segIndex, double distMeters) FindClosestSegment(IList<Position> verts, Position point)
        {
            if (verts == null || verts.Count < 2) return (-1, double.PositiveInfinity);

            double latRef = point.Latitude;
            var (px, py) = ToLocalMeters(point, latRef);

            int bestIdx = -1;
            double best = double.PositiveInfinity;

            for (int i = 0; i < verts.Count - 1; i++)
            {
                var a = verts[i];
                var b = verts[i + 1];

                var (ax, ay) = ToLocalMeters(a, latRef);
                var (bx, by) = ToLocalMeters(b, latRef);

                // distância ponto-segmento em 2D
                var vx = bx - ax; var vy = by - ay;
                var wx = px - ax; var wy = py - ay;

                double c1 = vx * wx + vy * wy;
                double c2 = vx * vx + vy * vy;
                double t = c2 <= 1e-9 ? 0.0 : Math.Clamp(c1 / c2, 0.0, 1.0);

                double projx = ax + t * vx;
                double projy = ay + t * vy;

                double dx = px - projx;
                double dy = py - projy;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < best)
                {
                    best = dist;
                    bestIdx = i;
                }
            }
            return (bestIdx, best);
        }

        #endregion
    }
}

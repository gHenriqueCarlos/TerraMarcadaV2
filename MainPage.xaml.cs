using Maui.GoogleMaps;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TerraMarcadaV2.Creation;
using TerraMarcadaV2.Editing;   // EditManager
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
        public MainPage()
        {
            InitializeComponent();

            db = ServiceHelper.GetService<DatabaseService>();

            _vm = new MapDataViewModel();
            _edit = new EditManager(map, _vm);
            _create = new CreationManager(map, _vm)
            {
                OnStatus = s => lblStatus.Text = s
            };

            // Clique no mapa: só usamos aqui para seleção/edição quando NÃO estivermos criando
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

            // carrega tudo do DB para o mapa
            await _vm.LoadAllToMapAsync(map);

            // liga cliques em todos os shapes atuais
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
        // Destaque / seleção visual
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
        // ============== CRIAÇÃO ================
        // =======================================
        private void OnSnapToggled(object sender, ToggledEventArgs e)
        {
            _create.SnapEnabled = e.Value;
            lblStatus.Text = e.Value ? "Snap habilitado." : "Snap desabilitado.";
        }

        private void OnCreatePointClicked(object sender, EventArgs e)
        {
            _create.StartPoint();
            lblStatus.Text = "Criação de PONTO: toque no mapa para posicionar.";
        }

        private async void OnCreatePointTimedClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ponto de Precisão", "Capturando média por 5 a 10 segundos. Mantenha o celular parado.\nClique em 'OK' e aguarde parado e sem mexer no celular.", "OK");
            _create.StartTimedPoint(); 
        }

        private void OnCreatePolylineClicked(object sender, EventArgs e)
        {
            _create.StartPolyline();
            lblStatus.Text = "Criação de LINHA: toque para adicionar vértices, 'Concluir' para finalizar.";
        }

        private void OnCreatePolygonClicked(object sender, EventArgs e)
        {
            _create.StartPolygon();
            lblStatus.Text = "Criação de POLÍGONO: toque para adicionar vértices, 'Concluir' ou toque no primeiro vértice para fechar.";
        }

        private void OnCreateHoleClicked(object sender, EventArgs e)
        {
            _create.StartHole();
            lblStatus.Text = "Criação de BURACO: 1º toque dentro do polígono-alvo; depois adicione os vértices e 'Concluir'.";
        }

        private void OnCreateCircleClicked(object sender, EventArgs e)
        {
            _create.StartCircle();
            lblStatus.Text = "Criação de CÍRCULO: 1º toque define o centro, 2º toque define o raio.";
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
            lblStatus.Text = "Criação cancelada.";
        }

        private void OnMapPinClicked(object sender, PinClickedEventArgs e)
        {
            if (_create.Active) return; // criação tem prioridade no clique3
            //e.Handled = false;
            _selectedPin = e.Pin;
        }

        // Adicionar botão para deletar elementos (no MainPage.xaml)
        private async void OnDeleteSelectedClicked(object sender, EventArgs e)
        {
            if (_create.Active) { lblStatus.Text = "Finalize ou cancele a criação antes de deletar."; return; }

            if (_selectedPolygon != null)
            {
                _vm.RemoveMapDataAsync(_selectedPolygon.Tag as MapData, map);
                lblStatus.Text = "Polígono deletado.";
                _selectedPolygon = null;
                return;
            }
            if (_selectedPolyline != null)
            {
                _vm.RemoveMapDataAsync(_selectedPolyline.Tag as MapData, map);
                lblStatus.Text = "Polilinha deletada.";
                _selectedPolyline = null;
                return;
            }
            if (_selectedCircle != null)
            {
                _vm.RemoveMapDataAsync(_selectedCircle.Tag as MapData, map);
                lblStatus.Text = "Círculo deletado.";
                _selectedCircle = null;
                return;
            }
            if (_selectedPin != null)
            {
                _vm.RemoveMapDataAsync(_selectedPin.Tag as MapData, map);
                lblStatus.Text = "Ponto deletado.";
                _selectedPin = null;
                return;
            }

            lblStatus.Text = "Nenhum elemento selecionado para deletar.";
        }


        // =======================================
        // =============== EDIÇÃO =================
        // =======================================
        private void OnInsertToggled(object sender, ToggledEventArgs e)
        {
            _edit.SetInsertMode(e.Value);
            lblStatus.Text = e.Value
                ? "Inserção de vértices ATIVA. Toque no mapa para inserir no segmento mais próximo."
                : "Inserção de vértices DESATIVADA.";
        }

        private void OnSelectPolygonClicked(object sender, EventArgs e)
        {
            if (_create.Active) { lblStatus.Text = "Finalize/cancele a criação antes de selecionar para edição."; return; }
            _awaitingSelectPolygon = true;
            _awaitingSelectPolyline = false;
            _awaitingHoleTap = false;
            lblStatus.Text = "Toque dentro do polígono para selecioná-lo.";
        }

        private void OnSelectPolylineClicked(object sender, EventArgs e)
        {
            if (_create.Active) { lblStatus.Text = "Finalize/cancele a criação antes de selecionar para edição."; return; }
            _awaitingSelectPolyline = true;
            _awaitingSelectPolygon = false;
            _awaitingHoleTap = false;
            lblStatus.Text = "Toque próximo à polilinha para selecioná-la.";
        }

        private void OnEditSelectedClicked(object sender, EventArgs e)
        {
            if (_create.Active) { lblStatus.Text = "Finalize/cancele a criação antes de editar."; return; }

            if (_selectedPolygon != null)
            {
                _edit.StartEditPolygon(_selectedPolygon);
                lblStatus.Text = "Editando polígono. Arraste os pinos laranja. Clique no pino para remover.";
                return;
            }
            if (_selectedPolyline != null)
            {
                _edit.StartEditPolyline(_selectedPolyline);
                lblStatus.Text = "Editando polilinha. Arraste os pinos laranja. Clique no pino para remover.";
                return;
            }
            DisplayAlert("Selecionar", "Selecione primeiro um polígono ou uma polilinha.", "OK");
        }

        private void OnEditHoleClicked(object sender, EventArgs e)
        {
            if (_create.Active) { lblStatus.Text = "Finalize/cancele a criação antes de editar buraco."; return; }
            if (_selectedPolygon == null)
            {
                DisplayAlert("Editar buraco", "Selecione primeiro o polígono pai.", "OK");
                return;
            }
            _awaitingHoleTap = true;
            _awaitingSelectPolygon = false;
            _awaitingSelectPolyline = false;
            lblStatus.Text = "Toque dentro do buraco desejado para editar.";
        }

        private void OnFinishEditingClicked(object sender, EventArgs e)
        {
            _edit.CancelEdit();
            lblStatus.Text = "Edição concluída.";
            ClearHighlights();
        }

        // Mostrar as opções de Criação
        private void OnCreateClicked(object sender, EventArgs e)
        {
            CreationOptions.IsVisible = !CreationOptions.IsVisible;
            EditOptions.IsVisible = false; // Garantir que o menu de edição não apareça
            MapElementsList.IsVisible = false;
        }

        // Mostrar as opções de Edição
        private void OnEditClicked(object sender, EventArgs e)
        {
            EditOptions.IsVisible = !EditOptions.IsVisible;
            CreationOptions.IsVisible = false; // Garantir que o menu de criação não apareça
            MapElementsList.IsVisible = false;
        }


        // =======================================
        // Clique no mapa: seleção (quando NÃO criando)
        // =======================================

        private async void OnMapLongClicked(object sender, MapLongClickedEventArgs e)

            {
                if (_create.Active) return; // criação tem prioridade no clique

                if (_awaitingSelectPolygon)
                {
                    var poly = FindContainingPolygon(e.Point);
                    if (poly != null)
                    {
                        _selectedPolygon = poly;
                        _selectedPolyline = null;
                        _selectedCircle = null;
                        Highlight(poly);
                        lblStatus.Text = $"Polígono selecionado (vértices: {poly.Positions.Count}).";
                    }
                    else
                    {
                        lblStatus.Text = "Nenhum polígono contém esse ponto.";
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
                        lblStatus.Text = $"Polilinha selecionada (vértices: {pl.Positions.Count}).";
                    }
                    else
                    {
                        lblStatus.Text = "Nenhuma polilinha próxima ao toque.";
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
                            lblStatus.Text = $"Editando buraco #{holeIndex + 1}.";
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
            if (_create.Active) return; // criação tem prioridade no clique

            if (_awaitingSelectPolygon)
            {
                var poly = FindContainingPolygon(e.Point);
                if (poly != null)
                {
                    _selectedPolygon = poly;
                    _selectedPolyline = null;
                    _selectedCircle = null;
                    Highlight(poly);
                    lblStatus.Text = $"Polígono selecionado (vértices: {poly.Positions.Count}).";
                }
                else
                {
                    lblStatus.Text = "Nenhum polígono contém esse ponto.";
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
                    lblStatus.Text = $"Polilinha selecionada (vértices: {pl.Positions.Count}).";
                }
                else
                {
                    lblStatus.Text = "Nenhuma polilinha próxima ao toque.";
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
                        lblStatus.Text = $"Editando buraco #{holeIndex + 1}.";
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

        // =======================================
        // Cliques diretos nos shapes (binder)
        // =======================================
        private void OnPolygonShapeClicked(Polygon pg)
        {
            if (_create.Active) return;

            _selectedPolygon = pg;
            _selectedPolyline = null;
            _selectedCircle = null;
            _selectedPin = null;
            Highlight(pg);
            lblStatus.Text = $"Polígono selecionado (vértices: {pg.Positions.Count}). Clique em 'Editar selecionado'.";
        }

        private void OnPolylineShapeClicked(Polyline pl)
        {
            if (_create.Active) return;
            _selectedPolyline = pl;
            _selectedPolygon = null;
            _selectedCircle = null;
            _selectedPin = null;
            Highlight(pl);
            lblStatus.Text = $"Polilinha selecionada (vértices: {pl.Positions.Count}). Clique em 'Editar selecionado'.";
        }

        private void OnCircleShapeClicked(Circle c)
        {
            if (_create.Active) return;
            _selectedCircle = c;
            _selectedPolygon = null;
            _selectedPolyline = null;
            _selectedPin = null;
            Highlight(c);
            lblStatus.Text = $"Círculo selecionado (raio: {c.Radius.Meters:0} m).";
        }

        // =======================================
        // Helpers de seleção
        // =======================================
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

        private readonly List<MapData> mapDataList = new List<MapData>(); // Lista para armazenar os dados do mapa

        private double CalculatePolylineDistance(List<Position> coordinates)
        {
            double totalDistance = 0;
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var start = coordinates[i];
                var end = coordinates[i + 1];
                totalDistance += Location.CalculateDistance(start.Latitude, start.Longitude, end.Latitude, end.Longitude, DistanceUnits.Kilometers);
            }
            return totalDistance * 1000; // Convertendo para metros
        }

        static double ComputePolygonAreaSquareMeters(List<Position> coordinates)
        {
            // Projeção local (equiretangular) + fórmula do polígono (shoelace)
            if (coordinates == null || coordinates.Count < 3) return 0;

            double lat0Deg = coordinates.Average(v => v.Latitude);
            double lat0Rad = lat0Deg * Math.PI / 180.0;
            double lon0Deg = coordinates.Average(v => v.Longitude);

            // metros por grau na latitude média
            double mPerLat = 111132.954 - 559.822 * Math.Cos(2 * lat0Rad) + 1.175 * Math.Cos(4 * lat0Rad);
            double mPerLon = 111132.954 * Math.Cos(lat0Rad);

            // para precisão, use o anel como está (não precisa repetir o 1º no final)
            var pts = coordinates.Select(v => (
                x: (v.Longitude - lon0Deg) * mPerLon,
                y: (v.Latitude - lat0Deg) * mPerLat
            )).ToArray();

            double s = 0;
            int n = pts.Length;
            for (int i = 0; i < n; i++)
            {
                var (xi, yi) = pts[i];
                var (xj, yj) = pts[(i + 1) % n];
                s += xi * yj - xj * yi;
            }
            return Math.Abs(s) * 0.5; // m²
        }

        //private double CalculatePolygonArea(List<Position> coordinates)
        //{
        //    // Verificar se o polígono tem pelo menos 3 pontos (não pode ser um ponto ou uma linha)
        //    if (coordinates.Count < 3)
        //        return 0;

        //    double area = 0;

        //    // Fórmula de Shoelace para calcular a área de um polígono
        //    for (int i = 0; i < coordinates.Count; i++)
        //    {
        //        int j = (i + 1) % coordinates.Count; // Índice do próximo ponto (circular)
        //        var p1 = coordinates[i];
        //        var p2 = coordinates[j];

        //        // Somar o produto das coordenadas (Longitude * Latitude)
        //        area += p1.Longitude * p2.Latitude;
        //        area -= p1.Latitude * p2.Longitude;
        //    }

        //    area = Math.Abs(area) / 2.0;

        //    // Retorna a área em metros quadrados (considerando a fórmula de área geodésica)
        //    return area;
        //}

        static string FormatAreaHa(double m2)
        {
            double ha = m2 / 10000.0;
            return ha < 1 ? $"{ha:0.###} ha ({m2:0} m²)" : $"{ha:0.##} ha";
        }

        private async void PopulateMapElementsListAsync()
        {
            // Limpar a lista de itens antes de preencher
            mapDataList.Clear();
            MapElementsListView.ItemsSource = null;

            var all = await db.GetAllMapData();

            foreach (var item in all)
            {
                if (item.Type == MapDataTypes.Polyline)
                {
                    // Calcular a distância da polilinha
                    double DistanceInMeters = CalculatePolylineDistance(item.GetCoordinates());

                    // Adiciona as informações de distância no nome com formatação condicional
                    item.Name += $": Distância: {DistanceInMeters:F2} m";
                }
                else if (item.Type == MapDataTypes.Polygon)
                {
                    // Calcular a área do polígono
                    double AreaInMetersSquared = ComputePolygonAreaSquareMeters(item.GetCoordinates());

                    // Adiciona as informações de área no nome
                    item.Name += $": Área: {FormatAreaHa(AreaInMetersSquared)} hectares, m² {AreaInMetersSquared:F2}";
                }
                else if (item.Type == MapDataTypes.Circle)
                {
                    // Calcular a área do polígono
                    double AreaInMetersSquared = ComputePolygonAreaSquareMeters(item.GetCoordinates());

                    // Adiciona as informações de área no nome
                    item.Name += $": Raio: {item.Radius} m";
                }

                mapDataList.Add(item); // Adicionar cada item à lista
            }

            // Atualizar o ItemsSource da ListView com os dados da lista
            MapElementsListView.ItemsSource = mapDataList;

            // Limpar a seleção após a atualização dos itens
            MapElementsListView.SelectedItem = null;

            // Exibir a lista (tornando-a visível)
            MapElementsList.IsVisible = true;
        }

        // Evento chamado quando um item da lista é selecionado
        private async void OnElementSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Verifica se a lista está vazia
            if (mapDataList.Count == 0)
            {
                await DisplayAlert("Atenção", "A lista de elementos está vazia. Por favor, recarregue a lista.", "OK");
                return;
            }

            // Verifica se nenhum item foi selecionado
            if (e.SelectedItem == null)
            {
                return; // Nenhum item selecionado
            }

            // Verifica se o item selecionado é válido
            var selectedItem = e.SelectedItem as MapData;

            // Garantir que o item está na lista antes de tentar utilizá-lo
            if (selectedItem != null && mapDataList.Contains(selectedItem))
            {
                // Foca o mapa na posição do elemento selecionado
                var position = selectedItem.GetCoordinates().FirstOrDefault();
                if (position != null)
                {
                    map.FocusOn(position, 500); // Centraliza o mapa no elemento selecionado
                }
            }
            else
            {
                // Caso o item selecionado não esteja mais na lista, desmarcar a seleção
                MapElementsListView.SelectedItem = null;
            }
        }


        private async void OnConfigClicked(object sender, EventArgs e)
        {
            // Mostrar display com algumas configurações simples

            var pickedCat = await DisplayActionSheet("Ir para…", "Cancelar", null, "Alterar Estilo do Mapa\n");

            if(pickedCat == "Alterar Estilo do Mapa")
            {
                var style = await DisplayActionSheet("Estilo do Mapa", "Cancelar", null, "Normal", "Satélite", "Terreno", "Híbrido");

                if(style == "Normal")
                {
                    map.MapType = MapType.Street;
                }
                else if(style == "Satélite")
                {
                    map.MapType = MapType.Satellite;
                }
                else if(style == "Terreno")
                {
                    map.MapType = MapType.Terrain;
                }
                else if(style == "Híbrido")
                {
                    map.MapType = MapType.Hybrid;
                }else map.MapType = MapType.Satellite;

                return;
            }

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

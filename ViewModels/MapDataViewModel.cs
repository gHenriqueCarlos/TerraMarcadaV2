using Maui.GoogleMaps;
using Microsoft.Maui.ApplicationModel;
using System.Reflection;
using System.Xml.Linq;
using TerraMarcadaV2.Helpers;
using TerraMarcadaV2.Models;
using TerraMarcadaV2.Services;
using Map = Maui.GoogleMaps.Map;

namespace TerraMarcadaV2.ViewModels
{
    public class MapDataViewModel
    {
        private readonly DatabaseService db;
        public MapDataViewModel()
        {
            db = ServiceHelper.GetService<DatabaseService>();
        }
        public async Task LoadAllToMapAsync(Map map)
        {
            var all = await db.GetAllMapData();

            // Limpa o mapa (opcional – comente se não quiser limpar)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                map.Pins.Clear();
                map.Polylines.Clear();
                map.Polygons.Clear();
                map.Circles.Clear();
            });

            try
            {
                // Tenta obter a localização atual do usuário
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    // Foca o mapa na posição do usuário com um zoom de 500 metros
                    map.FocusOn(new Position(location.Latitude, location.Longitude), 500);
                }
                else
                {
                    // Caso não consiga obter a localização, foca no primeiro elemento
                    var firstElement = all.FirstOrDefault();
                    if (firstElement != null)
                    {
                        map.FocusOn(firstElement.GetCoordinates().FirstOrDefault(), 500);
                    }
                }
            }
            catch (Exception ex)
            {
                // Lida com exceções ao tentar obter a localização
                Console.WriteLine($"Erro ao obter a localização: {ex.Message}");
                throw;
            }

            // 1ª passada: desenha tudo, guardando polígonos por Id
            var polygonById = new Dictionary<int, Polygon>();

            foreach (var d in all.Where(x => x.Type != MapDataTypes.Hole))
            {
                switch (d.Type)
                {
                    case MapDataTypes.Pin:
                        {
                            var pos = d.GetCoordinates().FirstOrDefault();
                            if (pos.Latitude == 0 && pos.Longitude == 0) continue;

                            var pin = new Pin
                            {
                                Label = d.Name,
                                Position = pos,
                                IsDraggable = d.IsDraggable,
                                ZIndex = 10,
                                Tag = d.Id // mantém o seu padrão
                            };
                            StyleUtils.ApplyAutoStyle(d);

                            MainThread.BeginInvokeOnMainThread(() => map.Pins.Add(pin));
                            break;
                        }

                    case MapDataTypes.Polyline:
                        {
                            var pl = new Polyline
                            {
                                StrokeColor = d.StrokeColor,
                                StrokeWidth = Math.Max(4f, d.StrokeWidth),
                                IsClickable = true,
                                ZIndex = 9,
                                Tag = d
                            };
                            StyleUtils.ApplyAutoStyle(d);

                            foreach (var p in d.GetCoordinates()) pl.Positions.Add(p);
                            MainThread.BeginInvokeOnMainThread(() => map.Polylines.Add(pl));
                            break;
                        }

                    case MapDataTypes.Polygon:
                        {
                            var pg = new Polygon
                            {
                                StrokeColor = d.StrokeColor,
                                StrokeWidth = Math.Max(3f, d.StrokeWidth),
                                FillColor = d.FillColor,
                                IsClickable = true,
                                ZIndex = 8,
                                Tag = d
                            };
                            StyleUtils.ApplyAutoStyle(d);

                            foreach (var p in d.GetCoordinates()) pg.Positions.Add(p);
                            polygonById[d.Id] = pg;
                            MainThread.BeginInvokeOnMainThread(() => map.Polygons.Add(pg));
                            break;
                        }

                    case MapDataTypes.Circle:
                        {
                            var center = d.GetCoordinates().FirstOrDefault();
                            if (center.Latitude == 0 && center.Longitude == 0) continue;

                            var c = new Circle
                            {
                                Center = center,
                                Radius = Distance.FromMeters(Math.Max(1, d.Radius)),
                                StrokeColor = d.StrokeColor,
                                StrokeWidth = Math.Max(4f, d.StrokeWidth),
                                FillColor = d.FillColor,
                                IsClickable = true,
                                ZIndex = 7,
                                Tag = d
                            };
                            StyleUtils.ApplyAutoStyle(d);

                            MainThread.BeginInvokeOnMainThread(() => map.Circles.Add(c));
                            break;
                        }
                }
            }

            // 2ª passada: aplica HOLES aos polígonos
            foreach (var h in all.Where(x => x.Type == MapDataTypes.Hole))
            {
                var ring = h.GetCoordinates().ToArray();
                if (ring.Length < 3) continue;

                Polygon parent = null;

                if (h.ParentId is int pid && polygonById.TryGetValue(pid, out var pg))
                    parent = pg;
                else
                {
                    // fallback: achar por localização do primeiro vértice
                    var first = ring[0];
                    parent = MapDataViewModel.FindNearestPolygon(new Location(first.Latitude, first.Longitude), map);
                }

                if (parent != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => parent.Holes.Add(ring));
                }
            }
        }

        public static bool IsPointInPolygon(Location p, IList<Location> ring)
        {
            bool inside = false;
            int n = ring.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var pi = ring[i];
                var pj = ring[j];
                bool intersect = ((pi.Latitude > p.Latitude) != (pj.Latitude > p.Latitude)) &&
                                 (p.Longitude < (pj.Longitude - pi.Longitude) * (p.Latitude - pi.Latitude) /
                                  (pj.Latitude - pi.Latitude + double.Epsilon) + pi.Longitude);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        // Procura o polígono mais próximo que contém a posição
        static Polygon FindNearestPolygon(Location position, Map map)
        {

            var polygons = map.Polygons;
            foreach (var polygon in polygons)
            {
                var locationsList = new List<Location>();
                locationsList.AddRange(polygon.Positions.Select(pos => new Location(pos.Latitude, pos.Longitude)));
                //var loc = new Location(polygon.Latitude, polygon.Longitude);
                if (IsPointInPolygon(position, locationsList))
                {
                    return polygon;
                }
            }

            return null;
        }

        public async Task SaveFromLiveAsync(MapData data, Map map)
        {
            if (data == null) return;

            switch (data.Type)
            {
                case MapDataTypes.Pin:
                    {
                        var pin = map.Pins.FirstOrDefault(p =>
                            (p.Tag is int id && id == data.Id) ||
                            (p.Tag is MapData md && md.Id == data.Id));

                        if (pin == null) return;

                        data.SetCoordinates(new List<Position> { pin.Position });
                        await db.UpdateMapData(data);
                        break;
                    }

                case MapDataTypes.Polyline:
                    {
                        var pl = map.Polylines.FirstOrDefault(x => x.Tag is MapData md && md.Id == data.Id);
                        if (pl == null) return;

                        data.SetCoordinates(pl.Positions.ToList());
                        data.StrokeWidth = pl.StrokeWidth;
                        data.StrokeColor = pl.StrokeColor;
                        await db.UpdateMapData(data);
                        break;
                    }

                case MapDataTypes.Polygon:
                    {
                        var pg = map.Polygons.FirstOrDefault(x => x.Tag is MapData md && md.Id == data.Id);
                        if (pg == null) return;

                        data.SetCoordinates(pg.Positions.ToList());
                        data.StrokeWidth = pg.StrokeWidth;
                        data.StrokeColor = pg.StrokeColor;
                        data.FillColor = pg.FillColor;
                        await db.UpdateMapData(data);
                        break;
                    }

                case MapDataTypes.Circle:
                    {
                        var c = map.Circles.FirstOrDefault(x => x.Tag is MapData md && md.Id == data.Id);
                        if (c == null) return;

                        data.SetCoordinates(new List<Position> { c.Center });
                        data.Radius = (float)c.Radius.Meters;
                        data.StrokeWidth = c.StrokeWidth;
                        data.StrokeColor = c.StrokeColor;
                        data.FillColor = c.FillColor;
                        await db.UpdateMapData(data);
                        break;
                    }

                case MapDataTypes.Hole:
                    {
                        // Hole precisa saber qual é o polígono-pai e o anel editado.
                        // Use a função específica abaixo passando o anel atualizado.
                        // Aqui não tem como inferir QUAL anel foi editado sem ajuda.
                        // Exemplo de uso recomendado está mais abaixo.
                        break;
                    }
            }
        }
        public async Task SaveHoleExplicitAsync(MapData holeData, Polygon parentPolygon, IEnumerable<Position> newRing)
        {
            if (holeData == null || parentPolygon == null) return;

            // Garantir ParentId (fundamental para localizar o registro do hole)
            if (holeData.ParentId == null)
            {
                var parentPolygonData = parentPolygon.Tag as MapData;
                if (parentPolygonData == null) return;
                holeData.ParentId = parentPolygonData.Id;
            }

            // Atualiza o MapData do hole com o anel novo e persiste
            var ringList = newRing.ToList();
            holeData.SetCoordinates(ringList);
            await db.UpdateMapData(holeData);
        }

        public async void RemoveMapDataAsync(MapData data, Map map)
        {
            if(data == null)
                return;

            switch (data.Type)
            {
                case MapDataTypes.Pin:
                    var pinToRemove = map.Pins.FirstOrDefault(p => (int)p.Tag == data.Id);
                    if (pinToRemove != null)
                    {
                        map.Pins.Remove(pinToRemove);
                        await db.DeleteMapData(data.Id);
                    }
                    break;
                case MapDataTypes.Polyline:
                    var polylineToRemove = map.Polylines.FirstOrDefault(pl => pl.Tag is MapData md && md.Id == data.Id);
                    if (polylineToRemove != null)
                    {
                        map.Polylines.Remove(polylineToRemove);
                        await db.DeleteMapData(data.Id);
                    }
                    break;
                case MapDataTypes.Polygon:
                    {
                        var polygonToRemove = map.Polygons.FirstOrDefault(pg => pg.Tag is MapData md && md.Id == data.Id);
                        if (polygonToRemove != null)
                        {
                            // 1) DB: remove todos os holes vinculados
                            await db.DeleteHolesByParentIdAsync(data.Id);

                            // 2) UI: remove o polígono (os holes somem juntos)
                            map.Polygons.Remove(polygonToRemove);

                            // 3) DB: remove o próprio polígono
                            await db.DeleteMapData(data.Id);
                        }
                        break;
                    }
                case MapDataTypes.Hole:
                    {
                        if (data.HolePolygon != null)
                        {
                            var target = data.GetCoordinates();

                            // encontra o hole na UI por conteúdo (não por referência)
                            int index = -1;
                            for (int i = 0; i < data.HolePolygon.Holes.Count; i++)
                            {
                                var hole = data.HolePolygon.Holes[i];
                                if (GeoUtils.SequenceAlmostEqual(hole.ToList(), target))
                                {
                                    index = i; break;
                                }
                            }
                            if (index >= 0)
                                data.HolePolygon.Holes.RemoveAt(index);

                            // DB: remove pelo ParentId + coords
                            var parentPolygonData = data.HolePolygon.Tag as MapData;
                            if (parentPolygonData != null)
                                await db.DeleteHoleByParentAndCoordsAsync(parentPolygonData.Id, target);
                            else
                                await db.DeleteMapData(data.Id); // fallback
                        }
                        break;
                    }

                case MapDataTypes.Circle:
                    var circleToRemove = map.Circles.FirstOrDefault(c => c.Tag is MapData md && md.Id == data.Id);
                    if (circleToRemove != null)
                    {
                        map.Circles.Remove(circleToRemove);
                        await db.DeleteMapData(data.Id);
                    }
                    break;
                default:
                    break;
            }
        }
        public async Task<MapData?> AddMapData(MapData data, Map map)
        {
            // Se map for null, só adiciona no DB (sem desenhar)

            // 1) auto estilo (trata null -> cores vibrantes)
            StyleUtils.ApplyAutoStyle(data);

            switch (data.Type)
            {
                case MapDataTypes.Pin:
                    {
                        var pos = data.GetCoordinates().First();
                        var pin = new Pin
                        {
                            Label = data.Name,
                            Position = pos,
                            IsDraggable = data.IsDraggable,
                            ZIndex = 10
                        };
                        await db.AddMapData(data);
                        pin.Tag = data.Id;
                        //pin.Han

                        if(map != null)
                            MainThread.BeginInvokeOnMainThread(() => map.Pins.Add(pin));
                        return data;
                    }

                case MapDataTypes.Polyline:
                    {
                        var polyline = new Polyline
                        {
                            IsClickable = true,
                            StrokeColor = data.StrokeColor ?? Colors.Red,                 // fallback
                            StrokeWidth = Math.Max(4f, data.StrokeWidth),
                            ZIndex = 9,
                            Tag = data
                        };
                        foreach (var c in data.GetCoordinates())
                            polyline.Positions.Add(c);

                        await db.AddMapData(data);
                        if (map != null)
                            MainThread.BeginInvokeOnMainThread(() => map.Polylines.Add(polyline));
                        return data;
                    }

                case MapDataTypes.Polygon:
                    {
                        var polygon = new Polygon
                        {
                            IsClickable = true,
                            StrokeColor = data.StrokeColor ?? Colors.Lime,                // fallback
                            StrokeWidth = Math.Max(3f, data.StrokeWidth),
                            FillColor = data.FillColor ?? Color.FromRgba(0, 255, 0, 64),// fallback
                            ZIndex = 8,
                            Tag = data
                        };
                        foreach (var c in data.GetCoordinates())
                            polygon.Positions.Add(c);

                        await db.AddMapData(data);
                        if (map != null)
                            MainThread.BeginInvokeOnMainThread(() => map.Polygons.Add(polygon));
                        return data;
                    }

                case MapDataTypes.Hole:
                    {
                        if (data.HolePolygon == null)
                        {
                            var first = data.GetCoordinates().First();
                            data.HolePolygon = MapDataViewModel.FindNearestPolygon(
                                new Location(first.Latitude, first.Longitude), map);
                        }
                        if (data.HolePolygon == null) return null;

                        var parentMd = data.HolePolygon.Tag as MapData;
                        if (parentMd == null) return null;
                        data.ParentId = parentMd.Id;

                        var ring = data.GetCoordinates().ToArray();

                        if(map != null)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                data.HolePolygon.Holes.Add(ring);
                            });
                        }

                        await db.AddMapData(data);
                        return data;
                    }

                case MapDataTypes.Circle:
                    {
                        var center = data.GetCoordinates().First();
                        var circle = new Circle
                        {
                            Center = center,
                            Radius = Distance.FromMeters(Math.Max(1, data.Radius)),
                            StrokeColor = data.StrokeColor ?? Colors.Cyan,                // fallback
                            StrokeWidth = Math.Max(4f, data.StrokeWidth),
                            FillColor = data.FillColor ?? Color.FromRgba(0, 255, 255, 48),
                            IsClickable = true,
                            ZIndex = 1,
                            Tag = data
                        };

                        await db.AddMapData(data);
                        if (map != null)
                            MainThread.BeginInvokeOnMainThread(() => map.Circles.Add(circle));
                        return data;
                    }
            }
            return null;
        }
        public async Task ImportKML(string kmlFilePath)
        {
            XDocument doc = XDocument.Load(kmlFilePath);
            var namespaces = doc.Root?.Name.Namespace;
            var placemarks = doc.Descendants(namespaces + "Placemark");

            foreach (var placemark in placemarks)
            {
                var name = placemark.Descendants(namespaces + "name").FirstOrDefault()?.Value;
                var coordinates = placemark.Descendants(namespaces + "coordinates").FirstOrDefault()?.Value;
                var isPolygon = placemark.Descendants(namespaces + "Polygon").Any();  // Verifica se é um polígono

                if (string.IsNullOrEmpty(coordinates)) continue;

                var coordList = coordinates.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Split(','))
                    .Where(p => p.Length >= 2)
                    .Select(p => new Position(double.Parse(p[1]), double.Parse(p[0])))
                    .ToList();

                if (coordList.Count == 1)  // Ponto
                {
                    var data = new MapData
                    {
                        Type = MapDataTypes.Pin,
                        Name = name ?? "Ponto",
                        IsDraggable = true
                    };
                    data.SetCoordinates(new List<Position> { coordList.First() });
                    await AddMapData(data, null);
                }
                else if (coordList.Count >= 2)  // Polilinha ou Polígono
                {
                    // Verifica se é um polígono ou uma polilinha
                    var data = new MapData
                    {
                        Name = name ?? "Forma",
                        StrokeWidth = 5f
                    };

                    if (isPolygon || coordList.First().Equals(coordList.Last()))  // Se for um polígono
                    {
                        data.Type = MapDataTypes.Polygon;
                        data.SetCoordinates(coordList);
                    }
                    else  // Caso contrário, é uma polilinha
                    {
                        data.Type = MapDataTypes.Polyline;
                        data.SetCoordinates(coordList);
                    }

                    await AddMapData(data, null);
                }
            }
        }
    }
}

using Maui.GoogleMaps;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Models
{
    public enum MapDataTypes { Pin = 0, Polyline = 1, Polygon = 2, Hole = 3, Circle = 4 }

    public class MapData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public MapDataTypes Type { get; set; }

        public bool IsVisible { get; set; } = true;

        public int ZIndex { get; set; } = 0;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // FK para buraco -> polígono pai
        public int? ParentId { get; set; }

        // -------------------------
        // CORES: armazenar como HEX
        // -------------------------
        public string? StrokeColorHex { get; set; }   // ex: #FF00FF00 (A R G B)
        public string? FillColorHex { get; set; }

        public float StrokeWidth { get; set; } = 5f;

        [Ignore]
        public Color StrokeColor
        {
            get => FromHexOrDefault(StrokeColorHex, Colors.Black);
            set => StrokeColorHex = ToHex(value);
        }

        [Ignore]
        public Color FillColor
        {
            get => FromHexOrDefault(FillColorHex, Colors.Blue);
            set => FillColorHex = ToHex(value);
        }

        //[Ignore]
        //public double DistanceInMeters { get; set; }
        //[Ignore]
        //public double DistanceInHectares { get; set; }
        //[Ignore]
        //public double AreaInMetersSquared { get; set; }
        //[Ignore]
        //public double AreaInHectares { get; set; }
        //[Ignore]
        //public string 
        
        // -------------------------
        // COORDENADAS: JSON de DTO
        // -------------------------
        public string Coordinates { get; set; } = "[]";

        public void SetCoordinates(IEnumerable<Position> positions)
        {
            var list = positions.Select(p => new CoordDto { Lat = p.Latitude, Lng = p.Longitude }).ToList();
            Coordinates = JsonSerializer.Serialize(list);
        }

        public List<Position> GetCoordinates()
        {
            if (string.IsNullOrWhiteSpace(Coordinates)) return new();

            // 1) caminho feliz: nossa DTO
            try
            {
                var list = JsonSerializer.Deserialize<List<CoordDto>>(Coordinates);
                if (list != null && list.Count > 0)
                    return list.Select(c => new Position(c.Lat, c.Lng)).ToList();
            }
            catch { /* cai pro fallback */ }

            // 2) fallback: tenta ler objetos { Latitude, Longitude } ou { lat, lng }
            try
            {
                using var doc = JsonDocument.Parse(Coordinates);
                var result = new List<Position>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    double lat = TryGet(el, "Lat", "lat", "Latitude");
                    double lng = TryGet(el, "Lng", "lng", "Longitude", "Long");
                    result.Add(new Position(lat, lng));
                }
                return result;
            }
            catch
            {
                return new();
            }
        }

        // Somente para Pin
        public bool IsDraggable { get; set; } = true;

        // Para Circle
        public float Radius { get; set; } = 0f;

        // Para Hole (uso em runtime, não salvar)
        [Ignore]
        public Polygon? HolePolygon { get; set; }

        // Helpers
        private static string? ToHex(Color? c)
        {
            if (c == null) return null;
            byte a = (byte)Math.Clamp(Math.Round(c.Alpha * 255.0), 0, 255);
            byte r = (byte)Math.Clamp(Math.Round(c.Red * 255.0), 0, 255);
            byte g = (byte)Math.Clamp(Math.Round(c.Green * 255.0), 0, 255);
            byte b = (byte)Math.Clamp(Math.Round(c.Blue * 255.0), 0, 255);
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        private static Color FromHexOrDefault(string? hex, Color fallback)
        {
            try { return string.IsNullOrWhiteSpace(hex) ? fallback : Color.FromArgb(hex); }
            catch { return fallback; }
        }

        private static double TryGet(JsonElement el, params string[] keys)
        {
            foreach (var k in keys)
                if (el.TryGetProperty(k, out var v)) return v.GetDouble();
            return 0.0;
        }

        private class CoordDto { public double Lat { get; set; } public double Lng { get; set; } }
    }
    public static class GeoUtils
    {
        // Canonicaliza (ordem original; arredondamento fixo) para comparar strings
        public static string Canonicalize(IEnumerable<Position> coords, int decimals = 6)
        {
            var inv = CultureInfo.InvariantCulture;
            return string.Join(";",
                coords.Select(p =>
                    $"{Math.Round(p.Latitude, decimals).ToString($"F{decimals}", inv)}," +
                    $"{Math.Round(p.Longitude, decimals).ToString($"F{decimals}", inv)}"));
        }

        // “Igualdade” por tolerância métrica ponto a ponto (mesmo tamanho)
        public static bool SequenceAlmostEqual(IList<Position> a, IList<Position> b, double epsMeters = 0.2)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (DistanceMeters(a[i], b[i]) > epsMeters) return false;
            return true;
        }

        public static double DistanceMeters(Position p1, Position p2)
        {
            const double R = 6371000.0;
            double dLat = Deg2Rad(p2.Latitude - p1.Latitude);
            double dLon = Deg2Rad(p2.Longitude - p1.Longitude);
            double lat1 = Deg2Rad(p1.Latitude);
            double lat2 = Deg2Rad(p2.Latitude);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private static double Deg2Rad(double deg) => deg * Math.PI / 180.0;
    }
}

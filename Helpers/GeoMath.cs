using Maui.GoogleMaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Helpers
{
    public class GeoMath
    {

        public static double CalculatePolylineDistance(List<Position> coordinates)
        {
            double totalDistance = 0;
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var start = coordinates[i];
                var end = coordinates[i + 1];
                totalDistance += Location.CalculateDistance(start.Latitude, start.Longitude, end.Latitude, end.Longitude, DistanceUnits.Kilometers);
            }
            return totalDistance * 1000;
        }
        public static double ComputePolygonAreaSquareMeters(List<Position> coordinates)
        {
            if (coordinates == null || coordinates.Count < 3) return 0;

            double lat0Deg = coordinates.Average(v => v.Latitude);
            double lat0Rad = lat0Deg * Math.PI / 180.0;
            double lon0Deg = coordinates.Average(v => v.Longitude);

            // metros por grau na latitude média
            double mPerLat = 111132.954 - 559.822 * Math.Cos(2 * lat0Rad) + 1.175 * Math.Cos(4 * lat0Rad);
            double mPerLon = 111132.954 * Math.Cos(lat0Rad);

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
        public static string FormatAreaHa(double m2)
        {
            double ha = m2 / 10000.0;
            return ha < 1 ? $"{ha:0.###} ha ({m2:0} m²)" : $"{ha:0.##} ha";
        }
    }
}

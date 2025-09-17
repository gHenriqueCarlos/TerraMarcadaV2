using System;
using System.Collections.Generic;
using System.Linq;
using Maui.GoogleMaps;
using TerraMarcadaV2.Models;
using Map = Maui.GoogleMaps.Map;

namespace TerraMarcadaV2.Helpers
{
    public static class MapCameraHelper
    {
        public static void FocusOn(this Map map, IList<Position> pts, double paddingMeters = 150)
        {
            if (map == null || pts == null || pts.Count == 0) return;

            double minLat = pts.Min(p => p.Latitude);
            double maxLat = pts.Max(p => p.Latitude);
            double minLon = pts.Min(p => p.Longitude);
            double maxLon = pts.Max(p => p.Longitude);

            var center = new Position((minLat + maxLat) / 2.0, (minLon + maxLon) / 2.0);

            double maxDist = 0;
            foreach (var p in new[]
            {
                new Position(minLat, minLon),
                new Position(minLat, maxLon),
                new Position(maxLat, minLon),
                new Position(maxLat, maxLon)
            })
            {
                maxDist = Math.Max(maxDist, GeoUtils.DistanceMeters(center, p));
            }

            var radius = Distance.FromMeters(maxDist + paddingMeters);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(center, radius));
        }

        public static void FocusOn(this Map map, Position p, double radiusMeters = 300)
        {
            if (map == null) return;
            map.MoveToRegion(MapSpan.FromCenterAndRadius(p, Distance.FromMeters(radiusMeters)));
        }
    }
}

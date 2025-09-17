// File: Helpers/MapShapeClickBinder.cs
using System;
using System.Collections.Generic;
using Maui.GoogleMaps;
using Map = Maui.GoogleMaps.Map;

namespace TerraMarcadaV2.Helpers
{
    public sealed class MapShapeClickBinder
    {
        private readonly HashSet<Polygon> _polygonsBound = new();
        private readonly HashSet<Polyline> _polylinesBound = new();
        private readonly HashSet<Circle> _circlesBound = new();

        private Action<Polygon>? _onPolygon;
        private Action<Polyline>? _onPolyline;
        private Action<Circle>? _onCircle;

        public void AttachAll(Map map, Action<Polygon> onPolygon, Action<Polyline> onPolyline, Action<Circle> onCircle)
        {
            _onPolygon = onPolygon;
            _onPolyline = onPolyline;
            _onCircle = onCircle;

            // Polygons
            foreach (var pg in map.Polygons)
            {
                if (_polygonsBound.Contains(pg)) continue;
                pg.IsClickable = true;
                pg.Clicked -= Polygon_Clicked; 
                pg.Clicked += Polygon_Clicked;
                _polygonsBound.Add(pg);
            }

            // Polylines
            foreach (var pl in map.Polylines)
            {
                if (_polylinesBound.Contains(pl)) continue;
                pl.IsClickable = true;
                pl.Clicked -= Polyline_Clicked;
                pl.Clicked += Polyline_Clicked;
                _polylinesBound.Add(pl);
            }

            // Circles
            foreach (var c in map.Circles)
            {
                if (_circlesBound.Contains(c)) continue;
                c.IsClickable = true;
                c.Clicked -= Circle_Clicked;
                c.Clicked += Circle_Clicked;
                _circlesBound.Add(c);
            }
        }

        public void DetachAll()
        {
            foreach (var pg in _polygonsBound)
                pg.Clicked -= Polygon_Clicked;
            foreach (var pl in _polylinesBound)
                pl.Clicked -= Polyline_Clicked;
            foreach (var c in _circlesBound)
                c.Clicked -= Circle_Clicked;

            _polygonsBound.Clear();
            _polylinesBound.Clear();
            _circlesBound.Clear();
        }

        private void Polygon_Clicked(object? sender, EventArgs e)
        {
            //sender.Po
            if (sender is Polygon pg) _onPolygon?.Invoke(pg);

        }
        private void Polyline_Clicked(object? sender, EventArgs e)
        {
            if (sender is Polyline pl) _onPolyline?.Invoke(pl);
        }
        private void Circle_Clicked(object? sender, EventArgs e)
        {
            if (sender is Circle c) _onCircle?.Invoke(c);
        }
    }
}

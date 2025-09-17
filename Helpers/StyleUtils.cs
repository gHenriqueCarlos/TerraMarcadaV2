// Helpers/StyleUtils.cs
using System;
using Microsoft.Maui.Graphics;
using TerraMarcadaV2.Models;

namespace TerraMarcadaV2.Helpers
{
    public static class StyleUtils
    {
        private static readonly Color[] StrokePalette = new[]
        {
            Colors.Red, Colors.OrangeRed, Colors.Orange, Colors.Gold,
            Colors.Yellow, Colors.Chartreuse, Colors.Lime, Colors.MediumSpringGreen,
            Colors.Cyan, Colors.DeepSkyBlue, Colors.DodgerBlue, Colors.MediumOrchid,
            Colors.Magenta, Colors.HotPink
        };

        private static readonly Random Rng = new Random();

        public static void ApplyAutoStyle(MapData d)
        {
            if (d == null) return;

            bool strokeUnset =
                d.StrokeWidth <= 0f ||
                d.StrokeColor == null ||                
                d.StrokeColor == Colors.Black;       

            bool fillUnset =
                d.FillColor == null ||                 
                d.FillColor.Alpha <= 0.02f ||
                d.FillColor == Colors.Blue;             

            if (d.StrokeWidth <= 0f)
                d.StrokeWidth = d.Type == MapDataTypes.Polyline ? 6f : 4f;

            if (strokeUnset)
                d.StrokeColor = PickStroke();

            if ((d.Type == MapDataTypes.Polygon || d.Type == MapDataTypes.Circle) && fillUnset)
                d.FillColor = FillFromStroke(d.StrokeColor, alpha: 64);
        }

        private static Color PickStroke() =>
            StrokePalette[Rng.Next(StrokePalette.Length)];

        private static Color FillFromStroke(Color stroke, byte alpha = 64)
        {
            byte r = (byte)Math.Clamp(Math.Round(stroke.Red * 255.0), 0, 255);
            byte g = (byte)Math.Clamp(Math.Round(stroke.Green * 255.0), 0, 255);
            byte b = (byte)Math.Clamp(Math.Round(stroke.Blue * 255.0), 0, 255);
            return Color.FromRgba(r, g, b, alpha);
        }
    }
}

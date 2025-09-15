//using SkiaSharp;
//using System.Globalization;

//namespace TerraMarcadaV2.Services;

//public static class OverlayServiceGeo
//{
//    public static Task<byte[]> EscreverOverlayBasico(
//        byte[] imageBytes,
//        double lat, double lon, DateTimeOffset timestamp,
//        double? accuracy, double? heading, double? speed)
//    {
//        using var input = new MemoryStream(imageBytes);
//        using var original = SKBitmap.Decode(input) ?? throw new Exception("Imagem inválida.");

//        using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
//        var canvas = surface.Canvas;
//        canvas.DrawBitmap(original, 0, 0);

//        var ci = new CultureInfo("pt-BR");
//        string dataStr = timestamp.ToLocalTime().ToString("dd 'de' MMMM 'de' yyyy, 'às' HH:mm:ss", ci);
//        string latDMS = CoordsService.CoordToDMS(lat, true);
//        string lonDMS = CoordsService.CoordToDMS(lon, false);
//        string texto1 = dataStr;
//        string texto2 = $"{latDMS} | {lonDMS}";
//        string texto3 = $"LAT: {lat:F6}  LON: {lon:F6}";
//        string extra = $"±{(accuracy?.ToString("F1") ?? "?")} m" +
//                       (heading.HasValue ? $"  HDG: {heading:F0}°" : "") +
//                       (speed.HasValue ? $"  SPD: {speed * 3.6:F1} km/h" : "");
//        string texto4 = extra.Trim();
//        string texto5 = "Terra Marcada";

//        using var tf = SKTypeface.FromFamilyName("Roboto", SKFontStyle.Bold) ?? SKTypeface.Default;
//        float size = Math.Max(18, original.Width * 0.04f);
//        using var font = new SKFont(tf, size);

//        using var fill = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Fill };
//        using var stroke = new SKPaint { Color = SKColors.Black.WithAlpha(180), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = size * 0.15f };

//        float margin = 20;
//        float y = margin + font.Size;
//        float x = original.Width - margin;

//        void DrawLine(string text)
//        {
//            foreach (var line in text.Split('\n'))
//            {
//                var width = font.MeasureText(line, fill);
//                float px = x - width;
//                canvas.DrawText(line, px, y, SKTextAlign.Left, font, stroke);
//                canvas.DrawText(line, px, y, SKTextAlign.Left, font, fill);
//                y += font.Size + 6;
//            }
//        }

//        DrawLine(texto1);
//        DrawLine(texto2);
//        DrawLine(texto3);
//        DrawLine(texto4);
//        DrawLine(texto5);

//        using var img = surface.Snapshot();
//        using var outMs = new MemoryStream();
//        img.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(outMs);
//        return Task.FromResult(outMs.ToArray());
//    }
//}
using SkiaSharp;
using System.Globalization;

namespace TerraMarcadaV2.Services;

public static class OverlayServiceGeo
{
    public static Task<byte[]> EscreverOverlayBasico(
        byte[] imageBytes,
        double lat, double lon, DateTimeOffset timestamp,
        double? accuracy, double? heading, double? speed)
    {
        using var input = new MemoryStream(imageBytes);
        using var original = SKBitmap.Decode(input) ?? throw new Exception("Imagem inválida.");

        // Criando a superfície onde o texto será desenhado
        using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(original, 0, 0); // Desenha a imagem original

        var ci = new CultureInfo("pt-BR");
        string dataStr = timestamp.ToLocalTime().ToString("dd 'de' MMMM 'de' yyyy, 'às' HH:mm:ss", ci);
        string latDMS = CoordsService.CoordToDMS(lat, true);
        string lonDMS = CoordsService.CoordToDMS(lon, false);
        string texto1 = dataStr;
        string texto2 = $"{latDMS} | {lonDMS}";
        string texto3 = $"LAT: {lat:F6}  LON: {lon:F6}";
        string extra = $"±{(accuracy?.ToString("F1") ?? "?")} m" +
                       (heading.HasValue ? $"  HDG: {heading:F0}°" : "") +
                       (speed.HasValue ? $"  SPD: {speed * 3.6:F1} km/h" : "");
        string texto4 = extra.Trim();
        string texto5 = "Terra Marcada";

        // Definindo o tipo de fonte
        using var tf = SKTypeface.FromFamilyName("Roboto", SKFontStyle.Bold) ?? SKTypeface.Default;
        float size = Math.Max(18, original.Width * 0.04f); // Tamanho da fonte
        using var font = new SKFont(tf, size);

        // Definindo o estilo do texto
        using var fill = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { Color = SKColors.Black.WithAlpha(180), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = size * 0.15f };

        float margin = 20;
        float y = margin + font.Size;
        float x = original.Width - margin;

        // Método para desenhar o texto
        void DrawLine(string text, SKTextAlign align = SKTextAlign.Left)
        {
            foreach (var line in text.Split('\n'))
            {
                var width = font.MeasureText(line, fill);
                float px = (align == SKTextAlign.Left) ? x - width : x; // Ajuste no alinhamento à direita
                canvas.DrawText(line, px, y, SKTextAlign.Left, font, stroke);
                canvas.DrawText(line, px, y, SKTextAlign.Left, font, fill);
                y += font.Size + 6; // Distância entre as linhas
            }
        }

        // Desenhando as linhas de texto
        DrawLine(texto1);
        DrawLine(texto2);
        DrawLine(texto3);
        DrawLine(texto4);
        DrawLine(texto5, SKTextAlign.Right); // Alinhando o último texto à direita

        // Capturando a imagem final com o overlay
        using var img = surface.Snapshot();
        using var outMs = new MemoryStream();
        img.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(outMs);
        return Task.FromResult(outMs.ToArray());
    }
}

using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

internal sealed class KnurlRenderer
{
    private readonly SKPaint _linePaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 0.6f,
        IsAntialias = true,
    };

    public void Draw(SKCanvas canvas, float cx, float cy, DialSizeProfile profile, SKColor accent)
    {
        var bezelR = profile.BezelR;
        var outerR = bezelR + 4f;
        var innerR = bezelR + 1f;
        var lineCount = profile.KnurlCount;
        if (lineCount < 8) lineCount = 8;

        for (int i = 0; i < lineCount; i++)
        {
            var angleDeg = (float)i / lineCount * 360f;
            var angleRad = angleDeg * MathF.PI / 180f;

            var x1 = cx + MathF.Cos(angleRad) * innerR;
            var y1 = cy + MathF.Sin(angleRad) * innerR;
            var x2 = cx + MathF.Cos(angleRad) * outerR;
            var y2 = cy + MathF.Sin(angleRad) * outerR;

            _linePaint.Color = new SKColor(255, 255, 255, 10); // 4% white
            canvas.DrawLine(x1, y1, x2, y2, _linePaint);
        }
    }
}

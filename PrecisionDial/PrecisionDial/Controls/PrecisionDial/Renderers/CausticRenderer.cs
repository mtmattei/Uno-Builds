using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

internal sealed class CausticRenderer
{
    private readonly SKPaint _causticPaint = new()
    {
        IsAntialias = true,
    };

    public void Draw(SKCanvas canvas, float cx, float cy, float knobR)
    {
        // Fixed light source at -40 degrees from top (does NOT rotate with knob)
        var lightAngleRad = -40f * MathF.PI / 180f;
        var highlightX = cx + MathF.Cos(lightAngleRad) * knobR * 0.35f;
        var highlightY = cy + MathF.Sin(lightAngleRad) * knobR * 0.35f;

        _causticPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(highlightX, highlightY),
            knobR * 0.6f,
            new[] { new SKColor(255, 255, 255, 18), SKColors.Transparent }, // 7% peak
            SKShaderTileMode.Clamp);

        canvas.DrawCircle(cx, cy, knobR, _causticPaint);
    }
}

using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

/// <summary>
/// Radial-line brushed-metal texture across the knob face. Drawn after the knob fill,
/// inside the rotated knob group so the lines spin with the dial.
/// Forms the base layer that ConeLightRenderer reuses (clipped + brightened) in menu mode.
///
/// The 72 alpha values are precomputed once (they depend only on the line
/// index), and the paint reuses a single SKColor struct — no per-frame color
/// allocation in the loop.
/// </summary>
internal sealed class BrushedMetalRenderer
{
    private const int LineCount = 72;

    private readonly SKPaint _linePaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 0.6f,
        IsAntialias = true,
    };

    private readonly byte[] _alphaTable = new byte[LineCount];
    private readonly float[] _cosTable = new float[LineCount];
    private readonly float[] _sinTable = new float[LineCount];

    public BrushedMetalRenderer()
    {
        for (int i = 0; i < LineCount; i++)
        {
            var a = (i / (float)LineCount) * MathF.PI * 2f;
            _cosTable[i] = MathF.Cos(a);
            _sinTable[i] = MathF.Sin(a);
            _alphaTable[i] = (byte)Math.Clamp(5f + MathF.Sin(a * 3f) * 2.5f, 1f, 12f);
        }
    }

    /// <summary>
    /// Draws ~72 radial lines from inner radius to near the knob edge.
    /// Caller must apply the knob rotation transform before calling so lines rotate with the knob.
    /// </summary>
    public void Draw(SKCanvas canvas, float cx, float cy, float knobR)
    {
        var innerR = knobR * 0.20f;
        var outerR = knobR * 0.97f;

        for (int i = 0; i < LineCount; i++)
        {
            // Reuse a single SKColor struct — construct from the RGB constant
            // + the precomputed alpha value, no heap allocation.
            _linePaint.Color = new SKColor(255, 255, 255, _alphaTable[i]);

            var cos = _cosTable[i];
            var sin = _sinTable[i];
            canvas.DrawLine(
                cx + cos * innerR, cy + sin * innerR,
                cx + cos * outerR, cy + sin * outerR,
                _linePaint);
        }
    }
}

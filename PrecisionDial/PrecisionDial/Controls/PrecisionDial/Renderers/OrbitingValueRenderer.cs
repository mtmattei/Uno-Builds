using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

/// <summary>
/// Draws the live value as text positioned at the endpoint of the last active arc segment.
/// As the value changes, the text physically orbits the dial.
/// Hidden below the orbiting-value size threshold (handled by caller).
/// </summary>
internal sealed class OrbitingValueRenderer
{
    private readonly SKPaint _paint = new()
    {
        IsAntialias = true,
    };

    private readonly SKFont _font = new()
    {
        Typeface = SKTypeface.FromFamilyName(
            "Segoe UI",
            SKFontStyleWeight.SemiBold,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright),
    };

    // Cached formatted value — avoids allocating a new string on every frame
    // while the value is stable. Only reformats when the numeric value or
    // the format condition (F1 vs F0 based on size) changes.
    private string _cachedText = string.Empty;
    private double _cachedValue = double.NaN;
    private bool _cachedFormatIsF1;

    public void Draw(
        SKCanvas canvas,
        ReadOnlySpan<SegmentData> segs,
        int activeCount,
        float cx,
        float cy,
        float arcR,
        float size,
        double value,
        float normalized,
        SKColor accent)
    {
        if (segs.Length == 0) return;

        // Position at end of last active segment, or arc start if 0.
        float angleDeg;
        if (activeCount > 0)
        {
            var idx = Math.Min(activeCount, segs.Length) - 1;
            angleDeg = segs[idx].EndAngle;
        }
        else
        {
            angleDeg = segs[0].StartAngle;
        }

        var angleRad = angleDeg * MathF.PI / 180f;
        var textR = arcR + size * 0.065f;
        var x = cx + MathF.Cos(angleRad) * textR;
        var y = cy + MathF.Sin(angleRad) * textR;

        _font.Size = MathF.Max(8f, size * 0.04f);

        // Vertically center the text on the position by offsetting half the cap height.
        var metrics = _font.Metrics;
        var yBaseline = y - (metrics.Ascent + metrics.Descent) * 0.5f;

        var clamped = Math.Clamp(normalized, 0f, 1f);
        var alpha = (byte)Math.Clamp(140 + clamped * 115f, 0f, 255f);
        _paint.Color = accent.WithAlpha(alpha);

        // Reformat the number only when the value or the format spec changes.
        var wantF1 = size >= 100f;
        if (wantF1 != _cachedFormatIsF1 || value != _cachedValue)
        {
            _cachedText = value.ToString(
                wantF1 ? "F1" : "F0",
                System.Globalization.CultureInfo.InvariantCulture);
            _cachedValue = value;
            _cachedFormatIsF1 = wantF1;
        }

        canvas.DrawText(_cachedText, x, yBaseline, SKTextAlign.Center, _font, _paint);
    }
}

using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PrecisionDial.Controls;

/// <summary>
/// Draws each menu item's Icon glyph at its arc segment midpoint, offset outward.
/// Selected icon scales up. Caller is responsible for the visibility threshold.
///
/// Icons render through the bundled Phosphor Icons TTF (loaded once via
/// <see cref="PhosphorFont"/>). All icon codepoints used by the app are in
/// Phosphor's PUA range (U+E000…U+F8FF), so the single typeface covers every
/// glyph on every platform with no per-character font fallback needed.
/// </summary>
internal sealed class MenuIconRenderer
{
    private readonly SKPaint _paint = new()
    {
        IsAntialias = true,
    };

    // Dedicated glow paint + cached blur mask filter for the selected icon's
    // halo — matches the selected arc segment's glow treatment.
    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
    };
    private readonly SKMaskFilter _glowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 7f);

    private readonly SKFont _font = new()
    {
        Typeface = PhosphorFont.Instance ?? SKTypeface.Default,
    };

    public void Draw(
        SKCanvas canvas,
        ReadOnlySpan<SegmentData> segs,
        IList<DialMenuItem>? items,
        int selectedIndex,
        float cx,
        float cy,
        float arcR,
        float size,
        SKColor accent,
        float rotationOffsetDeg = 0f,
        float liftProgress = 1f)
    {
        if (items is null || items.Count == 0 || segs.Length == 0) return;

        // Per brief §06: icons sit just outside the arc. The room to fit them
        // is reserved by the canvas (menu mode shrinks the dial geometry so
        // arcR leaves headroom outside it). See PrecisionDialCanvas.RenderMenu.
        //
        // rotationOffsetDeg shifts each icon's *position* around the dial by
        // the given angle but keeps the glyph upright in screen space.
        //
        // liftProgress (0..1) controls the shelved state: at 0 only the
        // selected icon is visible and it scales down to a resting size; at 1
        // all icons are visible at full scale.
        var androidBump = OperatingSystem.IsAndroid() ? 1.20f : 1.0f;
        var baseFont = MathF.Max(12.6f, size * 0.0765f) * androidBump;
        var iconR = arcR + size * 0.095f;
        var inactiveColorBase = new SKColor(255, 255, 255, 165); // ~65% white

        // Selected icon scale: lerps from 0.85 (resting) to 1.2 (lifted/active).
        var selectedScale = 0.85f + (1.2f - 0.85f) * Math.Clamp(liftProgress, 0f, 1f);

        var n = Math.Min(items.Count, segs.Length);
        for (int i = 0; i < n; i++)
        {
            var item = items[i];
            if (item is null || string.IsNullOrEmpty(item.Icon)) continue;

            var isSelected = i == selectedIndex;

            // Non-selected icons fade out entirely at progress 0 (fully shelved).
            // Fade slightly faster than the lift so they clear before the dial
            // finishes its drop — gives the resting state a clean silhouette.
            if (!isSelected)
            {
                var neighborAlpha = Math.Clamp(liftProgress * 1.4f - 0.2f, 0f, 1f);
                if (neighborAlpha <= 0.002f) continue;

                _font.Size = baseFont;
                _paint.Color = inactiveColorBase.WithAlpha((byte)(165 * neighborAlpha));
            }
            else
            {
                _font.Size = baseFont * selectedScale;
                _paint.Color = accent.WithAlpha(255);
            }

            var seg = segs[i];
            var rotatedAngleDeg = seg.MidAngle + rotationOffsetDeg;
            var angleRad = rotatedAngleDeg * MathF.PI / 180f;
            var x = cx + MathF.Cos(angleRad) * iconR;
            var y = cy + MathF.Sin(angleRad) * iconR;

            var metrics = _font.Metrics;
            var yBaseline = y - (metrics.Ascent + metrics.Descent) * 0.5f;

            // Glow pass for the selected icon — matches the selected arc
            // segment's halo treatment. Drawn underneath the sharp glyph.
            if (isSelected)
            {
                _glowPaint.MaskFilter = _glowFilter;
                _glowPaint.Color = accent.WithAlpha(180);
                canvas.DrawText(item.Icon, x, yBaseline, SKTextAlign.Center, _font, _glowPaint);
                _glowPaint.MaskFilter = null;
            }

            canvas.DrawText(item.Icon, x, yBaseline, SKTextAlign.Center, _font, _paint);
        }
    }
}

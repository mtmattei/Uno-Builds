using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

/// <summary>
/// One precomputed arc segment: start, end, midpoint angle in degrees (screen frame).
/// </summary>
internal readonly struct SegmentData
{
    public readonly float StartAngle;
    public readonly float EndAngle;
    public readonly float MidAngle;

    public SegmentData(float startAngle, float endAngle, float midAngle)
    {
        StartAngle = startAngle;
        EndAngle = endAngle;
        MidAngle = midAngle;
    }
}

/// <summary>
/// v3 dashed arc — N individual rounded-cap arc segments with progressive opacity.
/// Replaces the v2 single-stroke ArcRenderer + 20-segment bar.
///
/// Path geometry and per-segment SKPath objects are cached inside
/// <see cref="EnsureSegments"/> and <see cref="EnsureGeometry"/>. The render
/// passes just update stroke width / color and draw cached paths — no
/// allocations per frame, which matters a lot at 30–40 segments × 60 fps.
/// </summary>
internal sealed class DashedArcRenderer
{
    private const int MaxSegments = 128;

    private SegmentData[] _segments = Array.Empty<SegmentData>();
    private SKPath?[] _segmentPaths = Array.Empty<SKPath>();
    private int _segmentCount;
    private float _arcSweep;
    private float _startAngle;
    private float _gapDeg;

    // Geometry used to build the cached paths; rebuild only when cx/cy/arcR change.
    private float _pathCx = float.NaN;
    private float _pathCy = float.NaN;
    private float _pathArcR = float.NaN;

    private readonly SKPaint _inactivePaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeCap = SKStrokeCap.Round,
        IsAntialias = true,
        Color = new SKColor(255, 255, 255, 8), // ~3% white
    };

    private readonly SKPaint _activePaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeCap = SKStrokeCap.Round,
        IsAntialias = true,
    };

    private readonly SKPaint _glowPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeCap = SKStrokeCap.Round,
        IsAntialias = true,
    };

    // Value-mode glow blur cache — recreated only when the blur sigma changes.
    private SKMaskFilter? _valueGlowFilter;
    private float _valueGlowFilterSigma = -1f;

    // Menu-mode selected-segment glow — constant 8dp blur, created once.
    private readonly SKMaskFilter _menuSelectedGlowFilter =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8f);

    /// <summary>Read-only access to the precomputed segment list (for orbiting value, menu icons, etc).</summary>
    public ReadOnlySpan<SegmentData> Segments => _segments.AsSpan(0, _segmentCount);
    public int SegmentCount => _segmentCount;
    public float StartAngle => _startAngle;

    /// <summary>
    /// Recomputes the segment angle list. Called when segment count, arc sweep,
    /// start angle or gap changes. Also invalidates cached per-segment paths
    /// because those depend on these angles.
    /// </summary>
    public void EnsureSegments(int count, float arcSweep, float startAngle, float gapDeg)
    {
        if (count == _segmentCount &&
            MathF.Abs(arcSweep - _arcSweep) < 0.001f &&
            MathF.Abs(startAngle - _startAngle) < 0.001f &&
            MathF.Abs(gapDeg - _gapDeg) < 0.001f)
        {
            return;
        }

        if (count < 1) count = 1;
        if (count > MaxSegments) count = MaxSegments;

        _segmentCount = count;
        _arcSweep = arcSweep;
        _startAngle = startAngle;
        _gapDeg = gapDeg;

        if (_segments.Length < count)
            _segments = new SegmentData[count];

        var totalGap = gapDeg * (count - 1);
        var segDeg = (arcSweep - totalGap) / count;

        for (int i = 0; i < count; i++)
        {
            var s = startAngle + i * (segDeg + gapDeg);
            var e = s + segDeg;
            _segments[i] = new SegmentData(s, e, (s + e) * 0.5f);
        }

        // Segment angles changed — invalidate path cache (next EnsureGeometry
        // call will rebuild).
        _pathCx = float.NaN;
    }

    /// <summary>
    /// Builds (or rebuilds) the cached per-segment <see cref="SKPath"/> list
    /// for the supplied geometry. Only rebuilds when the geometry or segment
    /// layout changes, which is rare.
    /// </summary>
    private void EnsureGeometry(float cx, float cy, float arcR)
    {
        if (_pathCx == cx && _pathCy == cy && _pathArcR == arcR) return;

        _pathCx = cx;
        _pathCy = cy;
        _pathArcR = arcR;

        // Grow the path cache to match segment count.
        if (_segmentPaths.Length < _segmentCount)
        {
            var newArr = new SKPath?[_segmentCount];
            Array.Copy(_segmentPaths, newArr, _segmentPaths.Length);
            _segmentPaths = newArr;
        }

        var rect = new SKRect(cx - arcR, cy - arcR, cx + arcR, cy + arcR);
        for (int i = 0; i < _segmentCount; i++)
        {
            var seg = _segments[i];
            var sweep = seg.EndAngle - seg.StartAngle;

            var p = _segmentPaths[i];
            if (p is null)
            {
                p = new SKPath();
                _segmentPaths[i] = p;
            }
            else
            {
                p.Reset();
            }
            p.AddArc(rect, seg.StartAngle, sweep);
        }
    }

    /// <summary>
    /// Draw the value-mode dashed arc: inactive baseline + progressive active fill + last-5 glow.
    /// </summary>
    public void DrawValueMode(
        SKCanvas canvas,
        float cx,
        float cy,
        float arcR,
        SKColor accent,
        SKColor hotAccent,
        int activeCount,
        float inactiveStroke,
        float activeStroke,
        float velocityScale)
    {
        EnsureGeometry(cx, cy, arcR);

        _inactivePaint.StrokeWidth = inactiveStroke;
        _activePaint.StrokeWidth = activeStroke;
        _glowPaint.StrokeWidth = activeStroke * 2.2f;

        // Pass 1: inactive baseline — all cached paths
        for (int i = 0; i < _segmentCount; i++)
        {
            canvas.DrawPath(_segmentPaths[i]!, _inactivePaint);
        }

        if (activeCount <= 0) return;
        if (activeCount > _segmentCount) activeCount = _segmentCount;

        // Pass 2: trailing glow (last few active segments)
        var glowStart = Math.Max(0, activeCount - 5);
        var glowBlur = 4f + Math.Clamp(velocityScale, 0f, 1f) * 8f;

        // Recreate the blur mask only when the sigma actually changes.
        if (_valueGlowFilter is null || MathF.Abs(glowBlur - _valueGlowFilterSigma) > 0.25f)
        {
            _valueGlowFilter?.Dispose();
            _valueGlowFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, glowBlur);
            _valueGlowFilterSigma = glowBlur;
        }

        _glowPaint.MaskFilter = _valueGlowFilter;
        for (int i = glowStart; i < activeCount; i++)
        {
            var pos = (float)i / Math.Max(1, _segmentCount - 1);
            var inHot = pos >= 0.8f;
            var baseColor = inHot ? hotAccent : accent;
            _glowPaint.Color = baseColor.WithAlpha((byte)(60 + (i - glowStart) * 25));
            canvas.DrawPath(_segmentPaths[i]!, _glowPaint);
        }
        _glowPaint.MaskFilter = null;

        // Pass 3: active fill with progressive opacity
        for (int i = 0; i < activeCount; i++)
        {
            var pos = _segmentCount > 1 ? (float)i / (_segmentCount - 1) : 0f;
            var inHot = pos >= 0.8f;

            float alpha;
            SKColor color;
            if (inHot)
            {
                var hotPos = (pos - 0.8f) / 0.2f;
                alpha = 0.60f + hotPos * 0.40f;
                color = hotAccent;
            }
            else
            {
                var normPos = pos / 0.8f;
                alpha = 0.30f + normPos * 0.60f;
                color = accent;
            }

            _activePaint.Color = color.WithAlpha((byte)(alpha * 255f));
            canvas.DrawPath(_segmentPaths[i]!, _activePaint);
        }
    }

    /// <summary>
    /// Draw the menu-mode dashed arc:
    /// inactive (4% white), confirmed (25% accent — items prior to selection),
    /// selected (85% accent — currently active item).
    /// </summary>
    public void DrawMenuMode(
        SKCanvas canvas,
        float cx,
        float cy,
        float arcR,
        SKColor accent,
        int selectedIndex,
        float inactiveStroke,
        float activeStroke)
    {
        EnsureGeometry(cx, cy, arcR);

        _inactivePaint.StrokeWidth = inactiveStroke;
        _activePaint.StrokeWidth = activeStroke;
        _glowPaint.StrokeWidth = activeStroke * 2.4f;

        var confirmedColor = accent.WithAlpha((byte)(0.25f * 255f));
        var selectedColor = accent.WithAlpha((byte)(0.85f * 255f));
        _inactivePaint.Color = new SKColor(255, 255, 255, 10); // 4% white per brief

        for (int i = 0; i < _segmentCount; i++)
        {
            var path = _segmentPaths[i]!;
            if (i == selectedIndex)
            {
                _activePaint.Color = selectedColor;
                canvas.DrawPath(path, _activePaint);
            }
            else if (i < selectedIndex)
            {
                _activePaint.Color = confirmedColor;
                canvas.DrawPath(path, _activePaint);
            }
            else
            {
                canvas.DrawPath(path, _inactivePaint);
            }
        }

        // Selected segment glow — reuse the cached 8dp blur filter.
        if (selectedIndex >= 0 && selectedIndex < _segmentCount)
        {
            _glowPaint.MaskFilter = _menuSelectedGlowFilter;
            _glowPaint.Color = accent.WithAlpha(140);
            canvas.DrawPath(_segmentPaths[selectedIndex]!, _glowPaint);
            _glowPaint.MaskFilter = null;
        }
    }

    /// <summary>
    /// Returns the active-segment count for a normalized 0..1 value.
    /// </summary>
    public int ActiveCountForNormalized(float normalized)
    {
        if (_segmentCount == 0) return 0;
        var raw = normalized * _segmentCount;
        var n = (int)MathF.Round(raw);
        if (n < 0) n = 0;
        if (n > _segmentCount) n = _segmentCount;
        return n;
    }
}

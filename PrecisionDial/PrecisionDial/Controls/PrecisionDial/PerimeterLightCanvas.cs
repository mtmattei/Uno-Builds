using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace PrecisionDial.Controls;

/// <summary>
/// Ambient perimeter light: a rounded-rect line around the edge of its host
/// container with a bright amber "comet" continuously traveling along the path.
/// Self-invalidates every frame so it animates without an external timer —
/// but only while it is actually visible and has non-zero opacity, otherwise
/// the control stops redrawing and costs nothing.
///
/// Designed to match the rest of the PrecisionDial aesthetic — same accent
/// brush, same SkiaSharp pipeline, same warm-amber glow language.
/// </summary>
public sealed class PerimeterLightCanvas : SKCanvasElement
{
    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(
            nameof(AccentBrush),
            typeof(Brush),
            typeof(PerimeterLightCanvas),
            new PropertyMetadata(null));

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>
    /// Time in seconds for the comet to complete one full lap of the perimeter.
    /// </summary>
    public static readonly DependencyProperty CycleSecondsProperty =
        DependencyProperty.Register(
            nameof(CycleSeconds),
            typeof(double),
            typeof(PerimeterLightCanvas),
            new PropertyMetadata(11.0));

    public double CycleSeconds
    {
        get => (double)GetValue(CycleSecondsProperty);
        set => SetValue(CycleSecondsProperty, value);
    }

    private readonly long _startTick = Stopwatch.GetTimestamp();

    private readonly SKPaint _basePaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f,
        IsAntialias = true,
        Color = new SKColor(255, 255, 255, 12), // ~5% white — constant, cached
    };

    private readonly SKPaint _trailPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    private readonly SKPaint _headGlowPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    private readonly SKPaint _headCorePaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
        Color = new SKColor(255, 240, 200, 230), // constant bright core
    };

    // Cached blur mask filters — built once, reused forever (no per-frame alloc).
    private readonly SKMaskFilter _glowInnerBlur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8f);
    private readonly SKMaskFilter _glowOuterBlur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 16f);

    // Geometry cache — rebuilt only when the control is resized, not per frame.
    private float _cachedWidth = -1;
    private float _cachedHeight = -1;
    private SKPath? _cachedPath;
    private SKPathMeasure? _cachedMeasure;
    private float _cachedPerimeter;

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        var width = (float)area.Width;
        var height = (float)area.Height;
        if (width <= 0 || height <= 0) return;

        // If the control is invisible or fully transparent, don't render and
        // don't self-invalidate — the animation effectively pauses.
        if (Visibility != Visibility.Visible || Opacity <= 0.001)
        {
            return;
        }

        EnsureGeometry(width, height);
        if (_cachedPath is null || _cachedMeasure is null || _cachedPerimeter <= 0) return;

        var accent = ResolveAccentColor();

        // ── Base perimeter line — constant color, cached paint ────────────────
        canvas.DrawPath(_cachedPath, _basePaint);

        // ── Compute current phase along the path ──────────────────────────────
        var elapsed = Stopwatch.GetElapsedTime(_startTick).TotalSeconds;
        var cycle = Math.Max(1.0, CycleSeconds);
        var phase = (float)((elapsed % cycle) / cycle);
        var headDistance = phase * _cachedPerimeter;

        // ── Trail: ~30 small dots receding back along the path with falloff ──
        const int trailCount = 30;
        const float trailSpacing = 6f;
        for (int i = trailCount - 1; i >= 0; i--)
        {
            var d = headDistance - i * trailSpacing;
            if (d < 0) d += _cachedPerimeter;
            _cachedMeasure.GetPosition(d, out var p);

            var t = 1f - (float)i / trailCount;
            var alpha = (byte)(t * t * 90f);
            var radius = 0.6f + t * 1.8f;
            _trailPaint.Color = accent.WithAlpha(alpha);
            canvas.DrawCircle(p.X, p.Y, radius, _trailPaint);
        }

        // ── Head: bright core + soft glow halo — cached mask filters ─────────
        _cachedMeasure.GetPosition(headDistance, out var head);

        _headGlowPaint.MaskFilter = _glowInnerBlur;
        _headGlowPaint.Color = accent.WithAlpha(170);
        canvas.DrawCircle(head.X, head.Y, 6f, _headGlowPaint);

        _headGlowPaint.MaskFilter = _glowOuterBlur;
        _headGlowPaint.Color = accent.WithAlpha(60);
        canvas.DrawCircle(head.X, head.Y, 14f, _headGlowPaint);

        // Clear before drawing the un-blurred core so the mask filter doesn't
        // bleed into it.
        _headGlowPaint.MaskFilter = null;
        canvas.DrawCircle(head.X, head.Y, 1.6f, _headCorePaint);

        // Schedule the next frame only when still visible + animating.
        Invalidate();
    }

    private void EnsureGeometry(float width, float height)
    {
        if (_cachedPath is not null &&
            MathF.Abs(width - _cachedWidth) < 0.5f &&
            MathF.Abs(height - _cachedHeight) < 0.5f)
        {
            return;
        }

        _cachedWidth = width;
        _cachedHeight = height;

        _cachedPath?.Dispose();
        _cachedMeasure?.Dispose();

        const float inset = 14f;
        const float corner = 28f;
        var rect = new SKRect(inset, inset, width - inset, height - inset);

        _cachedPath = new SKPath();
        _cachedPath.AddRoundRect(rect, corner, corner);

        _cachedMeasure = new SKPathMeasure(_cachedPath, false);
        _cachedPerimeter = _cachedMeasure.Length;
    }

    private SKColor ResolveAccentColor()
    {
        if (AccentBrush is SolidColorBrush solidBrush)
        {
            var c = solidBrush.Color;
            return new SKColor(c.R, c.G, c.B, c.A);
        }
        return new SKColor(212, 169, 89, 255);
    }
}

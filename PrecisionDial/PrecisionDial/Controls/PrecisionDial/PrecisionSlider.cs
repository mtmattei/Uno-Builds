using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace PrecisionDial.Controls;

/// <summary>
/// A square-format version of PrecisionDial: same materials, same arc/ticks/indicator,
/// but rendered inside a rounded-rectangle (squircle) shape.
/// Interaction: vertical drag — up increases value, down decreases.
/// </summary>
public sealed class PrecisionSlider : Panel
{
    private readonly PrecisionSliderCanvas _canvas;
    private bool _isDragging;
    private double _dragStartY;
    private double _dragStartValue;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(PrecisionSlider),
            new PropertyMetadata(0.0, static (d, e) => ((PrecisionSlider)d)._canvas.Invalidate()));
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(PrecisionSlider), new PropertyMetadata(0.0));
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(PrecisionSlider), new PropertyMetadata(100.0));
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public static readonly DependencyProperty DetentCountProperty =
        DependencyProperty.Register(nameof(DetentCount), typeof(int), typeof(PrecisionSlider), new PropertyMetadata(20));
    public int DetentCount { get => (int)GetValue(DetentCountProperty); set => SetValue(DetentCountProperty, value); }

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(PrecisionSlider),
            new PropertyMetadata(null, static (d, e) => ((PrecisionSlider)d)._canvas.Invalidate()));
    public Brush AccentBrush { get => (Brush)GetValue(AccentBrushProperty); set => SetValue(AccentBrushProperty, value); }

    public static readonly DependencyProperty SensitivityProperty =
        DependencyProperty.Register(nameof(Sensitivity), typeof(double), typeof(PrecisionSlider), new PropertyMetadata(0.4));
    public double Sensitivity { get => (double)GetValue(SensitivityProperty); set => SetValue(SensitivityProperty, value); }

    internal double NormalizedValue =>
        (Maximum - Minimum) > 0 ? (Value - Minimum) / (Maximum - Minimum) : 0.0;

    public PrecisionSlider()
    {
        _canvas = new PrecisionSliderCanvas(this);
        Children.Add(_canvas);
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;
        ManipulationMode = ManipulationModes.TranslateY;
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
        _dragStartY = e.GetCurrentPoint(this).Position.Y;
        _dragStartValue = Value;
        CapturePointer(e.Pointer);
        _canvas.Invalidate();
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging) return;
        var deltaY = _dragStartY - e.GetCurrentPoint(this).Position.Y; // up = increase
        Value = Math.Clamp(_dragStartValue + deltaY * Sensitivity, Minimum, Maximum);
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e) => EndDrag(e.Pointer);
    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e) => EndDrag(e.Pointer);

    private void EndDrag(Pointer pointer)
    {
        if (!_isDragging) return;
        _isDragging = false;
        ReleasePointerCapture(pointer);
        _canvas.Invalidate();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _canvas.Measure(availableSize);
        return _canvas.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _canvas.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }
}

internal sealed class PrecisionSliderCanvas : SKCanvasElement
{
    private readonly PrecisionSlider _owner;

    // Paints — allocated once
    private readonly SKPaint _trackPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round,
        IsAntialias = true, Color = new SKColor(255, 255, 255, 10),
    };
    private readonly SKPaint _activePaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 2f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _activeGlowPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 6f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _bezelPaint = new() { IsAntialias = true };
    private readonly SKPaint _knobPaint = new() { IsAntialias = true };
    private readonly SKPaint _dimplePaint = new() { IsAntialias = true };
    private readonly SKPaint _tickPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _indicatorLinePaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _indicatorGlowPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 5f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _causticPaint = new() { IsAntialias = true };
    private readonly SKPaint _rimPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f, IsAntialias = true,
        Color = new SKColor(255, 255, 255, 10),
    };
    private readonly SKPaint _knurlPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 0.6f, IsAntialias = true,
        Color = new SKColor(255, 255, 255, 10),
    };

    public PrecisionSliderCanvas(PrecisionSlider owner) => _owner = owner;

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        float w = (float)area.Width, h = (float)area.Height;
        float cx = w / 2, cy = h / 2;
        float pad = Math.Min(w, h) * 0.05f;
        float radius = Math.Min(cx, cy) - pad;

        float normalized = (float)_owner.NormalizedValue;
        const float arcSweepDeg = 270f;
        float rotDeg = normalized * arcSweepDeg - arcSweepDeg / 2f;
        int detentCount = _owner.DetentCount;
        var accent = ResolveAccent();

        // Geometry — squircle half-sizes and corner radii
        float arcHs = radius * 0.9f;
        float bezelHs = radius * 0.74f;
        float knobHs = radius * 0.71f;
        float arcCornerR = arcHs * 0.16f;
        float bezelCornerR = bezelHs * 0.16f;
        float knobCornerR = knobHs * 0.16f;

        // Build squircle arc path + measure
        using var arcPath = MakeSquirclePath(cx, cy, arcHs, arcCornerR);
        using var arcMeasure = new SKPathMeasure(arcPath, false);
        float arcTotalLen = arcMeasure.Length;
        float arcStartDist = ComputeArcStartFraction(arcHs, arcCornerR) * arcTotalLen;
        float arcSweepLen = 0.75f * arcTotalLen;

        // ── Layer 1: Track arc ──
        using (var trackPath = new SKPath())
        {
            ExtractArc(arcMeasure, arcTotalLen, arcStartDist, arcSweepLen, trackPath);
            canvas.DrawPath(trackPath, _trackPaint);
        }

        // ── Layer 2: Active arc + glow ──
        if (normalized > 0.001f)
        {
            using var activePath = new SKPath();
            ExtractArc(arcMeasure, arcTotalLen, arcStartDist, normalized * arcSweepLen, activePath);

            _activeGlowPaint.Color = accent.WithAlpha((byte)(50 + normalized * 80));
            _activeGlowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f + normalized * 6f);
            canvas.DrawPath(activePath, _activeGlowPaint);

            _activePaint.Color = accent;
            canvas.DrawPath(activePath, _activePaint);
        }

        // ── Layer 3: Detent ticks ──
        for (int i = 0; i <= detentCount; i++)
        {
            float frac = (float)i / detentCount;
            float dist = arcStartDist + frac * arcSweepLen;
            if (dist >= arcTotalLen) dist -= arcTotalLen;
            if (!arcMeasure.GetPositionAndTangent(dist, out var pos, out var tan)) continue;

            bool isMajor = i % 5 == 0;
            bool isActive = frac <= normalized;
            // Outward normal for CW path: (ty, -tx)
            float nx = tan.Y, ny = -tan.X;
            float innerOff = isMajor ? 7f : 5f;
            float outerOff = isMajor ? 0f : 2f;

            _tickPaint.StrokeWidth = isMajor ? 1.5f : 0.75f;
            _tickPaint.Color = isActive
                ? accent.WithAlpha((byte)(isMajor ? 200 : 100))
                : new SKColor(255, 255, 255, (byte)(isMajor ? 30 : 13));

            canvas.DrawLine(
                pos.X - nx * outerOff, pos.Y - ny * outerOff,
                pos.X - nx * innerOff, pos.Y - ny * innerOff,
                _tickPaint);
        }

        // ── Layer 4: Bezel ──
        var bezelRect = new SKRect(cx - bezelHs, cy - bezelHs, cx + bezelHs, cy + bezelHs);
        _bezelPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - bezelHs * 0.3f, cy - bezelHs * 0.3f),
            bezelHs * 2f,
            new[] { new SKColor(60, 60, 62), new SKColor(20, 20, 21) },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(bezelRect, bezelCornerR, bezelCornerR, _bezelPaint);

        // ── Layer 4b: Knurl (fine lines along bezel perimeter) ──
        using var bezelPath = MakeSquirclePath(cx, cy, bezelHs + 2f, bezelCornerR);
        using var bezelMeasure = new SKPathMeasure(bezelPath, false);
        float bezelLen = bezelMeasure.Length;
        for (int i = 0; i < 160; i++)
        {
            float dist = ((float)i / 160) * bezelLen;
            if (!bezelMeasure.GetPositionAndTangent(dist, out var pos, out var tan)) continue;
            float nx = tan.Y, ny = -tan.X;
            canvas.DrawLine(pos.X - nx * 0.5f, pos.Y - ny * 0.5f,
                            pos.X - nx * 3.5f, pos.Y - ny * 3.5f, _knurlPaint);
        }

        // ── Layer 5: Knob body (sweep gradient — same as round dial) ──
        var knobRect = new SKRect(cx - knobHs, cy - knobHs, cx + knobHs, cy + knobHs);
        var colors = new SKColor[]
        {
            new(42,42,43), new(50,50,51), new(45,45,46),
            new(49,49,50), new(43,43,44), new(47,47,48),
            new(42,42,43), new(50,50,51), new(45,45,46),
            new(49,49,50), new(43,43,44), new(47,47,48), new(42,42,43),
        };
        var positions = new float[]
        {
            0f,1/12f,2/12f,3/12f,4/12f,5/12f,
            6/12f,7/12f,8/12f,9/12f,10/12f,11/12f,1f,
        };
        _knobPaint.Shader = SKShader.CreateCompose(
            SKShader.CreateSweepGradient(new SKPoint(cx, cy), colors, positions),
            SKShader.CreateRadialGradient(
                new SKPoint(cx - knobHs * 0.3f, cy - knobHs * 0.4f),
                knobHs * 1.5f,
                new[] { new SKColor(255, 255, 255, 20), SKColors.Transparent },
                SKShaderTileMode.Clamp),
            SKBlendMode.Screen);
        canvas.DrawRoundRect(knobRect, knobCornerR, knobCornerR, _knobPaint);

        // ── Layer 6: Center dimple (small rounded rect) ──
        const float dimpleHs = 6f;
        _dimplePaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - 1.5f, cy - 1.5f), dimpleHs * 2f,
            new[] { new SKColor(58, 58, 59), new SKColor(30, 30, 31) },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(
            new SKRect(cx - dimpleHs, cy - dimpleHs, cx + dimpleHs, cy + dimpleHs),
            2f, 2f, _dimplePaint);

        // ── Layer 7: Indicator (radial line near knob edge, same as round dial) ──
        float indAngleDeg = rotDeg - 90f;
        float indAngleRad = indAngleDeg * MathF.PI / 180f;
        float indLen = knobHs * 0.12f;
        float indOff = knobHs * 0.12f;
        float indInnerR = knobHs - indOff - indLen;
        float indOuterR = knobHs - indOff;
        float ix1 = cx + MathF.Cos(indAngleRad) * indInnerR;
        float iy1 = cy + MathF.Sin(indAngleRad) * indInnerR;
        float ix2 = cx + MathF.Cos(indAngleRad) * indOuterR;
        float iy2 = cy + MathF.Sin(indAngleRad) * indOuterR;

        _indicatorGlowPaint.Color = accent.WithAlpha((byte)(40 + normalized * 60));
        _indicatorGlowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f + normalized * 5f);
        canvas.DrawLine(ix1, iy1, ix2, iy2, _indicatorGlowPaint);

        _indicatorLinePaint.Color = accent.WithAlpha((byte)(180 + normalized * 75));
        canvas.DrawLine(ix1, iy1, ix2, iy2, _indicatorLinePaint);

        // ── Layer 8: Caustic (fixed-position radial highlight) ──
        float lightAngleRad = -40f * MathF.PI / 180f;
        float hlX = cx + MathF.Cos(lightAngleRad) * knobHs * 0.35f;
        float hlY = cy + MathF.Sin(lightAngleRad) * knobHs * 0.35f;
        _causticPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(hlX, hlY), knobHs * 0.6f,
            new[] { new SKColor(255, 255, 255, 18), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(knobRect, knobCornerR, knobCornerR, _causticPaint);

        // ── Layer 9: Rim light ──
        canvas.DrawRoundRect(knobRect, knobCornerR, knobCornerR, _rimPaint);
    }

    // ── Helpers ──

    private static SKPath MakeSquirclePath(float cx, float cy, float hs, float r)
    {
        var path = new SKPath();
        path.AddRoundRect(new SKRect(cx - hs, cy - hs, cx + hs, cy + hs), r, r);
        return path;
    }

    /// <summary>
    /// Computes the perimeter fraction at which the 270° arc starts (bottom-left equivalent).
    /// SkiaSharp AddRoundRect starts at (left+r, top) going CW.
    /// Arc start = 135° Skia-convention = midpoint of the bottom-left corner arc.
    /// Distance = 3*straight + 2.5*cornerArc from path start.
    /// </summary>
    private static float ComputeArcStartFraction(float hs, float r)
    {
        float straight = 2 * hs - 2 * r;
        float cornerArc = MathF.PI * r / 2;
        float total = 4 * straight + 4 * cornerArc;
        float dist = 3 * straight + 2.5f * cornerArc;
        return dist / total;
    }

    private static void ExtractArc(SKPathMeasure measure, float totalLen,
        float startDist, float sweepDist, SKPath dst)
    {
        float endDist = startDist + sweepDist;
        if (endDist <= totalLen)
        {
            measure.GetSegment(startDist, endDist, dst, true);
        }
        else
        {
            measure.GetSegment(startDist, totalLen, dst, true);
            if (endDist - totalLen > 0.01f)
                measure.GetSegment(0, endDist - totalLen, dst, true);
        }
    }

    private SKColor ResolveAccent()
    {
        if (_owner.AccentBrush is SolidColorBrush b)
            return new SKColor(b.Color.R, b.Color.G, b.Color.B, b.Color.A);
        return new SKColor(212, 169, 89, 255);
    }
}

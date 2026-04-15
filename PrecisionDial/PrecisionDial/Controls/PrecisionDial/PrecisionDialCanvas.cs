using System;
using System.Diagnostics;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace PrecisionDial.Controls;

public sealed class PrecisionDialCanvas : SKCanvasElement
{
    private readonly PrecisionDial _owner;

    // v3 renderers
    private readonly DashedArcRenderer _dashedArcRenderer = new();
    private readonly OrbitingValueRenderer _orbitingValueRenderer = new();
    private readonly MenuIconRenderer _menuIconRenderer = new();
    private readonly BrushedMetalRenderer _brushedMetalRenderer = new();
    private readonly ConeLightRenderer _coneLightRenderer = new();

    // v2 renderers (carried over)
    private readonly DetentTickRenderer _detentRenderer = new();
    private readonly DialRenderer _dialRenderer = new();
    private readonly IndicatorRenderer _indicatorRenderer = new();
    private readonly PulseRenderer _pulseRenderer = new();
    private readonly KnurlRenderer _knurlRenderer = new();
    private readonly ParticleRenderer _particleRenderer = new();
    private readonly CausticRenderer _causticRenderer = new();

    private readonly SKPaint _rimPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 0.5f,
        Color = new SKColor(255, 255, 255, 10), // 4% white
        IsAntialias = true,
    };

    private bool _pulseActive;
    private long _pulseStartTick;
    private const long PulseDurationMs = 150;

    private float _lastCx, _lastCy, _lastSize;

    public PrecisionDialCanvas(PrecisionDial owner)
    {
        _owner = owner;
    }

    public void TriggerDetentPulse()
    {
        _pulseActive = true;
        _pulseStartTick = Stopwatch.GetTimestamp();
        Invalidate();
    }

    public void EmitParticles(float velocityScale)
    {
        if (velocityScale < 0.01f || _lastSize <= 0) return;

        var profile = new DialSizeProfile(_lastSize);
        var knobR = profile.KnobR;
        var rotationDeg = (float)_owner.DisplayRotationDegrees;
        var angleDeg = rotationDeg - 90f;
        var angleRad = angleDeg * MathF.PI / 180f;
        var outerR = knobR - knobR * 0.12f;
        var x = _lastCx + MathF.Cos(angleRad) * outerR;
        var y = _lastCy + MathF.Sin(angleRad) * outerR;

        _particleRenderer.Emit(x, y, velocityScale, ResolveAccentColor());
        Invalidate();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        var width = (float)area.Width;
        var height = (float)area.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var rawSize = MathF.Min(width, height);
        var mode = _owner.DialMode;

        // In menu mode, shrink the geometric "size" so the brief's iconR offset
        // (arcR + size*0.075) keeps icons OUTSIDE the arc but still inside the
        // canvas bounds. Value mode uses the full available space.
        var size = mode == DialMode.Menu ? rawSize * 0.85f : rawSize;
        var profile = new DialSizeProfile(size);

        _lastCx = cx;
        _lastCy = cy;
        _lastSize = size;

        var arcSweep = (float)_owner.ArcSweepDegrees;
        var startAngle = 90f + (360f - arcSweep) / 2f;
        var accentColor = ResolveAccentColor();
        var hotAccentColor = WarmShift(accentColor);

        if (mode == DialMode.Menu)
        {
            RenderMenu(canvas, cx, cy, profile, arcSweep, startAngle, accentColor);
        }
        else
        {
            RenderValue(canvas, cx, cy, profile, arcSweep, startAngle, accentColor, hotAccentColor);
        }

        // ── Pulse + particles (shared) ────────────────────────────────────────
        if (_pulseActive)
        {
            var elapsed = Stopwatch.GetElapsedTime(_pulseStartTick).TotalMilliseconds;
            var progress = (float)Math.Clamp(elapsed / PulseDurationMs, 0, 1);
            if (progress >= 1f)
                _pulseActive = false;
            else
            {
                _pulseRenderer.Draw(canvas, cx, cy, profile.KnobR, progress, accentColor);
                Invalidate();
            }
        }

        _particleRenderer.Step();
        if (_particleRenderer.HasActiveParticles)
        {
            _particleRenderer.Draw(canvas, accentColor);
            Invalidate();
        }
    }

    // ── Value mode pipeline ──────────────────────────────────────────────────
    private void RenderValue(
        SKCanvas canvas,
        float cx,
        float cy,
        DialSizeProfile profile,
        float arcSweep,
        float startAngle,
        SKColor accent,
        SKColor hotAccent)
    {
        var arcR = profile.ArcR;
        var knobR = profile.KnobR;
        var bezelR = profile.BezelR;
        var rotationDeg = (float)_owner.DisplayRotationDegrees;
        var normalized = (float)_owner.DisplayNormalizedValue;

        var segCount = ResolveValueSegmentCount(profile);
        var gapDeg = 2.2f;
        _dashedArcRenderer.EnsureSegments(segCount, arcSweep, startAngle, gapDeg);

        var velocityScale = (float)Math.Clamp(Math.Abs(_owner.CurrentVelocity) / 100.0, 0.0, 1.0);

        // Layer 1-3: dashed arc + glow
        var activeCount = _dashedArcRenderer.ActiveCountForNormalized(normalized);
        _dashedArcRenderer.DrawValueMode(
            canvas, cx, cy, arcR, accent, hotAccent, activeCount,
            profile.SegmentInactiveStroke, profile.SegmentActiveStroke, velocityScale);

        // Layer 4: detent ticks
        _detentRenderer.Draw(canvas, cx, cy, profile, arcSweep, _owner.DetentCount, _owner.CurrentDetentIndex, accent);

        // Layer 5: knurl
        _knurlRenderer.Draw(canvas, cx, cy, profile, accent);

        // Layer 6: bezel
        _dialRenderer.DrawBezel(canvas, cx, cy, bezelR);

        // Layer 7: knob body + brushed metal (rotated)
        canvas.Save();
        canvas.RotateDegrees(rotationDeg, cx, cy);
        _dialRenderer.DrawKnob(canvas, cx, cy, knobR);
        if (profile.ShowBrushedMetal)
            _brushedMetalRenderer.Draw(canvas, cx, cy, knobR);
        canvas.Restore();

        // Layer 8: line indicator (rotated)
        _indicatorRenderer.Draw(canvas, cx, cy, knobR, rotationDeg, normalized, accent);

        // Layer 9: caustic
        _causticRenderer.Draw(canvas, cx, cy, knobR);

        // Layer 10: center dimple
        _dialRenderer.DrawCenterDimple(canvas, cx, cy);

        // Rim light
        canvas.DrawCircle(cx, cy, knobR, _rimPaint);

        // Layer 13: orbiting value
        if (profile.ShowOrbitingValue)
        {
            _orbitingValueRenderer.Draw(
                canvas, _dashedArcRenderer.Segments, activeCount,
                cx, cy, arcR, profile.Size, _owner.Value, normalized, accent);
        }
    }

    // ── Menu mode pipeline ───────────────────────────────────────────────────
    private void RenderMenu(
        SKCanvas canvas,
        float cx,
        float cy,
        DialSizeProfile profile,
        float arcSweep,
        float startAngle,
        SKColor accent)
    {
        var items = _owner.MenuItems;
        var count = items?.Count ?? 0;
        if (count <= 0)
        {
            // Nothing to render — fall back to value mode silhouette so the dial isn't blank.
            RenderValue(canvas, cx, cy, profile, arcSweep, startAngle, accent, WarmShift(accent));
            return;
        }

        var arcR = profile.ArcR;
        var knobR = profile.KnobR;
        var bezelR = profile.BezelR;
        var selectedIndex = Math.Clamp(_owner.SelectedIndex, 0, count - 1);

        var gapDeg = 2.4f;
        _dashedArcRenderer.EnsureSegments(count, arcSweep, startAngle, gapDeg);

        // The knob and cone rotate to point at the selected segment's midpoint.
        // Convert screen-frame midpoint angle (0=east, 90=south) to knob-up rotation (+90 offset).
        var midAngle = _dashedArcRenderer.Segments[selectedIndex].MidAngle;
        var rotationDeg = midAngle + 90f;

        // Collapse rotation: when the bottom sheet is collapsing, rotate the whole
        // dial so the selected item ends up at the top center (screen-frame -90°).
        // At lift progress = 1 (fully lifted) the rotation is 0; at progress = 0
        // (fully collapsed) it's (-90° - midAngle) so the selected segment moves
        // to the top. Interpolates linearly across the lift transition.
        var liftProgress = (float)Math.Clamp(_owner.MenuLiftProgress, 0.0, 1.0);
        var collapseR = (1f - liftProgress) * (-90f - midAngle);

        // Outer transform: the arc segments, knob, brushed metal and cone rotate
        // together by collapseR so they stay locked to the selection as it swings
        // toward the top. Icons are drawn OUTSIDE this rotation (their positions
        // take collapseR into account but the glyphs stay upright in screen space).
        canvas.Save();
        if (MathF.Abs(collapseR) > 0.001f)
            canvas.RotateDegrees(collapseR, cx, cy);

        // Layer 1-2: menu arc segments + selected glow
        _dashedArcRenderer.DrawMenuMode(
            canvas, cx, cy, arcR, accent, selectedIndex,
            profile.SegmentInactiveStroke, profile.SegmentActiveStroke);

        // Layer 4: knurl
        _knurlRenderer.Draw(canvas, cx, cy, profile, accent);

        // Layer 5: bezel
        _dialRenderer.DrawBezel(canvas, cx, cy, bezelR);

        // Layer 6: knob body + brushed metal (rotated)
        canvas.Save();
        canvas.RotateDegrees(rotationDeg, cx, cy);
        _dialRenderer.DrawKnob(canvas, cx, cy, knobR);
        if (profile.ShowBrushedMetal)
            _brushedMetalRenderer.Draw(canvas, cx, cy, knobR);

        // Layer 7-11: cone-of-light (rotated with the knob)
        // Half-angle of cone = segment half-angle × 1.6
        var segHalfAngle = (arcSweep - gapDeg * (count - 1)) / count * 0.5f;
        var coneHalfAngle = segHalfAngle * 1.6f;

        // Scale blur sigmas with size — brief specifies dp values referenced to the design size.
        var sigmaScale = profile.Size / 200f;
        _coneLightRenderer.Draw(
            canvas, cx, cy, knobR, coneHalfAngle,
            18f * sigmaScale, 10f * sigmaScale, 5f * sigmaScale,
            accent);

        canvas.Restore(); // undo knob rotation

        canvas.Restore(); // undo outer collapse rotation

        // Layer 3 (moved): menu icons drawn in unrotated screen space.
        // rotationOffsetDeg shifts each icon's position by collapseR so they
        // move with the arc, but the glyph itself stays upright.
        // liftProgress hides the non-selected icons and shrinks the selected
        // one when the dial is shelved.
        if (profile.ShowMenuIcons)
        {
            _menuIconRenderer.Draw(
                canvas, _dashedArcRenderer.Segments, items, selectedIndex,
                cx, cy, arcR, profile.Size, accent,
                collapseR,
                liftProgress);
        }

        // Layer 12: caustic (fixed — light source is stationary regardless of dial collapse)
        _causticRenderer.Draw(canvas, cx, cy, knobR);

        // Layer 13: center dimple
        _dialRenderer.DrawCenterDimple(canvas, cx, cy);

        // Rim
        canvas.DrawCircle(cx, cy, knobR, _rimPaint);
    }

    private int ResolveValueSegmentCount(DialSizeProfile profile)
    {
        var override_ = _owner.SegmentCount;
        return override_ > 0 ? override_ : profile.AutoSegments;
    }

    private SKColor ResolveAccentColor()
    {
        if (_owner.AccentBrush is Microsoft.UI.Xaml.Media.SolidColorBrush solidBrush)
        {
            var c = solidBrush.Color;
            return new SKColor(c.R, c.G, c.B, c.A);
        }
        return new SKColor(212, 169, 89, 255);
    }

    /// <summary>Shifts an accent color slightly warmer (+hue toward orange, +saturation).</summary>
    private static SKColor WarmShift(SKColor c)
    {
        c.ToHsl(out var h, out var s, out var l);
        h = (h + 340f) % 360f; // shift hue ~-20 (warmer/redder)
        s = Math.Clamp(s + 10f, 0f, 100f);
        return SKColor.FromHsl(h, s, l, c.Alpha);
    }
}

internal sealed class DetentTickRenderer
{
    private readonly SKPaint _tickPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };

    public void Draw(SKCanvas canvas, float cx, float cy, DialSizeProfile profile,
        float arcSweep, int detentCount, int currentDetent, SKColor accent)
    {
        if (detentCount <= 0) return;

        var arcR = profile.ArcR;
        var halfGap = (360f - arcSweep) / 2f;
        for (int i = 0; i <= detentCount; i++)
        {
            var fraction = (float)i / detentCount;
            var angleDeg = 90f + halfGap + fraction * arcSweep;
            var angleRad = angleDeg * MathF.PI / 180f;
            var isMajor = i % 5 == 0;
            var isActive = i <= currentDetent;
            var innerR = arcR * (isMajor ? 0.86f : 0.89f);
            var outerR = arcR * 0.93f;
            var x1 = cx + MathF.Cos(angleRad) * innerR;
            var y1 = cy + MathF.Sin(angleRad) * innerR;
            var x2 = cx + MathF.Cos(angleRad) * outerR;
            var y2 = cy + MathF.Sin(angleRad) * outerR;
            if (isActive)
                _tickPaint.Color = accent.WithAlpha((byte)(isMajor ? 200 : 100));
            else
                _tickPaint.Color = new SKColor(255, 255, 255, (byte)(isMajor ? 30 : 13));
            _tickPaint.StrokeWidth = isMajor ? 1.5f : 0.75f;
            canvas.DrawLine(x1, y1, x2, y2, _tickPaint);
        }
    }
}

internal sealed class DialRenderer
{
    private readonly SKPaint _bezelPaint = new() { IsAntialias = true };
    private readonly SKPaint _knobPaint = new() { IsAntialias = true };
    private readonly SKPaint _dimplePaint = new() { IsAntialias = true };

    public void DrawBezel(SKCanvas canvas, float cx, float cy, float bezelR)
    {
        _bezelPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - bezelR * 0.3f, cy - bezelR * 0.3f),
            bezelR * 2f,
            new[] { new SKColor(60, 60, 62), new SKColor(20, 20, 21) },
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(cx, cy, bezelR + 3f, _bezelPaint);
    }

    public void DrawKnob(SKCanvas canvas, float cx, float cy, float knobR)
    {
        var colors = new SKColor[]
        {
            new(42, 42, 43), new(50, 50, 51), new(45, 45, 46),
            new(49, 49, 50), new(43, 43, 44), new(47, 47, 48),
            new(42, 42, 43), new(50, 50, 51), new(45, 45, 46),
            new(49, 49, 50), new(43, 43, 44), new(47, 47, 48),
            new(42, 42, 43),
        };
        var positions = new float[]
        {
            0f, 1/12f, 2/12f, 3/12f, 4/12f, 5/12f,
            6/12f, 7/12f, 8/12f, 9/12f, 10/12f, 11/12f, 1f,
        };
        _knobPaint.Shader = SKShader.CreateCompose(
            SKShader.CreateSweepGradient(new SKPoint(cx, cy), colors, positions),
            SKShader.CreateRadialGradient(
                new SKPoint(cx - knobR * 0.3f, cy - knobR * 0.4f),
                knobR * 1.5f,
                new[] { new SKColor(255, 255, 255, 20), SKColors.Transparent },
                SKShaderTileMode.Clamp),
            SKBlendMode.Screen);
        canvas.DrawCircle(cx, cy, knobR, _knobPaint);
    }

    public void DrawCenterDimple(SKCanvas canvas, float cx, float cy)
    {
        var dimpleRadius = 6f;
        _dimplePaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - 1.5f, cy - 1.5f),
            dimpleRadius * 2f,
            new[] { new SKColor(58, 58, 59), new SKColor(30, 30, 31) },
            SKShaderTileMode.Clamp);
        canvas.DrawCircle(cx, cy, dimpleRadius, _dimplePaint);
    }
}

internal sealed class IndicatorRenderer
{
    private readonly SKPaint _linePaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };
    private readonly SKPaint _glowPaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 5f, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
    };

    // Cached blur filter — only recreated when the sigma actually changes by
    // more than a quarter pixel. Previously this allocated a new native
    // SKMaskFilter every frame during drag, which leaked and churned GC.
    private SKMaskFilter? _glowFilter;
    private float _glowFilterSigma = -1f;

    public void Draw(SKCanvas canvas, float cx, float cy, float knobR,
        float rotationDeg, float normalized, SKColor accent)
    {
        var indicatorLength = knobR * 0.12f;
        var indicatorOffset = knobR * 0.12f;
        var angleDeg = rotationDeg - 90f;
        var angleRad = angleDeg * MathF.PI / 180f;
        var innerR = knobR - indicatorOffset - indicatorLength;
        var outerR = knobR - indicatorOffset;
        var x1 = cx + MathF.Cos(angleRad) * innerR;
        var y1 = cy + MathF.Sin(angleRad) * innerR;
        var x2 = cx + MathF.Cos(angleRad) * outerR;
        var y2 = cy + MathF.Sin(angleRad) * outerR;

        var glowSigma = 3f + normalized * 5f;
        if (_glowFilter is null || MathF.Abs(glowSigma - _glowFilterSigma) > 0.25f)
        {
            _glowFilter?.Dispose();
            _glowFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, glowSigma);
            _glowFilterSigma = glowSigma;
        }

        var glowAlpha = (byte)(40 + normalized * 60);
        _glowPaint.Color = accent.WithAlpha(glowAlpha);
        _glowPaint.MaskFilter = _glowFilter;
        canvas.DrawLine(x1, y1, x2, y2, _glowPaint);

        var lineAlpha = (byte)(180 + normalized * 75);
        _linePaint.Color = accent.WithAlpha(lineAlpha);
        canvas.DrawLine(x1, y1, x2, y2, _linePaint);
    }
}

internal sealed class PulseRenderer
{
    private readonly SKPaint _pulsePaint = new()
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 1f, IsAntialias = true,
    };

    public void Draw(SKCanvas canvas, float cx, float cy, float knobR,
        float progress, SKColor accent)
    {
        var expandedRadius = knobR + progress * knobR * 0.08f;
        var alpha = (byte)(40 * (1f - progress));
        _pulsePaint.Color = accent.WithAlpha(alpha);
        canvas.DrawCircle(cx, cy, expandedRadius, _pulsePaint);
    }
}

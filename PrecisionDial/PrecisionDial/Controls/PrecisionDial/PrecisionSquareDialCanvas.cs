using System;
using System.Diagnostics;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace PrecisionDial.Controls;

public sealed class PrecisionSquareDialCanvas : SKCanvasElement
{
    private readonly PrecisionSquareDial _owner;
    private readonly ParticleRenderer _particleRenderer = new();

    private bool _pulseActive;
    private long _pulseStartTick;
    private const long PulseDurationMs = 150;

    private float _lastCx, _lastCy, _lastSize;

    public PrecisionSquareDialCanvas(PrecisionSquareDial owner)
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
        var normalized = (float)_owner.DisplayNormalizedValue;
        var arcFraction = (float)_owner.ArcSweepDegrees / 360f;
        var tStart = (1f - arcFraction) / 2f;
        var t = tStart + normalized * arcFraction;
        var knobSize = _lastSize * 0.71f;
        var (px, py) = GetPerimeterPoint(t, _lastCx, _lastCy, knobSize, knobSize, knobSize * 0.12f);
        _particleRenderer.Emit(px, py, velocityScale, ResolveAccentColor());
        Invalidate();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        var width = (float)area.Width;
        var height = (float)area.Height;
        var cx = width / 2f;
        var cy = height / 2f;
        var pad = Math.Min(width, height) * 0.05f;
        var size = Math.Min(width, height) - 2f * pad;

        _lastCx = cx;
        _lastCy = cy;
        _lastSize = size;

        var normalized = (float)_owner.DisplayNormalizedValue;
        var detentCount = _owner.DetentCount;
        var currentDetent = _owner.CurrentDetentIndex;
        var arcFraction = (float)_owner.ArcSweepDegrees / 360f;
        var accentColor = ResolveAccentColor();
        var tStart = (1f - arcFraction) / 2f;
        var tEnd = 1f - tStart;

        var bezelR = size * 0.12f;
        var knobSize = size * 0.71f;
        var knobR = knobSize * 0.12f;
        var arcSize = size * 0.9f;
        var arcR = arcSize * 0.12f;

        // Bezel
        DrawBezel(canvas, cx, cy, size, bezelR);

        // Knurl texture around bezel
        DrawKnurl(canvas, cx, cy, size, bezelR);

        // Knob body
        DrawKnob(canvas, cx, cy, knobSize, knobR);

        // Arc track (dim)
        using (var trackPath = MakeArcPath(tStart, tEnd, cx, cy, arcSize, arcR))
        using (var trackPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round,
            IsAntialias = true, Color = new SKColor(255, 255, 255, 10),
        })
        {
            canvas.DrawPath(trackPath, trackPaint);
        }

        // Active arc
        if (normalized > 0f)
        {
            var tActive = tStart + normalized * arcFraction;
            using var activePath = MakeArcPath(tStart, tActive, cx, cy, arcSize, arcR);
            var glowAlpha = (byte)(50 + normalized * 80);
            using (var glowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke, StrokeWidth = 6f, StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                Color = accentColor.WithAlpha(glowAlpha),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f + normalized * 6f),
            })
            {
                canvas.DrawPath(activePath, glowPaint);
            }
            using var activePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f, StrokeCap = SKStrokeCap.Round,
                IsAntialias = true, Color = accentColor,
            };
            canvas.DrawPath(activePath, activePaint);
        }

        // Detent ticks
        DrawDetentTicks(canvas, cx, cy, size, arcFraction, tStart, detentCount, currentDetent, accentColor);

        // Indicator notch
        DrawIndicator(canvas, cx, cy, knobSize, arcFraction, tStart, normalized, accentColor);

        // Caustic highlight
        DrawCaustic(canvas, cx, cy, knobSize / 2f, knobR);

        // Rim light
        var knobRect = new SKRect(cx - knobSize / 2f, cy - knobSize / 2f, cx + knobSize / 2f, cy + knobSize / 2f);
        using (var rimPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f,
            Color = new SKColor(255, 255, 255, 10), IsAntialias = true,
        })
        {
            canvas.DrawRoundRect(knobRect, knobR, knobR, rimPaint);
        }

        // Center dimple
        DrawCenterDimple(canvas, cx, cy);

        // Pulse
        if (_pulseActive)
        {
            var elapsed = Stopwatch.GetElapsedTime(_pulseStartTick).TotalMilliseconds;
            var progress = (float)Math.Clamp(elapsed / PulseDurationMs, 0, 1);
            if (progress >= 1f)
                _pulseActive = false;
            else
            {
                DrawPulse(canvas, cx, cy, knobSize, knobR, progress, accentColor);
                Invalidate();
            }
        }

        // Particles
        _particleRenderer.Step();
        if (_particleRenderer.HasActiveParticles)
        {
            _particleRenderer.Draw(canvas, accentColor);
            Invalidate();
        }
    }

    // ── Perimeter math ──────────────────────────────────────────────────────────

    // Returns (x, y) on the perimeter of a rounded square,
    // parameterized t ∈ [0, 1), starting at bottom-center, going clockwise.
    private static (float x, float y) GetPerimeterPoint(
        float t, float cx, float cy, float w, float h, float r)
    {
        float hw = w / 2f, hh = h / 2f;
        r = Math.Min(r, Math.Min(hw, hh) * 0.999f);
        float arc = MathF.PI / 2f * r;
        float s0 = hw - r;      // bottom-center → BR corner start
        float s2 = h - 2f * r;  // right edge
        float s4 = w - 2f * r;  // top edge (going left)
        float s6 = h - 2f * r;  // left edge (going down)
        float s8 = hw - r;      // BL corner end → bottom-center
        float total = s0 + arc + s2 + arc + s4 + arc + s6 + arc + s8;
        if (total <= 0f) return (cx, cy + hh);

        float d = ((t % 1f) + 1f) % 1f * total;

        // Seg 0: right along bottom, center → BR corner
        if (d < s0) return (cx + d, cy + hh);
        d -= s0;
        // Seg 1: BR corner arc, 90° → 0°
        if (d < arc) { float a = (90f - d / arc * 90f) * MathF.PI / 180f; return (cx + hw - r + r * MathF.Cos(a), cy + hh - r + r * MathF.Sin(a)); }
        d -= arc;
        // Seg 2: up along right
        if (d < s2) return (cx + hw, cy + hh - r - d);
        d -= s2;
        // Seg 3: TR corner arc, 0° → -90°
        if (d < arc) { float a = (-d / arc * 90f) * MathF.PI / 180f; return (cx + hw - r + r * MathF.Cos(a), cy - hh + r + r * MathF.Sin(a)); }
        d -= arc;
        // Seg 4: left along top
        if (d < s4) return (cx + hw - r - d, cy - hh);
        d -= s4;
        // Seg 5: TL corner arc, -90° → -180°
        if (d < arc) { float a = (-90f - d / arc * 90f) * MathF.PI / 180f; return (cx - hw + r + r * MathF.Cos(a), cy - hh + r + r * MathF.Sin(a)); }
        d -= arc;
        // Seg 6: down along left
        if (d < s6) return (cx - hw, cy - hh + r + d);
        d -= s6;
        // Seg 7: BL corner arc, 180° → 90°
        if (d < arc) { float a = (180f - d / arc * 90f) * MathF.PI / 180f; return (cx - hw + r + r * MathF.Cos(a), cy + hh - r + r * MathF.Sin(a)); }
        d -= arc;
        // Seg 8: right along bottom, BL → center
        return (cx - hw + r + d, cy + hh);
    }

    private static SKPath MakeArcPath(
        float tStart, float tEnd, float cx, float cy, float size, float r)
    {
        const int steps = 80;
        var path = new SKPath();
        for (int i = 0; i <= steps; i++)
        {
            float t = tStart + (float)i / steps * (tEnd - tStart);
            var (x, y) = GetPerimeterPoint(t, cx, cy, size, size, r);
            if (i == 0) path.MoveTo(x, y);
            else path.LineTo(x, y);
        }
        return path;
    }

    // ── Draw helpers ────────────────────────────────────────────────────────────

    private static void DrawBezel(SKCanvas canvas, float cx, float cy, float size, float r)
    {
        var rect = new SKRect(cx - size / 2f, cy - size / 2f, cx + size / 2f, cy + size / 2f);
        using var paint = new SKPaint { IsAntialias = true };
        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - size * 0.15f, cy - size * 0.15f),
            size,
            new[] { new SKColor(60, 60, 62), new SKColor(20, 20, 21) },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(rect, r, r, paint);
    }

    private static void DrawKnurl(SKCanvas canvas, float cx, float cy, float size, float r)
    {
        const int count = 160;
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 0.6f,
            Color = new SKColor(255, 255, 255, 10), IsAntialias = true,
        };
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            var (x, y) = GetPerimeterPoint(t, cx, cy, size, size, r);
            float dx = cx - x, dy = cy - y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) continue;
            float nx = dx / len, ny = dy / len;
            canvas.DrawLine(x, y, x + nx * 4f, y + ny * 4f, paint);
        }
    }

    private static void DrawKnob(SKCanvas canvas, float cx, float cy, float size, float r)
    {
        var rect = new SKRect(cx - size / 2f, cy - size / 2f, cx + size / 2f, cy + size / 2f);
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
        using var paint = new SKPaint { IsAntialias = true };
        paint.Shader = SKShader.CreateCompose(
            SKShader.CreateSweepGradient(new SKPoint(cx, cy), colors, positions),
            SKShader.CreateRadialGradient(
                new SKPoint(cx - size * 0.15f, cy - size * 0.2f),
                size * 0.75f,
                new[] { new SKColor(255, 255, 255, 20), SKColors.Transparent },
                SKShaderTileMode.Clamp),
            SKBlendMode.Screen);
        canvas.DrawRoundRect(rect, r, r, paint);
    }

    private static void DrawDetentTicks(
        SKCanvas canvas, float cx, float cy, float baseSize,
        float arcFraction, float tStart,
        int detentCount, int currentDetent, SKColor accent)
    {
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, IsAntialias = true,
        };
        for (int i = 0; i <= detentCount; i++)
        {
            float fraction = (float)i / detentCount;
            float t = tStart + fraction * arcFraction;
            bool isMajor = i % 5 == 0;
            bool isActive = i <= currentDetent;

            float outerSz = baseSize * 0.84f;
            float innerSz = baseSize * (isMajor ? 0.77f : 0.80f);

            var (ox, oy) = GetPerimeterPoint(t, cx, cy, outerSz, outerSz, outerSz * 0.12f);
            var (ix, iy) = GetPerimeterPoint(t, cx, cy, innerSz, innerSz, innerSz * 0.12f);

            paint.StrokeWidth = isMajor ? 1.5f : 0.75f;
            paint.Color = isActive
                ? accent.WithAlpha((byte)(isMajor ? 200 : 100))
                : new SKColor(255, 255, 255, (byte)(isMajor ? 30 : 13));
            canvas.DrawLine(ox, oy, ix, iy, paint);
        }
    }

    private static void DrawIndicator(
        SKCanvas canvas, float cx, float cy, float knobSize,
        float arcFraction, float tStart,
        float normalized, SKColor accent)
    {
        float t = tStart + normalized * arcFraction;
        float outerSz = knobSize * 0.88f;
        float innerSz = knobSize * 0.76f;

        var (ox, oy) = GetPerimeterPoint(t, cx, cy, outerSz, outerSz, outerSz * 0.12f);
        var (ix, iy) = GetPerimeterPoint(t, cx, cy, innerSz, innerSz, innerSz * 0.12f);

        var glowAlpha = (byte)(40 + normalized * 60);
        using var glowPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 5f, StrokeCap = SKStrokeCap.Round,
            IsAntialias = true,
            Color = accent.WithAlpha(glowAlpha),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f + normalized * 5f),
        };
        canvas.DrawLine(ix, iy, ox, oy, glowPaint);

        using var linePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round,
            IsAntialias = true, Color = accent.WithAlpha((byte)(180 + normalized * 75)),
        };
        canvas.DrawLine(ix, iy, ox, oy, linePaint);
    }

    private static void DrawCaustic(SKCanvas canvas, float cx, float cy, float halfSize, float r)
    {
        var rect = new SKRect(cx - halfSize, cy - halfSize, cx + halfSize, cy + halfSize);
        using var paint = new SKPaint { IsAntialias = true };
        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - halfSize * 0.3f, cy - halfSize * 0.4f),
            halfSize * 0.6f,
            new[] { new SKColor(255, 255, 255, 18), SKColors.Transparent },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(rect, r, r, paint);
    }

    private static void DrawCenterDimple(SKCanvas canvas, float cx, float cy)
    {
        const float size = 12f;
        const float r = 3f;
        var rect = new SKRect(cx - size / 2f, cy - size / 2f, cx + size / 2f, cy + size / 2f);
        using var paint = new SKPaint { IsAntialias = true };
        paint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(cx - 1.5f, cy - 1.5f),
            size,
            new[] { new SKColor(58, 58, 59), new SKColor(30, 30, 31) },
            SKShaderTileMode.Clamp);
        canvas.DrawRoundRect(rect, r, r, paint);
    }

    private static void DrawPulse(
        SKCanvas canvas, float cx, float cy, float size, float r,
        float progress, SKColor accent)
    {
        var expandedSize = size + progress * size * 0.08f;
        var expandedR = r + progress * r * 0.08f;
        var rect = new SKRect(
            cx - expandedSize / 2f, cy - expandedSize / 2f,
            cx + expandedSize / 2f, cy + expandedSize / 2f);
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke, StrokeWidth = 1f, IsAntialias = true,
            Color = accent.WithAlpha((byte)(40 * (1f - progress))),
        };
        canvas.DrawRoundRect(rect, expandedR, expandedR, paint);
    }

    private SKColor ResolveAccentColor()
    {
        if (_owner.AccentBrush is Microsoft.UI.Xaml.Media.SolidColorBrush b)
            return new SKColor(b.Color.R, b.Color.G, b.Color.B, b.Color.A);
        return new SKColor(212, 169, 89, 255);
    }
}

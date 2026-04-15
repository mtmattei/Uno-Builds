using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

/// <summary>
/// Cone-of-light indicator for menu mode. One wedge path drawn through three blur passes,
/// plus a clipped amber metal-grain pass and a center convergence glow.
///
/// Caller must apply the knob rotation transform before calling so the cone rotates with the knob.
/// The cone path is built pointing "up" (-90° in screen frame) — the rotation does the rest.
///
/// All three blur filters + the cone path itself are cached and only rebuilt
/// when the knob radius, half-angle, or blur sigmas actually change.
/// </summary>
internal sealed class ConeLightRenderer
{
    private readonly SKPaint _conePaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    private readonly SKPaint _grainPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 0.6f,
        IsAntialias = true,
    };

    private readonly SKPaint _centerPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    // ── Cached geometry (rebuilt only when cx/cy/knobR/halfAngleDeg change) ──
    private SKPath? _cachedConePath;
    private float _cachedCx = float.NaN;
    private float _cachedCy = float.NaN;
    private float _cachedKnobR = float.NaN;
    private float _cachedHalfAngle = float.NaN;

    // ── Cached blur mask filters (rebuilt only when a sigma changes) ─────────
    private SKMaskFilter? _blur1;
    private SKMaskFilter? _blur2;
    private SKMaskFilter? _blur3;
    private float _blur1Sigma = -1f;
    private float _blur2Sigma = -1f;
    private float _blur3Sigma = -1f;

    private const float DEG_TO_RAD = MathF.PI / 180f;

    public void Draw(
        SKCanvas canvas,
        float cx,
        float cy,
        float knobR,
        float halfAngleDeg,
        float blurSigma1,
        float blurSigma2,
        float blurSigma3,
        SKColor accent)
    {
        EnsureConePath(cx, cy, knobR, halfAngleDeg);
        EnsureBlurFilters(blurSigma1, blurSigma2, blurSigma3);
        if (_cachedConePath is null) return;

        // ── Pass 1: deep diffuse (widest blur, lowest opacity). ──
        _conePaint.MaskFilter = _blur1;
        _conePaint.Color = accent.WithAlpha((byte)(0.09f * 255f));
        canvas.DrawPath(_cachedConePath, _conePaint);

        // ── Pass 2: medium body. ──
        _conePaint.MaskFilter = _blur2;
        _conePaint.Color = accent.WithAlpha((byte)(0.06f * 255f));
        canvas.DrawPath(_cachedConePath, _conePaint);

        // ── Pass 3: tight core. ──
        _conePaint.MaskFilter = _blur3;
        _conePaint.Color = accent.WithAlpha((byte)(0.04f * 255f));
        canvas.DrawPath(_cachedConePath, _conePaint);

        _conePaint.MaskFilter = null;

        // ── Clipped amber metal grain — re-draw the radial lines inside the cone. ──
        canvas.Save();
        canvas.ClipPath(_cachedConePath, antialias: true);

        for (int i = 0; i < 72; i++)
        {
            var a = (i / 72f) * MathF.PI * 2f;
            var alphaF = 5f + MathF.Sin(a * 3f) * 2.5f;
            var alpha = (byte)Math.Clamp(alphaF, 1f, 12f);
            _grainPaint.Color = accent.WithAlpha(alpha);

            var x1 = cx + MathF.Cos(a) * knobR * 0.20f;
            var y1 = cy + MathF.Sin(a) * knobR * 0.20f;
            var x2 = cx + MathF.Cos(a) * knobR * 0.97f;
            var y2 = cy + MathF.Sin(a) * knobR * 0.97f;
            canvas.DrawLine(x1, y1, x2, y2, _grainPaint);
        }

        canvas.Restore();

        // ── Center convergence — soft small glow at the knob center. ──
        _centerPaint.MaskFilter = _blur1;
        _centerPaint.Color = accent.WithAlpha((byte)(0.035f * 255f));
        canvas.DrawCircle(cx, cy, 16f, _centerPaint);
        _centerPaint.MaskFilter = null;
    }

    private void EnsureConePath(float cx, float cy, float knobR, float halfAngleDeg)
    {
        if (_cachedConePath is not null
            && _cachedCx == cx
            && _cachedCy == cy
            && _cachedKnobR == knobR
            && MathF.Abs(_cachedHalfAngle - halfAngleDeg) < 0.01f)
        {
            return;
        }

        _cachedConePath?.Dispose();
        _cachedConePath = BuildConePath(cx, cy, knobR, halfAngleDeg);
        _cachedCx = cx;
        _cachedCy = cy;
        _cachedKnobR = knobR;
        _cachedHalfAngle = halfAngleDeg;
    }

    private void EnsureBlurFilters(float s1, float s2, float s3)
    {
        if (_blur1 is null || MathF.Abs(s1 - _blur1Sigma) > 0.25f)
        {
            _blur1?.Dispose();
            _blur1 = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, s1);
            _blur1Sigma = s1;
        }
        if (_blur2 is null || MathF.Abs(s2 - _blur2Sigma) > 0.25f)
        {
            _blur2?.Dispose();
            _blur2 = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, s2);
            _blur2Sigma = s2;
        }
        if (_blur3 is null || MathF.Abs(s3 - _blur3Sigma) > 0.25f)
        {
            _blur3?.Dispose();
            _blur3 = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, s3);
            _blur3Sigma = s3;
        }
    }

    /// <summary>
    /// Builds a wedge: an arc at the knob perimeter (between -90 ± half), straight edges
    /// inward to a small arc at radius 10 near the center.
    /// </summary>
    private static SKPath BuildConePath(float cx, float cy, float knobR, float halfAngleDeg)
    {
        const float tipR = 10f;
        const float tipHalfDeg = 3f;

        var leftEdgeDeg = -90f - halfAngleDeg;
        var rightEdgeDeg = -90f + halfAngleDeg;
        var leftTipDeg = -90f - tipHalfDeg;
        var rightTipDeg = -90f + tipHalfDeg;

        var le = leftEdgeDeg * DEG_TO_RAD;
        var rt = rightTipDeg * DEG_TO_RAD;

        var path = new SKPath();
        var outerRect = new SKRect(cx - knobR, cy - knobR, cx + knobR, cy + knobR);
        var innerRect = new SKRect(cx - tipR, cy - tipR, cx + tipR, cy + tipR);

        path.MoveTo(cx + MathF.Cos(le) * knobR, cy + MathF.Sin(le) * knobR);

        var outerSweep = rightEdgeDeg - leftEdgeDeg;
        path.AddArc(outerRect, leftEdgeDeg, outerSweep);

        path.LineTo(cx + MathF.Cos(rt) * tipR, cy + MathF.Sin(rt) * tipR);

        var innerSweep = leftTipDeg - rightTipDeg;
        path.AddArc(innerRect, rightTipDeg, innerSweep);

        path.Close();
        return path;
    }
}

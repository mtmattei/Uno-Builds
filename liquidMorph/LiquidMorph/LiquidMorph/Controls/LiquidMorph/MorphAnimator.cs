using System;
using System.Diagnostics;
using Microsoft.UI.Dispatching;

namespace LiquidMorph.Controls.LiquidMorph;

/// <summary>
/// Two-phase animator for the liquid morph transition.
/// Total duration: 1600ms with asymmetric midpoint (55% exit / 45% enter).
/// Exit is slower (dissolve accelerates into chaos), enter is faster (snaps to clarity).
///
/// Animated properties per frame:
///   - Displacement (turbulence warp intensity)
///   - Blur (gaussian softening)
///   - Scale (1.0→1.06 on exit, 0.94→1.0 on enter - "breathing")
///   - ContentOpacity (1→0 on exit, 0→1 on enter - dissolve/coalesce)
///
/// All curves are pre-baked into lookup tables at animation start.
/// </summary>
public sealed class MorphAnimator
{
    private readonly DispatcherQueueTimer _timer;
    private readonly Stopwatch _stopwatch = new();

    private Action? _onMidpoint;
    private Action? _onComplete;
    private bool _midpointFired;

    private const int LutSize = 200;
    private readonly float[] _displacementLut = new float[LutSize + 1];
    private readonly float[] _blurLut = new float[LutSize + 1];
    private readonly float[] _scaleLut = new float[LutSize + 1];
    private readonly float[] _opacityLut = new float[LutSize + 1];

    public TimeSpan TotalDuration { get; set; } = TimeSpan.FromMilliseconds(2000);
    public float MaxDisplacement { get; set; } = 160f;
    public float MaxBlur { get; set; } = 32f;

    /// <summary>
    /// Where the midpoint falls in the 0..1 range.
    /// 0.55 = exit takes 55% of duration (slower dissolve), enter takes 45% (snappier).
    /// </summary>
    public float MidpointRatio { get; set; } = 0.55f;

    public float CurrentDisplacement { get; private set; }
    public float CurrentBlur { get; private set; }
    public float CurrentProgress { get; private set; }
    public float CurrentScale { get; private set; } = 1f;
    public float CurrentContentOpacity { get; private set; } = 1f;

    public event Action? FrameUpdated;

    public MorphAnimator(DispatcherQueue dispatcherQueue)
    {
        _timer = dispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.IsRepeating = true;
        _timer.Tick += OnTick;
    }

    public void StartTransition(Action onMidpoint, Action onComplete)
    {
        _onMidpoint = onMidpoint;
        _onComplete = onComplete;
        _midpointFired = false;

        float mid = MidpointRatio;

        for (int i = 0; i <= LutSize; i++)
        {
            double t = (double)i / LutSize;
            _displacementLut[i] = MaxDisplacement * DisplacementCurve(t, mid);
            _blurLut[i] = MaxBlur * BlurCurve(t, mid);
            _scaleLut[i] = ScaleCurve(t, mid);
            _opacityLut[i] = OpacityCurve(t, mid);
        }

        _stopwatch.Restart();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _stopwatch.Stop();
        CurrentDisplacement = 0;
        CurrentBlur = 0;
        CurrentScale = 1f;
        CurrentContentOpacity = 1f;
    }

    private void OnTick(DispatcherQueueTimer sender, object args)
    {
        var t = Math.Clamp(
            _stopwatch.Elapsed.TotalMilliseconds / TotalDuration.TotalMilliseconds,
            0, 1);

        if (!_midpointFired && t >= MidpointRatio)
        {
            _midpointFired = true;
            _onMidpoint?.Invoke();
        }

        CurrentProgress = (float)t;

        int idx = Math.Clamp((int)(t * LutSize), 0, LutSize);
        CurrentDisplacement = _displacementLut[idx];
        CurrentBlur = _blurLut[idx];
        CurrentScale = _scaleLut[idx];
        CurrentContentOpacity = _opacityLut[idx];

        FrameUpdated?.Invoke();

        if (t >= 1.0)
        {
            _timer.Stop();
            _stopwatch.Stop();
            CurrentDisplacement = 0;
            CurrentBlur = 0;
            CurrentScale = 1f;
            CurrentContentOpacity = 1f;
            FrameUpdated?.Invoke();
            _onComplete?.Invoke();
        }
    }

    // --- Curve functions (used only during LUT generation) ---

    /// <summary>
    /// Displacement: 0 → max at midpoint → 0.
    /// Exit uses ease-in (accelerates into chaos), enter uses ease-out (snaps to calm).
    /// </summary>
    private static float DisplacementCurve(double t, float mid)
    {
        if (t <= mid)
        {
            double localT = t / mid;
            return (float)CubicBezierEase(localT, 0.45, 0.0, 0.8, 0.3);
        }
        else
        {
            double localT = (t - mid) / (1.0 - mid);
            return (float)(1.0 - CubicBezierEase(localT, 0.2, 0.7, 0.55, 1.0));
        }
    }

    /// <summary>
    /// Blur: lags displacement onset with intermediate keyframe at 30%.
    /// </summary>
    private static float BlurCurve(double t, float mid)
    {
        if (t <= mid)
        {
            double localT = t / mid;
            if (localT <= 0.5)
            {
                double subT = localT / 0.5;
                return (float)(0.3 * CubicBezierEase(subT, 0.45, 0.0, 0.8, 0.3));
            }
            else
            {
                double subT = (localT - 0.5) / 0.5;
                return (float)(0.3 + 0.7 * CubicBezierEase(subT, 0.45, 0.0, 0.8, 0.3));
            }
        }
        else
        {
            double localT = (t - mid) / (1.0 - mid);
            if (localT <= 0.5)
            {
                double subT = localT / 0.5;
                return (float)(1.0 - 0.7 * CubicBezierEase(subT, 0.2, 0.7, 0.55, 1.0));
            }
            else
            {
                double subT = (localT - 0.5) / 0.5;
                return (float)(0.3 * (1.0 - CubicBezierEase(subT, 0.2, 0.7, 0.55, 1.0)));
            }
        }
    }

    /// <summary>
    /// Scale breathing:
    /// Exit: 1.0 → 1.06 (gentle expand as it dissolves)
    /// Enter: 0.94 → 1.0 (contracts back to natural size)
    /// Jump at midpoint is hidden by peak distortion + zero opacity.
    /// </summary>
    private static float ScaleCurve(double t, float mid)
    {
        if (t <= mid)
        {
            double localT = t / mid;
            float eased = (float)CubicBezierEase(localT, 0.45, 0.0, 0.8, 0.3);
            return 1.0f + 0.06f * eased;
        }
        else
        {
            double localT = (t - mid) / (1.0 - mid);
            float eased = (float)CubicBezierEase(localT, 0.2, 0.7, 0.55, 1.0);
            return 0.94f + 0.06f * eased;
        }
    }

    /// <summary>
    /// Content opacity:
    /// Exit: 1.0 → 0.0 (ease-in - slow start, accelerates to invisible)
    /// Enter: 0.0 → 1.0 (ease-out - appears quickly, settles)
    /// </summary>
    private static float OpacityCurve(double t, float mid)
    {
        if (t <= mid)
        {
            double localT = t / mid;
            float eased = (float)CubicBezierEase(localT, 0.42, 0.0, 0.9, 0.2);
            return 1.0f - eased;
        }
        else
        {
            double localT = (t - mid) / (1.0 - mid);
            float eased = (float)CubicBezierEase(localT, 0.1, 0.8, 0.4, 1.0);
            return eased;
        }
    }

    private static double CubicBezierEase(double t, double x1, double y1, double x2, double y2)
    {
        if (t <= 0) return 0;
        if (t >= 1) return 1;

        double u = t;
        for (int i = 0; i < 8; i++)
        {
            double xGuess = BezierComponent(u, x1, x2);
            double dx = BezierComponentDerivative(u, x1, x2);
            if (Math.Abs(dx) < 1e-8) break;
            u -= (xGuess - t) / dx;
            u = Math.Clamp(u, 0, 1);
        }

        return BezierComponent(u, y1, y2);
    }

    private static double BezierComponent(double u, double p1, double p2)
    {
        double cu = 1.0 - u;
        return 3.0 * cu * cu * u * p1 + 3.0 * cu * u * u * p2 + u * u * u;
    }

    private static double BezierComponentDerivative(double u, double p1, double p2)
    {
        double cu = 1.0 - u;
        return 3.0 * cu * cu * p1 + 6.0 * cu * u * (p2 - p1) + 3.0 * u * u * (1.0 - p2);
    }
}

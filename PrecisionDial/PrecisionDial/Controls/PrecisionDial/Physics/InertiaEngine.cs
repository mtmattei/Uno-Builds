using System;

namespace PrecisionDial.Controls;

public sealed class InertiaEngine
{
    // Animation tick interval (matches the DispatcherTimer in PrecisionDial.cs).
    private const double TickDt = 0.016;

    // Tunables. The previous v2 numbers were tuned against a units bug — the engine
    // consumed VelocityTracker output (units per SECOND) as if it were units per TICK,
    // a ~60× over-amplification that made any non-trivial drag fling the dial across
    // the entire range. With the units corrected (delta = velocity * dt below), the
    // tunables here are relative to a true units-per-second velocity.

    // Fraction of measured drag velocity that becomes inertia velocity on release.
    private const double TransferRatio = 0.40;
    // Hard cap on initial inertia velocity in units per second — keeps a fast flick
    // from launching the dial across the full range. With TransferRatio 0.40 and
    // decay sum factor ~0.184, the maximum post-release tail is ~11 value units.
    private const double MaxInitialVelocity = 150.0;
    // Stop threshold in units per second.
    private const double StopVelocity = 5.0;

    private double _velocity; // units per second
    private bool _active;

    public void Start(double initialVelocityPerSecond)
    {
        var sign = Math.Sign(initialVelocityPerSecond);
        var magnitude = Math.Min(Math.Abs(initialVelocityPerSecond), MaxInitialVelocity);
        _velocity = sign * magnitude * TransferRatio;
        _active = Math.Abs(_velocity) > StopVelocity;
    }

    public double? Step(double decayRate, double currentValue, double min, double max)
    {
        if (!_active) return null;

        _velocity *= decayRate;

        // delta in value units = velocity (units/sec) × tick interval (sec)
        var delta = _velocity * TickDt;
        var newValue = Math.Clamp(currentValue + delta, min, max);

        if (Math.Abs(_velocity) < StopVelocity || newValue <= min || newValue >= max)
        {
            _active = false;
            return newValue - currentValue;
        }

        return newValue - currentValue;
    }

    public double CurrentVelocity => _velocity;
    public bool IsActive => _active;
    public void Cancel() => _active = false;
}

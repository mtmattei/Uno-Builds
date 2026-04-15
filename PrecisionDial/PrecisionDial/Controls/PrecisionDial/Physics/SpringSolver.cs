using System;

namespace PrecisionDial.Controls;

public sealed class SpringSolver
{
    private double _current;
    private double _velocity;
    private double _target;
    private bool _settled = true;

    public double Stiffness { get; set; } = 400;
    public double Damping { get; set; } = 30;
    public double Mass { get; set; } = 1;

    public void SetTarget(double target)
    {
        _target = target;
        _settled = false;
    }

    public void SnapTo(double value)
    {
        _current = value; _target = value;
        _velocity = 0; _settled = true;
    }

    public double Step(double dt)
    {
        if (_settled) return _current;

        var displacement = _current - _target;
        var springForce = -Stiffness * displacement;
        var dampingForce = -Damping * _velocity;
        var accel = (springForce + dampingForce) / Mass;

        _velocity += accel * dt;
        _current += _velocity * dt;

        if (Math.Abs(displacement) < 0.01 && Math.Abs(_velocity) < 0.01)
        {
            _current = _target;
            _velocity = 0;
            _settled = true;
        }

        return _current;
    }

    public double Current => _current;
    public bool IsSettled => _settled;
}

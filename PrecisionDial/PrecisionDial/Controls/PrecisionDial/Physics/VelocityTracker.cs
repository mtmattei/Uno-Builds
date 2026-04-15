using System;

namespace PrecisionDial.Controls;

public sealed class VelocityTracker
{
    private readonly double[] _samples;
    private int _index;
    private int _count;

    public VelocityTracker(int windowSize = 5)
    {
        _samples = new double[windowSize];
    }

    public void AddSample(double velocity)
    {
        _samples[_index] = velocity;
        _index = (_index + 1) % _samples.Length;
        if (_count < _samples.Length) _count++;
    }

    public double GetAverageVelocity()
    {
        if (_count == 0) return 0;
        double sum = 0;
        for (int i = 0; i < _count; i++) sum += _samples[i];
        return sum / _count;
    }

    public void Reset()
    {
        _index = 0;
        _count = 0;
    }
}

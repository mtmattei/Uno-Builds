using System;
using Windows.Foundation;

namespace PrecisionDial.Controls;

public sealed class AngularDragHandler
{
    public double ComputeValueDelta(
        Point pointerPos, Point center,
        ref double lastAngle, double arcSweep, double range)
    {
        var dx = pointerPos.X - center.X;
        var dy = pointerPos.Y - center.Y;
        var currentAngle = Math.Atan2(dy, dx) * (180.0 / Math.PI);

        var delta = currentAngle - lastAngle;
        if (delta > 180) delta -= 360;
        if (delta < -180) delta += 360;

        lastAngle = currentAngle;
        return (delta / arcSweep) * range;
    }
}

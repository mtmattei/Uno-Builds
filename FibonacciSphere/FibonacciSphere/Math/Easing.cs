using System;

namespace FibonacciSphere.Math;

/// <summary>
/// Easing functions for smooth animations.
/// </summary>
public static class Easing
{
    public enum EasingType
    {
        Linear,
        EaseInOut,
        Elastic
    }

    /// <summary>
    /// Applies the specified easing function to a value.
    /// </summary>
    /// <param name="t">Input value (0 to 1)</param>
    /// <param name="type">Type of easing to apply</param>
    /// <returns>Eased value</returns>
    public static float Apply(float t, EasingType type)
    {
        return type switch
        {
            EasingType.Linear => Linear(t),
            EasingType.EaseInOut => EaseInOut(t),
            EasingType.Elastic => Elastic(t),
            _ => Linear(t)
        };
    }

    /// <summary>
    /// Linear interpolation (no easing).
    /// </summary>
    public static float Linear(float t)
    {
        return t;
    }

    /// <summary>
    /// Smooth ease-in-out using sine function.
    /// </summary>
    public static float EaseInOut(float t)
    {
        return -(MathF.Cos(MathF.PI * t) - 1f) / 2f;
    }

    /// <summary>
    /// Elastic ease-out for bouncy effects.
    /// </summary>
    public static float Elastic(float t)
    {
        if (t == 0f)
        {
            return 0f;
        }

        if (t == 1f)
        {
            return 1f;
        }

        const float c4 = (2f * MathF.PI) / 3f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
}

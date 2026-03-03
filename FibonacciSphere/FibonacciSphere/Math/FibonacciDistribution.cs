using System;
using System.Collections.Generic;
using System.Numerics;

namespace FibonacciSphere.Math;

/// <summary>
/// Generates points distributed evenly across a sphere using the Fibonacci sphere algorithm.
/// </summary>
public static class FibonacciDistribution
{
    private static readonly float GoldenRatio = (1f + MathF.Sqrt(5f)) / 2f;
    private static readonly float GoldenAngle = 2f * MathF.PI / (GoldenRatio * GoldenRatio);

    /// <summary>
    /// Generates N points evenly distributed on a unit sphere using the golden angle.
    /// </summary>
    /// <param name="count">Number of points to generate</param>
    /// <returns>List of 3D positions on a unit sphere</returns>
    public static List<Vector3> GenerateFibonacciSphere(int count)
    {
        if (count <= 0)
        {
            return new List<Vector3>();
        }

        var points = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            // Y coordinate from -1 to 1
            float y = 1f - (2f * i / (count - 1));

            // Radius at this Y level (circle of latitude)
            float radius = MathF.Sqrt(1f - y * y);

            // Angle around the Y axis using the golden angle
            float theta = GoldenAngle * i;

            float x = MathF.Cos(theta) * radius;
            float z = MathF.Sin(theta) * radius;

            points.Add(new Vector3(x, y, z));
        }

        return points;
    }

    /// <summary>
    /// Generates a random phase offset for each point to create varied wobble effects.
    /// </summary>
    /// <param name="count">Number of phase values to generate</param>
    /// <returns>Array of phase offsets in radians (0 to 2*PI)</returns>
    public static float[] GenerateWobblePhases(int count)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var phases = new float[count];

        for (int i = 0; i < count; i++)
        {
            phases[i] = (float)(random.NextDouble() * 2 * System.Math.PI);
        }

        return phases;
    }
}

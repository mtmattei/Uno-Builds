using System;
using System.Numerics;

namespace FibonacciSphere.Math;

/// <summary>
/// Extension methods for Vector3 operations.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Rotates a vector around the Y axis by the specified angle.
    /// </summary>
    /// <param name="vector">The vector to rotate</param>
    /// <param name="angle">Rotation angle in radians</param>
    /// <returns>Rotated vector</returns>
    public static Vector3 RotateY(this Vector3 vector, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        return new Vector3(
            vector.X * cos + vector.Z * sin,
            vector.Y,
            -vector.X * sin + vector.Z * cos
        );
    }

    /// <summary>
    /// Rotates a vector around the X axis by the specified angle.
    /// </summary>
    /// <param name="vector">The vector to rotate</param>
    /// <param name="angle">Rotation angle in radians</param>
    /// <returns>Rotated vector</returns>
    public static Vector3 RotateX(this Vector3 vector, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        return new Vector3(
            vector.X,
            vector.Y * cos - vector.Z * sin,
            vector.Y * sin + vector.Z * cos
        );
    }

    /// <summary>
    /// Rotates a vector around the Z axis by the specified angle.
    /// </summary>
    public static Vector3 RotateZ(this Vector3 vector, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        return new Vector3(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos,
            vector.Z
        );
    }

    /// <summary>
    /// Combined Y then X rotation - more efficient than two separate calls.
    /// </summary>
    public static Vector3 RotateYX(this Vector3 vector, float angleY, float angleX)
    {
        float cosY = MathF.Cos(angleY);
        float sinY = MathF.Sin(angleY);
        float cosX = MathF.Cos(angleX);
        float sinX = MathF.Sin(angleX);

        // First rotate around Y
        float x1 = vector.X * cosY + vector.Z * sinY;
        float y1 = vector.Y;
        float z1 = -vector.X * sinY + vector.Z * cosY;

        // Then rotate around X
        return new Vector3(
            x1,
            y1 * cosX - z1 * sinX,
            y1 * sinX + z1 * cosX
        );
    }

    /// <summary>
    /// Rotates a vector around an arbitrary axis.
    /// </summary>
    /// <param name="vector">The vector to rotate</param>
    /// <param name="axis">The axis to rotate around (should be normalized)</param>
    /// <param name="angle">Rotation angle in radians</param>
    /// <returns>Rotated vector</returns>
    public static Vector3 RotateAround(this Vector3 vector, Vector3 axis, float angle)
    {
        var rotation = Quaternion.CreateFromAxisAngle(axis, angle);
        return Vector3.Transform(vector, rotation);
    }

    /// <summary>
    /// Returns the direction from one point to another, normalized.
    /// </summary>
    public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
    {
        return Vector3.Normalize(to - from);
    }

    /// <summary>
    /// Calculates the squared distance between two points (faster than Distance when only comparing).
    /// </summary>
    public static float DistanceSquared(this Vector3 a, Vector3 b)
    {
        var diff = b - a;
        return Vector3.Dot(diff, diff);
    }

    /// <summary>
    /// Linear interpolation between two vectors.
    /// </summary>
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return a + (b - a) * t;
    }

    /// <summary>
    /// Spherical linear interpolation between two vectors.
    /// </summary>
    public static Vector3 Slerp(Vector3 a, Vector3 b, float t)
    {
        float dot = Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b));
        dot = System.Math.Clamp(dot, -1f, 1f);
        float theta = MathF.Acos(dot) * t;

        Vector3 relativeVec = Vector3.Normalize(b - a * dot);
        return a * MathF.Cos(theta) + relativeVec * MathF.Sin(theta);
    }
}

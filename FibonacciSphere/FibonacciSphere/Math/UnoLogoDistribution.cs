using System;
using System.Collections.Generic;
using System.Numerics;

namespace FibonacciSphere.Math;

/// <summary>
/// Generates points distributed along the Uno Platform logo shape:
/// 4 interlocking rounded rectangle rings arranged in a diamond/pinwheel pattern.
/// </summary>
public static class UnoLogoDistribution
{
    /// <summary>
    /// Generates points distributed along the Uno Platform logo - 4 interlocking rings.
    /// </summary>
    /// <param name="count">Total number of points to generate</param>
    /// <returns>List of 3D positions forming the logo</returns>
    public static List<Vector3> GenerateUnoLogoParametric(int count)
    {
        if (count <= 0)
        {
            return new List<Vector3>();
        }

        var points = new List<Vector3>(count);

        // Divide points among the 4 rings
        int pointsPerRing = count / 4;
        int remainder = count % 4;

        // Ring parameters
        float ringWidth = 0.7f;      // Width of each rounded rectangle
        float ringHeight = 0.7f;     // Height of each rounded rectangle
        float cornerRadius = 0.18f;  // Radius of the rounded corners
        float tubeRadius = 0.055f;   // Thickness of the ring tube

        // Golden angle for tube distribution
        float goldenRatio = (1f + MathF.Sqrt(5f)) / 2f;
        float goldenAngle = 2f * MathF.PI / (goldenRatio * goldenRatio);

        int globalIndex = 0;

        // Ring rotations (in radians) - positioned as diamond pattern
        // The logo has 4 rings at 45° intervals, interlocking
        float[] ringRotationsZ = {
            MathF.PI / 4f,           // Ring 0: 45° - diagonal (blue)
            -MathF.PI / 4f,          // Ring 1: -45° - other diagonal (green)
            MathF.PI / 4f,           // Ring 2: 45° - diagonal (purple)
            -MathF.PI / 4f           // Ring 3: -45° - other diagonal (pink/red)
        };

        // Offset each ring in 3D space for the interlocking effect
        // The rings form a 2x2 grid pattern when viewed from front
        float offsetAmount = 0.22f;
        float zOffset = 0.03f;
        Vector3[] ringOffsets = {
            new Vector3(-offsetAmount, -offsetAmount, zOffset),    // Blue - bottom left
            new Vector3(offsetAmount, -offsetAmount, -zOffset),    // Green - bottom right
            new Vector3(offsetAmount, offsetAmount, zOffset),      // Purple - top right
            new Vector3(-offsetAmount, offsetAmount, -zOffset)     // Pink - top left
        };

        for (int ring = 0; ring < 4; ring++)
        {
            int ringPoints = pointsPerRing + (ring < remainder ? 1 : 0);
            float rotationZ = ringRotationsZ[ring];
            Vector3 offset = ringOffsets[ring];

            for (int i = 0; i < ringPoints; i++)
            {
                // Parameter along the rounded rectangle path (0 to 1)
                float t = (float)i / ringPoints;

                // Get position on rounded rectangle
                Vector3 pathPoint = GetRoundedRectPoint(t, ringWidth, ringHeight, cornerRadius);

                // Add tube thickness using golden angle
                float tubeAngle = goldenAngle * globalIndex;
                Vector3 tubeOffset = GetTubeOffset(pathPoint, t, ringWidth, ringHeight, cornerRadius, tubeRadius, tubeAngle);

                // Apply rotation around Z axis
                Vector3 rotatedPoint = RotateZ(pathPoint + tubeOffset, rotationZ);

                // Apply ring offset for interlocking
                points.Add(rotatedPoint + offset);

                globalIndex++;
            }
        }

        // Normalize to fit in reasonable bounds
        NormalizePoints(points);

        return points;
    }

    /// <summary>
    /// Gets a point on a rounded rectangle path.
    /// </summary>
    private static Vector3 GetRoundedRectPoint(float t, float width, float height, float cornerRadius)
    {
        // Total perimeter calculation
        float straightWidth = width - 2 * cornerRadius;
        float straightHeight = height - 2 * cornerRadius;
        float cornerArc = MathF.PI * cornerRadius / 2f; // Quarter circle
        float totalLength = 2 * straightWidth + 2 * straightHeight + 4 * cornerArc;

        float distance = t * totalLength;

        float halfW = width / 2f;
        float halfH = height / 2f;
        float cr = cornerRadius;

        // Segment lengths
        float seg1 = straightWidth;
        float seg2 = seg1 + cornerArc;
        float seg3 = seg2 + straightHeight;
        float seg4 = seg3 + cornerArc;
        float seg5 = seg4 + straightWidth;
        float seg6 = seg5 + cornerArc;
        float seg7 = seg6 + straightHeight;

        if (distance < seg1)
        {
            // Bottom straight
            float localT = distance / straightWidth;
            return new Vector3(-halfW + cr + localT * straightWidth, -halfH, 0);
        }
        else if (distance < seg2)
        {
            // Bottom-right corner
            float localT = (distance - seg1) / cornerArc;
            float angle = -MathF.PI / 2f + localT * MathF.PI / 2f;
            return new Vector3(halfW - cr + MathF.Cos(angle) * cr, -halfH + cr + MathF.Sin(angle) * cr, 0);
        }
        else if (distance < seg3)
        {
            // Right straight
            float localT = (distance - seg2) / straightHeight;
            return new Vector3(halfW, -halfH + cr + localT * straightHeight, 0);
        }
        else if (distance < seg4)
        {
            // Top-right corner
            float localT = (distance - seg3) / cornerArc;
            float angle = 0 + localT * MathF.PI / 2f;
            return new Vector3(halfW - cr + MathF.Cos(angle) * cr, halfH - cr + MathF.Sin(angle) * cr, 0);
        }
        else if (distance < seg5)
        {
            // Top straight
            float localT = (distance - seg4) / straightWidth;
            return new Vector3(halfW - cr - localT * straightWidth, halfH, 0);
        }
        else if (distance < seg6)
        {
            // Top-left corner
            float localT = (distance - seg5) / cornerArc;
            float angle = MathF.PI / 2f + localT * MathF.PI / 2f;
            return new Vector3(-halfW + cr + MathF.Cos(angle) * cr, halfH - cr + MathF.Sin(angle) * cr, 0);
        }
        else if (distance < seg7)
        {
            // Left straight
            float localT = (distance - seg6) / straightHeight;
            return new Vector3(-halfW, halfH - cr - localT * straightHeight, 0);
        }
        else
        {
            // Bottom-left corner
            float localT = (distance - seg7) / cornerArc;
            float angle = MathF.PI + localT * MathF.PI / 2f;
            return new Vector3(-halfW + cr + MathF.Cos(angle) * cr, -halfH + cr + MathF.Sin(angle) * cr, 0);
        }
    }

    /// <summary>
    /// Gets the tube offset for a point on the path.
    /// </summary>
    private static Vector3 GetTubeOffset(Vector3 pathPoint, float t, float width, float height, float cornerRadius, float tubeRadius, float tubeAngle)
    {
        // Calculate normal direction (perpendicular to path, pointing outward)
        float epsilon = 0.001f;
        float tNext = (t + epsilon) % 1f;
        if (tNext < t) tNext = t + epsilon; // Handle wrap-around
        Vector3 nextPoint = GetRoundedRectPoint(System.Math.Min(tNext, 0.999f), width, height, cornerRadius);
        Vector3 diff = nextPoint - pathPoint;

        // Avoid zero-length tangent
        if (diff.LengthSquared() < 0.0001f)
        {
            diff = new Vector3(1, 0, 0);
        }
        Vector3 tangent = Vector3.Normalize(diff);

        // Normal in XY plane (perpendicular to tangent)
        Vector3 normal = new Vector3(-tangent.Y, tangent.X, 0);

        // Binormal (Z direction)
        Vector3 binormal = Vector3.UnitZ;

        // Create tube offset using angle around the tube
        float offsetInPlane = MathF.Cos(tubeAngle) * tubeRadius;
        float offsetZ = MathF.Sin(tubeAngle) * tubeRadius;

        return normal * offsetInPlane + binormal * offsetZ;
    }

    /// <summary>
    /// Rotates a point around the Z axis.
    /// </summary>
    private static Vector3 RotateZ(Vector3 point, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Vector3(
            point.X * cos - point.Y * sin,
            point.X * sin + point.Y * cos,
            point.Z
        );
    }

    private static void NormalizePoints(List<Vector3> points)
    {
        if (points.Count == 0) return;

        // Find bounds
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in points)
        {
            minX = MathF.Min(minX, p.X);
            maxX = MathF.Max(maxX, p.X);
            minY = MathF.Min(minY, p.Y);
            maxY = MathF.Max(maxY, p.Y);
            minZ = MathF.Min(minZ, p.Z);
            maxZ = MathF.Max(maxZ, p.Z);
        }

        // Center and scale
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        float centerZ = (minZ + maxZ) / 2f;

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;
        float rangeZ = maxZ - minZ;
        float maxRange = MathF.Max(rangeX, MathF.Max(rangeY, MathF.Max(rangeZ, 0.001f)));
        float scale = 1.8f / maxRange;

        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            points[i] = new Vector3(
                (p.X - centerX) * scale,
                (p.Y - centerY) * scale,
                (p.Z - centerZ) * scale
            );
        }
    }
}

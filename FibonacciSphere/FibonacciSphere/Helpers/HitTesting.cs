using System.Collections.Generic;
using System.Numerics;
using FibonacciSphere.Models;
using FibonacciSphere.Rendering;

namespace FibonacciSphere.Helpers;

/// <summary>
/// Provides hit testing functionality for detecting which point is under the cursor.
/// </summary>
public static class HitTesting
{
    /// <summary>
    /// Finds the nearest point to the given screen position within a tolerance.
    /// </summary>
    /// <param name="points">All sphere points</param>
    /// <param name="screenPosition">Position in screen coordinates</param>
    /// <param name="screenSize">Screen dimensions</param>
    /// <param name="camera">Camera for projection</param>
    /// <param name="tolerance">Maximum distance to consider a hit (in pixels)</param>
    /// <returns>The nearest point if within tolerance, null otherwise</returns>
    public static SpherePoint? FindNearestPoint(
        IEnumerable<SpherePoint> points,
        Vector2 screenPosition,
        Vector2 screenSize,
        Camera3D camera,
        float tolerance = 20f)
    {
        SpherePoint? nearest = null;
        float minDistance = tolerance;
        float nearestDepth = float.MaxValue;

        foreach (var point in points)
        {
            var (projectedPos, depth) = camera.ProjectToScreen(point.CurrentPosition, screenSize);

            float distance = Vector2.Distance(screenPosition, projectedPos);

            // Check if this point is closer (prioritize by screen distance, then by depth)
            if (distance < minDistance || (distance < minDistance + 5f && depth < nearestDepth))
            {
                // Consider the point's visual size for hit testing
                float hitRadius = point.Size + 5f;
                if (distance <= hitRadius || distance < minDistance)
                {
                    minDistance = distance;
                    nearestDepth = depth;
                    nearest = point;
                }
            }
        }

        return nearest;
    }
}

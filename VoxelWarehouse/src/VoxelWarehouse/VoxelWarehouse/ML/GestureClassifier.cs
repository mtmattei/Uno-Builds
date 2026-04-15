using System;
using VoxelWarehouse.Models;

namespace VoxelWarehouse.ML;

/// <summary>
/// Pure geometry-based hand gesture classification from 21 MediaPipe landmarks.
/// Tuned for reliability over precision — uses normalized distances and relaxed thresholds.
/// </summary>
public static class GestureClassifier
{
    // Pinch: thumb-index tip distance relative to palm size
    private const float PinchThreshold = 0.35f;

    public static GestureType Classify(ReadOnlySpan<Landmark3D> lm)
    {
        if (lm.Length < 21)
            return GestureType.None;

        float palmSize = Distance(lm[0], lm[9]); // wrist to middle MCP
        if (palmSize < 1e-6f)
            return GestureType.None;

        // Check pinch first (highest priority action gesture)
        float pinchDist = Distance(lm[4], lm[8]) / palmSize;
        if (pinchDist < PinchThreshold)
            return GestureType.Pinch;

        // Check finger extension using tip-to-MCP vs PIP-to-MCP ratio
        bool indexUp = IsExtended(lm, 5, 6, 8, palmSize);
        bool middleUp = IsExtended(lm, 9, 10, 12, palmSize);
        bool ringUp = IsExtended(lm, 13, 14, 16, palmSize);
        bool pinkyUp = IsExtended(lm, 17, 18, 20, palmSize);

        int upCount = (indexUp ? 1 : 0) + (middleUp ? 1 : 0) + (ringUp ? 1 : 0) + (pinkyUp ? 1 : 0);

        // Point: index up, rest down
        if (indexUp && upCount == 1)
            return GestureType.Point;

        // Fist: all down
        if (upCount == 0)
            return GestureType.Fist;

        // Open: 3+ fingers up
        if (upCount >= 3)
            return GestureType.Open;

        // Default to open (move mode) for ambiguous poses
        return GestureType.Open;
    }

    /// <summary>
    /// Simple extension check: finger tip is farther from wrist than PIP joint.
    /// Uses the wrist (landmark 0) as the reference point.
    /// </summary>
    private static bool IsExtended(ReadOnlySpan<Landmark3D> lm, int mcp, int pip, int tip, float palmSize)
    {
        var wrist = lm[0];
        float tipDist = Distance(lm[tip], wrist);
        float pipDist = Distance(lm[pip], wrist);

        // Tip should be significantly farther than PIP from wrist
        // Using ratio > 1.15 (relaxed — was 1.3+, caused too many false negatives)
        return tipDist > pipDist * 1.15f;
    }

    private static float Distance(Landmark3D a, Landmark3D b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy); // 2D distance (Z is unreliable from monocular camera)
    }
}

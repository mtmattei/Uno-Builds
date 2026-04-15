using System;
using VoxelWarehouse.Models;

namespace VoxelWarehouse.ML;

/// <summary>
/// Result from the hand landmark model after decoding.
/// </summary>
public readonly record struct LandmarkResult(
    Landmark3D[] Landmarks,
    float Confidence,
    float Handedness);

/// <summary>
/// Decodes raw ONNX output from the MediaPipe hand landmark model.
/// Transforms 21 3D keypoints from the cropped/warped coordinate space
/// back to the original image coordinates using an inverse affine matrix.
/// </summary>
public static class LandmarkDecoder
{
    private const int LandmarkCount = 21;
    private const int CoordsPerLandmark = 3; // x, y, z
    private const float ConfidenceThreshold = 0.5f;

    /// <summary>
    /// Decodes hand landmark model output.
    /// </summary>
    /// <param name="rawLandmarks">Raw landmark coordinates [1, 63] — 21 landmarks x 3 (x, y, z).</param>
    /// <param name="rawConfidence">Hand presence confidence [1, 1].</param>
    /// <param name="rawHandedness">Handedness score [1, 1] — 0.0 = left, 1.0 = right.</param>
    /// <param name="inverseAffine">3x2 inverse affine [a, b, tx, c, d, ty] mapping landmark coords back to source image.</param>
    /// <param name="landmarkInputSize">Landmark model input size (e.g. 224).</param>
    /// <param name="imageWidth">Original source image width.</param>
    /// <param name="imageHeight">Original source image height.</param>
    /// <returns>Decoded landmark result, or null if confidence is below threshold.</returns>
    public static LandmarkResult? Decode(
        float[] rawLandmarks,
        float[] rawConfidence,
        float[] rawHandedness,
        float[] inverseAffine,
        int landmarkInputSize,
        int imageWidth,
        int imageHeight)
    {
        float confidence = Sigmoid(rawConfidence[0]);
        if (confidence < ConfidenceThreshold)
            return null;

        float handedness = Sigmoid(rawHandedness[0]);

        var landmarks = new Landmark3D[LandmarkCount];

        float a = inverseAffine[0], b = inverseAffine[1], tx = inverseAffine[2];
        float c = inverseAffine[3], d = inverseAffine[4], ty = inverseAffine[5];

        for (int i = 0; i < LandmarkCount; i++)
        {
            int idx = i * CoordsPerLandmark;

            // Raw coordinates are in landmark model input space [0, landmarkInputSize]
            float lx = rawLandmarks[idx];
            float ly = rawLandmarks[idx + 1];
            float lz = rawLandmarks[idx + 2];

            // Transform x, y back to original image coordinates using the inverse affine
            float srcX = a * lx + b * ly + tx;
            float srcY = c * lx + d * ly + ty;

            // Normalize to [0, 1] relative to the original image
            float normX = srcX / imageWidth;
            float normY = srcY / imageHeight;

            // Z is a relative depth, normalize by landmark input size for consistency
            float normZ = lz / landmarkInputSize;

            landmarks[i] = new Landmark3D(normX, normY, normZ);
        }

        return new LandmarkResult(landmarks, confidence, handedness);
    }

    /// <summary>
    /// Computes the centroid of all 21 landmarks in normalized image coordinates.
    /// Used for cursor positioning.
    /// </summary>
    public static (float X, float Y) ComputeCentroid(Landmark3D[] landmarks)
    {
        if (landmarks.Length == 0)
            return (0.5f, 0.5f);

        float sumX = 0, sumY = 0;
        for (int i = 0; i < landmarks.Length; i++)
        {
            sumX += landmarks[i].X;
            sumY += landmarks[i].Y;
        }

        return (sumX / landmarks.Length, sumY / landmarks.Length);
    }

    /// <summary>
    /// Computes a weighted cursor position using the index finger tip (primary)
    /// and the hand centroid (secondary) for stability.
    /// </summary>
    public static (float X, float Y) ComputeCursorPosition(Landmark3D[] landmarks, float tipWeight = 0.7f)
    {
        if (landmarks.Length < LandmarkCount)
            return ComputeCentroid(landmarks);

        var indexTip = landmarks[(int)HandLandmarkId.IndexTip];
        var (centX, centY) = ComputeCentroid(landmarks);

        float cursorX = indexTip.X * tipWeight + centX * (1f - tipWeight);
        float cursorY = indexTip.Y * tipWeight + centY * (1f - tipWeight);

        return (cursorX, cursorY);
    }

    private static float Sigmoid(float x)
    {
        if (x >= 0)
        {
            float ez = MathF.Exp(-x);
            return 1f / (1f + ez);
        }
        else
        {
            float ez = MathF.Exp(x);
            return ez / (1f + ez);
        }
    }
}

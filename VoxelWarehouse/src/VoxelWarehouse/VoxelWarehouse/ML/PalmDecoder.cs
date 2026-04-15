using System;
using System.Collections.Generic;

namespace VoxelWarehouse.ML;

/// <summary>
/// A detected palm bounding box from the palm detection model.
/// Coordinates are normalized [0, 1] relative to the input image.
/// </summary>
public readonly record struct PalmDetection(
    float CenterX, float CenterY,
    float Width, float Height,
    float RotationRadians,
    float Confidence,
    PalmKeypoint[] Keypoints);

/// <summary>
/// One of 7 keypoints output by the palm detection model.
/// Coordinates normalized [0, 1].
/// </summary>
public readonly record struct PalmKeypoint(float X, float Y);

/// <summary>
/// Decodes raw ONNX output from the MediaPipe palm detection model.
/// Applies sigmoid activation, anchor decoding, confidence thresholding, and NMS.
/// </summary>
public static class PalmDecoder
{
    private const int NumKeypoints = 7;
    private const int BoxDataSize = 4 + 2 * NumKeypoints; // cx, cy, w, h + 7 keypoint (x,y) pairs = 18
    private const float ConfidenceThreshold = 0.5f;
    private const float NmsIouThreshold = 0.3f;

    /// <summary>
    /// Decodes raw palm detection output against pre-computed anchors.
    /// </summary>
    /// <param name="rawBoxes">Raw bounding box regressions [numAnchors, 18].</param>
    /// <param name="rawScores">Raw confidence scores [numAnchors, 1].</param>
    /// <param name="anchors">Pre-computed anchor centers [numAnchors, 2] as flat array (cx, cy pairs).</param>
    /// <param name="inputSize">Model input size in pixels (e.g. 192).</param>
    /// <returns>List of detected palms after NMS, sorted by confidence descending.</returns>
    public static List<PalmDetection> Decode(
        float[] rawBoxes, float[] rawScores, float[] anchors, int inputSize)
    {
        int numAnchors = rawScores.Length;
        var candidates = new List<PalmDetection>();

        for (int i = 0; i < numAnchors; i++)
        {
            float score = Sigmoid(rawScores[i]);
            if (score < ConfidenceThreshold)
                continue;

            int boxIdx = i * BoxDataSize;
            float anchorCx = anchors[i * 2];
            float anchorCy = anchors[i * 2 + 1];

            // Decode box center and size relative to anchor
            float cx = rawBoxes[boxIdx] / inputSize + anchorCx;
            float cy = rawBoxes[boxIdx + 1] / inputSize + anchorCy;
            float w = rawBoxes[boxIdx + 2] / inputSize;
            float h = rawBoxes[boxIdx + 3] / inputSize;

            // Decode 7 keypoints
            var keypoints = new PalmKeypoint[NumKeypoints];
            for (int k = 0; k < NumKeypoints; k++)
            {
                int kIdx = boxIdx + 4 + k * 2;
                float kx = rawBoxes[kIdx] / inputSize + anchorCx;
                float ky = rawBoxes[kIdx + 1] / inputSize + anchorCy;
                keypoints[k] = new PalmKeypoint(kx, ky);
            }

            // Compute rotation from wrist (kp0) to middle finger MCP (kp2)
            float rotation = ComputeRotation(keypoints);

            candidates.Add(new PalmDetection(cx, cy, w, h, rotation, score, keypoints));
        }

        return NonMaxSuppression(candidates);
    }

    /// <summary>
    /// Loads pre-computed anchors from a binary file.
    /// Format: 4-byte int (count), then count * 2 floats (cx, cy pairs).
    /// </summary>
    public static float[] LoadAnchors(string path)
    {
        var bytes = System.IO.File.ReadAllBytes(path);
        int count = BitConverter.ToInt32(bytes, 0);
        var anchors = new float[count * 2];
        Buffer.BlockCopy(bytes, 4, anchors, 0, count * 2 * sizeof(float));
        return anchors;
    }

    /// <summary>
    /// Generates SSD anchors for the palm detection model.
    /// This matches the MediaPipe SSD anchor generation with strides [8, 16, 16, 16]
    /// and 2 anchors per grid cell.
    /// </summary>
    public static float[] GenerateAnchors(int inputSize)
    {
        int[] strides = [8, 16, 16, 16];
        int[] anchorsPerStride = [2, 6, 6, 6]; // MediaPipe palm detection anchor config

        var anchors = new List<float>();

        for (int layerIdx = 0; layerIdx < strides.Length; layerIdx++)
        {
            int stride = strides[layerIdx];
            int gridSize = (inputSize + stride - 1) / stride;
            int numAnchors = anchorsPerStride[layerIdx];

            for (int gy = 0; gy < gridSize; gy++)
            {
                for (int gx = 0; gx < gridSize; gx++)
                {
                    float cx = (gx + 0.5f) / gridSize;
                    float cy = (gy + 0.5f) / gridSize;

                    for (int a = 0; a < numAnchors; a++)
                    {
                        anchors.Add(cx);
                        anchors.Add(cy);
                    }
                }
            }
        }

        return anchors.ToArray();
    }

    /// <summary>
    /// Computes hand rotation angle from the palm keypoints.
    /// Uses the vector from wrist center (kp0) to middle finger MCP (kp2).
    /// </summary>
    private static float ComputeRotation(PalmKeypoint[] keypoints)
    {
        float dx = keypoints[2].X - keypoints[0].X;
        float dy = keypoints[2].Y - keypoints[0].Y;
        // Rotation to align hand vertically (pointing up)
        return MathF.Atan2(dx, -dy);
    }

    /// <summary>
    /// Weighted Non-Maximum Suppression. Keeps highest confidence detection and removes
    /// overlapping detections above the IoU threshold.
    /// </summary>
    private static List<PalmDetection> NonMaxSuppression(List<PalmDetection> detections)
    {
        detections.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

        var kept = new List<PalmDetection>();
        var suppressed = new bool[detections.Count];

        for (int i = 0; i < detections.Count; i++)
        {
            if (suppressed[i])
                continue;

            kept.Add(detections[i]);

            for (int j = i + 1; j < detections.Count; j++)
            {
                if (suppressed[j])
                    continue;

                if (ComputeIoU(detections[i], detections[j]) > NmsIouThreshold)
                    suppressed[j] = true;
            }
        }

        return kept;
    }

    /// <summary>
    /// Computes Intersection over Union between two axis-aligned bounding boxes.
    /// </summary>
    private static float ComputeIoU(PalmDetection a, PalmDetection b)
    {
        float aLeft = a.CenterX - a.Width / 2;
        float aRight = a.CenterX + a.Width / 2;
        float aTop = a.CenterY - a.Height / 2;
        float aBottom = a.CenterY + a.Height / 2;

        float bLeft = b.CenterX - b.Width / 2;
        float bRight = b.CenterX + b.Width / 2;
        float bTop = b.CenterY - b.Height / 2;
        float bBottom = b.CenterY + b.Height / 2;

        float interLeft = MathF.Max(aLeft, bLeft);
        float interRight = MathF.Min(aRight, bRight);
        float interTop = MathF.Max(aTop, bTop);
        float interBottom = MathF.Min(aBottom, bBottom);

        float interW = MathF.Max(0, interRight - interLeft);
        float interH = MathF.Max(0, interBottom - interTop);
        float interArea = interW * interH;

        float aArea = a.Width * a.Height;
        float bArea = b.Width * b.Height;
        float unionArea = aArea + bArea - interArea;

        return unionArea > 0 ? interArea / unionArea : 0;
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

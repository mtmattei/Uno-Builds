using System;

namespace VoxelWarehouse.ML;

/// <summary>
/// Frame preprocessing utilities for ONNX hand tracking models.
/// All operations use raw byte/float array math with no external image library dependencies.
/// </summary>
public static class ImagePreprocessor
{
    /// <summary>
    /// Converts a BGRA8 pixel buffer to a planar RGB float32 tensor normalized to [0, 1].
    /// Output layout: [1, 3, height, width] as a flat float array (CHW format).
    /// </summary>
    public static float[] BgraToRgbFloat(ReadOnlySpan<byte> bgra, int width, int height)
    {
        int pixelCount = width * height;
        var tensor = new float[3 * pixelCount];

        int rOffset = 0;
        int gOffset = pixelCount;
        int bOffset = 2 * pixelCount;

        for (int i = 0; i < pixelCount; i++)
        {
            int srcIdx = i * 4;
            tensor[bOffset + i] = bgra[srcIdx] / 255f;      // B
            tensor[gOffset + i] = bgra[srcIdx + 1] / 255f;   // G
            tensor[rOffset + i] = bgra[srcIdx + 2] / 255f;   // R
        }

        return tensor;
    }

    /// <summary>
    /// Converts an RGB8 pixel buffer to a planar RGB float32 tensor normalized to [0, 1].
    /// Output layout: [1, 3, height, width] as a flat float array (CHW format).
    /// </summary>
    public static float[] RgbToFloat(ReadOnlySpan<byte> rgb, int width, int height)
    {
        return RgbToFloat(rgb, width, height, null);
    }

    /// <summary>
    /// Buffer-reusing variant: writes into an existing float array to avoid allocation.
    /// </summary>
    public static float[] RgbToFloat(ReadOnlySpan<byte> rgb, int width, int height, float[]? buffer)
    {
        int pixelCount = width * height;
        int totalLen = 3 * pixelCount;
        var tensor = (buffer is not null && buffer.Length >= totalLen) ? buffer : new float[totalLen];

        int rOffset = 0;
        int gOffset = pixelCount;
        int bOffset = 2 * pixelCount;

        // Use reciprocal multiply instead of divide (faster in tight loop)
        const float inv255 = 1f / 255f;

        for (int i = 0; i < pixelCount; i++)
        {
            int srcIdx = i * 3;
            tensor[rOffset + i] = rgb[srcIdx] * inv255;
            tensor[gOffset + i] = rgb[srcIdx + 1] * inv255;
            tensor[bOffset + i] = rgb[srcIdx + 2] * inv255;
        }

        return tensor;
    }

    /// <summary>
    /// Bilinear resize of a planar float32 RGB tensor from (srcW, srcH) to (dstW, dstH).
    /// Input/output layout: [3, H, W] flat array.
    /// </summary>
    public static float[] ResizePlanar(float[] src, int srcW, int srcH, int dstW, int dstH)
    {
        int srcPixels = srcW * srcH;
        int dstPixels = dstW * dstH;
        var dst = new float[3 * dstPixels];

        float xScale = (float)srcW / dstW;
        float yScale = (float)srcH / dstH;

        for (int c = 0; c < 3; c++)
        {
            int srcOff = c * srcPixels;
            int dstOff = c * dstPixels;

            for (int dy = 0; dy < dstH; dy++)
            {
                float srcY = (dy + 0.5f) * yScale - 0.5f;
                int y0 = Math.Max(0, (int)MathF.Floor(srcY));
                int y1 = Math.Min(srcH - 1, y0 + 1);
                float fy = srcY - y0;

                for (int dx = 0; dx < dstW; dx++)
                {
                    float srcX = (dx + 0.5f) * xScale - 0.5f;
                    int x0 = Math.Max(0, (int)MathF.Floor(srcX));
                    int x1 = Math.Min(srcW - 1, x0 + 1);
                    float fx = srcX - x0;

                    float v00 = src[srcOff + y0 * srcW + x0];
                    float v10 = src[srcOff + y0 * srcW + x1];
                    float v01 = src[srcOff + y1 * srcW + x0];
                    float v11 = src[srcOff + y1 * srcW + x1];

                    float val = v00 * (1 - fx) * (1 - fy)
                              + v10 * fx * (1 - fy)
                              + v01 * (1 - fx) * fy
                              + v11 * fx * fy;

                    dst[dstOff + dy * dstW + dx] = val;
                }
            }
        }

        return dst;
    }

    /// <summary>
    /// Converts a planar CHW tensor to interleaved HWC format for models that expect it.
    /// Input: [3, H, W], Output: [H, W, 3].
    /// </summary>
    public static float[] PlanarToInterleaved(float[] planar, int width, int height)
    {
        int pixelCount = width * height;
        var hwc = new float[3 * pixelCount];

        for (int i = 0; i < pixelCount; i++)
        {
            hwc[i * 3] = planar[i];                    // R
            hwc[i * 3 + 1] = planar[pixelCount + i];   // G
            hwc[i * 3 + 2] = planar[2 * pixelCount + i]; // B
        }

        return hwc;
    }

    /// <summary>
    /// Crops and warps a region from a planar RGB float tensor using an affine transformation.
    /// Used to extract the palm region for the landmark model.
    /// The affine matrix maps destination coords to source coords (inverse warp).
    /// </summary>
    /// <param name="src">Source planar tensor [3, srcH, srcW].</param>
    /// <param name="srcW">Source width.</param>
    /// <param name="srcH">Source height.</param>
    /// <param name="dstW">Destination crop width.</param>
    /// <param name="dstH">Destination crop height.</param>
    /// <param name="inverseAffine">3x2 inverse affine matrix [a, b, tx, c, d, ty] mapping dst-to-src.</param>
    /// <returns>Warped planar tensor [3, dstH, dstW].</returns>
    public static float[] AffineCrop(float[] src, int srcW, int srcH, int dstW, int dstH, float[] inverseAffine)
    {
        int srcPixels = srcW * srcH;
        int dstPixels = dstW * dstH;
        var dst = new float[3 * dstPixels];

        float a = inverseAffine[0], b = inverseAffine[1], tx = inverseAffine[2];
        float c = inverseAffine[3], d = inverseAffine[4], ty = inverseAffine[5];

        for (int dy = 0; dy < dstH; dy++)
        {
            for (int dx = 0; dx < dstW; dx++)
            {
                float srcX = a * dx + b * dy + tx;
                float srcY = c * dx + d * dy + ty;

                int x0 = (int)MathF.Floor(srcX);
                int y0 = (int)MathF.Floor(srcY);
                int x1 = x0 + 1;
                int y1 = y0 + 1;
                float fx = srcX - x0;
                float fy = srcY - y0;

                x0 = Math.Clamp(x0, 0, srcW - 1);
                x1 = Math.Clamp(x1, 0, srcW - 1);
                y0 = Math.Clamp(y0, 0, srcH - 1);
                y1 = Math.Clamp(y1, 0, srcH - 1);

                int dstIdx = dy * dstW + dx;

                for (int ch = 0; ch < 3; ch++)
                {
                    int co = ch * srcPixels;
                    float v00 = src[co + y0 * srcW + x0];
                    float v10 = src[co + y0 * srcW + x1];
                    float v01 = src[co + y1 * srcW + x0];
                    float v11 = src[co + y1 * srcW + x1];

                    dst[ch * dstPixels + dstIdx] =
                        v00 * (1 - fx) * (1 - fy)
                      + v10 * fx * (1 - fy)
                      + v01 * (1 - fx) * fy
                      + v11 * fx * fy;
                }
            }
        }

        return dst;
    }

    /// <summary>
    /// Computes a 2D affine warp matrix (and its inverse) to extract a rotated rectangle
    /// from the source image. The rectangle is defined by center, size, and rotation angle.
    /// Returns (forward 3x2, inverse 3x2) as flat float[6] arrays.
    /// Forward maps source to destination, inverse maps destination to source.
    /// </summary>
    public static (float[] Forward, float[] Inverse) ComputeRotatedRectWarp(
        float centerX, float centerY, float width, float height, float rotationRadians, int dstW, int dstH)
    {
        float cos = MathF.Cos(rotationRadians);
        float sin = MathF.Sin(rotationRadians);

        float scaleX = width / dstW;
        float scaleY = height / dstH;

        // Inverse affine: maps destination pixel to source pixel
        // dst_centered = dst - (dstW/2, dstH/2)
        // src = R * scale * dst_centered + center
        float halfDstW = dstW / 2f;
        float halfDstH = dstH / 2f;

        // Combined: src = [cos*sx, -sin*sy, cx - cos*sx*hw + sin*sy*hh]
        //                  [sin*sx,  cos*sy, cy - sin*sx*hw - cos*sy*hh]
        float a = cos * scaleX;
        float b = -sin * scaleY;
        float txInv = centerX - a * halfDstW - b * halfDstH;
        float c = sin * scaleX;
        float d = cos * scaleY;
        float tyInv = centerY - c * halfDstW - d * halfDstH;

        var inverse = new float[] { a, b, txInv, c, d, tyInv };

        // Forward affine: maps source pixel to destination pixel (inverse of the inverse)
        float det = a * d - b * c;
        if (MathF.Abs(det) < 1e-10f)
            det = 1e-10f;

        float invDet = 1f / det;
        float fA = d * invDet;
        float fB = -b * invDet;
        float fC = -c * invDet;
        float fD = a * invDet;
        float fTx = -(fA * txInv + fB * tyInv);
        float fTy = -(fC * txInv + fD * tyInv);

        var forward = new float[] { fA, fB, fTx, fC, fD, fTy };

        return (forward, inverse);
    }
}

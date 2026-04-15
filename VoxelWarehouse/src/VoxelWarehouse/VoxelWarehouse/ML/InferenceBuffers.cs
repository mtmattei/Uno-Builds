using System.Buffers;

namespace VoxelWarehouse.ML;

/// <summary>
/// Pre-allocated buffer pool for the ONNX inference pipeline.
/// Eliminates per-frame allocations for the hot path (camera → inference → result).
/// </summary>
public sealed class InferenceBuffers : IDisposable
{
    private const int PalmSize = 192;
    private const int LandmarkSize = 224;
    private const int CameraW = 640;
    private const int CameraH = 480;

    // Frame conversion buffer (RGB bytes → planar float)
    public float[] FrameFloat { get; } = new float[3 * CameraW * CameraH];

    // Palm detection buffers
    public float[] PalmResized { get; } = new float[3 * PalmSize * PalmSize];
    public float[] PalmHWC { get; } = new float[PalmSize * PalmSize * 3];

    // Landmark detection buffers
    public float[] LandmarkCropped { get; } = new float[3 * LandmarkSize * LandmarkSize];
    public float[] LandmarkHWC { get; } = new float[LandmarkSize * LandmarkSize * 3];

    // Reusable landmark array
    public Landmark3D[] Landmarks { get; } = new Landmark3D[21];

    public void Dispose() { /* All managed arrays, no unmanaged resources */ }
}

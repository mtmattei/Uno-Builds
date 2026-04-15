using System;

namespace VoxelWarehouse.Services;

/// <summary>
/// Provides camera frame capture for the hand tracking pipeline.
/// </summary>
public interface ICameraService : IDisposable
{
    /// <summary>Width of captured frames in pixels.</summary>
    int FrameWidth { get; }

    /// <summary>Height of captured frames in pixels.</summary>
    int FrameHeight { get; }

    /// <summary>Whether the camera is currently capturing.</summary>
    bool IsCapturing { get; }

    /// <summary>Starts capturing frames from the camera.</summary>
    void StartCapture();

    /// <summary>Stops capturing frames.</summary>
    void StopCapture();

    /// <summary>
    /// Gets the latest frame as an RGB byte array (3 bytes per pixel, row-major).
    /// Returns null if no frame is available yet.
    /// </summary>
    byte[]? GetLatestFrame();
}

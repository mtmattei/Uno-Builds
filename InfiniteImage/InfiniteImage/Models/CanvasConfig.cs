namespace InfiniteImage.Models;

/// <summary>
/// Configuration constants for the Infinite 3D Canvas.
/// </summary>
public static class CanvasConfig
{
    // Chunk System
    public const float ChunkSize = 600f;
    public const int RenderRadiusXY = 1;
    public const int RenderRadiusZ = 1;  // Reduced from 2 to 1 for better FPS
    public const int PlanesPerChunk = 3;  // Reduced from 4 to 3
    public const int MaxPhotosPerChunk = 4;  // Guard rail: max photos from library per chunk (reduced from 8)
    public const int MaxVisiblePlanes = 60;  // Hard limit on total visible planes to maintain FPS
    public const int MaxCacheSize = 150;

    // Camera
    public const float Fov = 60f;
    public const float Near = 10f;
    public const float Far = 3000f;

    // Movement Physics
    public const float VelocityLerp = 0.08f;
    public const float VelocityDecay = 0.94f;
    public const float PanSensitivity = 0.72f;
    public const float ZoomSensitivity = 2.25f;
    public const float KeyboardSpeedXY = 10.8f;
    public const float KeyboardSpeedZ = 18f;

    // Visuals
    public const float PlaneMinSize = 120f;
    public const float PlaneMaxSize = 220f;
    public const float DepthFadeStart = 400f;
    public const float DepthFadeEnd = 1200f;
    public const float NearFadeDistance = 100f;
    public const float OpacityThreshold = 0.02f;

    // Performance
    public const int TargetFPS = 60;
    public const double TargetFrameTimeMs = 1000.0 / TargetFPS; // 16.67ms
    public const int TelemetryUpdateIntervalMs = 1000;

    // Image Loading & Memory
    public const int MaxConcurrentImageLoads = 4;
    public const long MaxImageCacheMemoryMB = 50;  // Reduced from 100 to 50
    public const long MaxImageCacheMemoryBytes = MaxImageCacheMemoryMB * 1024 * 1024;
    public const int EstimatedBytesPerPixel = 4; // RGBA

    // Calculated
    public static int TotalActiveChunks =>
        (2 * RenderRadiusXY + 1) * (2 * RenderRadiusXY + 1) * (2 * RenderRadiusZ + 1);

    public static int MaxPlanes => TotalActiveChunks * PlanesPerChunk;
}

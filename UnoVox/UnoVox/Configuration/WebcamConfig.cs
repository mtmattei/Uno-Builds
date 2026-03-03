namespace UnoVox.Configuration;

/// <summary>
/// Configuration for webcam capture and initialization.
/// </summary>
public class WebcamConfig
{
    /// <summary>Default camera capture width in pixels</summary>
    public int DefaultWidth { get; set; } = 640;

    /// <summary>Default camera capture height in pixels</summary>
    public int DefaultHeight { get; set; } = 480;

    /// <summary>Target frames per second for camera capture</summary>
    public int DefaultFps { get; set; } = 30;

    /// <summary>Timeout in milliseconds for frame read operations</summary>
    public int FrameReadTimeoutMs { get; set; } = 33;

    /// <summary>Maximum consecutive frame read failures before stopping capture</summary>
    public int MaxConsecutiveFailures { get; set; } = 30;

    /// <summary>Maximum camera index to probe during enumeration</summary>
    public int MaxCameraProbeIndex { get; set; } = 3;

    /// <summary>Sleep interval in capture thread (ms). ~30 FPS = 33ms</summary>
    public int CaptureThreadSleepMs { get; set; } = 33;

    /// <summary>
    /// Number of warm-up frames to read before testing camera validity.
    /// Many cameras need time to adjust exposure and focus.
    /// </summary>
    public int InitializationWarmupFrames { get; set; } = 3;

    /// <summary>Timeout per camera when enumerating available cameras (ms)</summary>
    public int CameraProbeTimeoutMs { get; set; } = 500;
}

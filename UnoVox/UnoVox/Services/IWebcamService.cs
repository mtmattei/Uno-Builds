using SkiaSharp;

namespace UnoVox.Services;

public interface IWebcamService : IDisposable
{
    bool IsInitialized { get; }
    bool IsCapturing { get; }
    event EventHandler<SKBitmap>? FrameCaptured;
    
    void SetCameraIndex(int index);
    Task<bool> InitializeAsync();
    Task<bool> StartCaptureAsync();
    Task StopCaptureAsync();
}

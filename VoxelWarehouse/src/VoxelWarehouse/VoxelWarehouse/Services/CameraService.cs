using System;
using System.Threading;
using OpenCvSharp;

namespace VoxelWarehouse.Services;

/// <summary>
/// Real camera service using OpenCvSharp4.
/// Captures frames from the default webcam at 640×480 and converts to RGB byte array.
/// </summary>
public sealed class CameraService : ICameraService
{
    private VideoCapture? _capture;
    private Mat? _frameMat;
    private Mat? _rgbMat;
    private byte[]? _currentFrame;
    private byte[]? _frameBuffer;
    private Thread? _captureThread;
    private volatile bool _running;
    private bool _disposed;

    public int FrameWidth { get; private set; } = 640;
    public int FrameHeight { get; private set; } = 480;
    public bool IsCapturing => _running;

    public void StartCapture()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(CameraService));
        if (_running) return;

        _capture = new VideoCapture(0);
        if (!_capture.IsOpened())
        {
            _capture.Dispose();
            _capture = null;
            System.Diagnostics.Debug.WriteLine("[CameraService] No camera found, using fallback");
            StartFallbackCapture();
            return;
        }

        _capture.Set(VideoCaptureProperties.FrameWidth, 640);
        _capture.Set(VideoCaptureProperties.FrameHeight, 480);

        FrameWidth = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
        FrameHeight = (int)_capture.Get(VideoCaptureProperties.FrameHeight);

        _frameMat = new Mat();
        _rgbMat = new Mat();
        _running = true;

        _captureThread = new Thread(CaptureLoop)
        {
            Name = "CameraCapture",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        _captureThread.Start();
    }

    public void StopCapture()
    {
        _running = false;
        _captureThread?.Join(TimeSpan.FromSeconds(2));
        _captureThread = null;
        _fallbackTimer?.Dispose();
        _fallbackTimer = null;

        _capture?.Dispose();
        _capture = null;
        _frameMat?.Dispose();
        _frameMat = null;
        _rgbMat?.Dispose();
        _rgbMat = null;
    }

    public byte[]? GetLatestFrame() => _currentFrame;

    private void CaptureLoop()
    {
        while (_running && _capture is not null)
        {
            try
            {
                if (!_capture.Read(_frameMat!) || _frameMat!.Empty())
                {
                    Thread.Sleep(5);
                    continue;
                }

                // Convert BGR → RGB
                Cv2.CvtColor(_frameMat, _rgbMat!, ColorConversionCodes.BGR2RGB);

                // Extract raw bytes — reuse buffer to reduce GC pressure
                int size = FrameWidth * FrameHeight * 3;
                _frameBuffer ??= new byte[size];
                if (_frameBuffer.Length != size) _frameBuffer = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(_rgbMat!.Data, _frameBuffer, 0, size);
                _currentFrame = _frameBuffer;

                // Target ~30fps
                Thread.Sleep(33);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CameraService] Frame capture error: {ex.Message}");
                Thread.Sleep(100);
            }
        }
    }

    #region Fallback (no camera available)

    private Timer? _fallbackTimer;
    private long _fallbackFrameCount;

    private void StartFallbackCapture()
    {
        FrameWidth = 640;
        FrameHeight = 480;
        _running = true;

        _fallbackTimer = new Timer(_ =>
        {
            _fallbackFrameCount++;
            var frame = new byte[FrameWidth * FrameHeight * 3];

            // Generate a simple test pattern (moving gradient)
            float time = _fallbackFrameCount * 0.033f;
            for (int y = 0; y < FrameHeight; y++)
            {
                for (int x = 0; x < FrameWidth; x++)
                {
                    int idx = (y * FrameWidth + x) * 3;
                    byte val = (byte)((x + y + (int)(time * 60)) % 64 + 20);
                    frame[idx] = val;
                    frame[idx + 1] = val;
                    frame[idx + 2] = val;
                }
            }

            _currentFrame = frame;
        }, null, 0, 33);
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopCapture();
    }
}

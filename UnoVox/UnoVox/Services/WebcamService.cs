using OpenCvSharp;
using SkiaSharp;
using Microsoft.Extensions.Options;
using UnoVox.Configuration;

namespace UnoVox.Services;

public class WebcamService : IWebcamService
{
    private VideoCapture? _capture;
    private Thread? _captureThread;
    private bool _isCapturing;
    private readonly object _lock = new();
    private int _selectedCameraIndex = 0;
    private readonly WebcamConfig _config;

    public event EventHandler<SKBitmap>? FrameCaptured;
    public bool IsInitialized { get; private set; }
    public bool IsCapturing => _isCapturing;

    public WebcamService(IOptions<WebcamConfig> config)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Gets the list of available camera devices (async to avoid UI blocking)
    /// </summary>
    public static async Task<List<(int index, string name)>> GetAvailableCamerasAsync()
    {
        var cameras = new List<(int index, string name)>();

        // Enumerate on background thread to avoid UI stalls
        await Task.Run(() =>
        {
            System.Diagnostics.Debug.WriteLine("[WebcamService] Starting camera enumeration...");

            // Only probe configured max indices - faster enumeration
            // Most systems have 0-2 cameras at most
            const int maxProbe = 3; // TODO: Make this configurable

            for (int i = 0; i < maxProbe; i++)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[WebcamService] Probing camera index {i}...");

                    // Check cache first
                    var cachedApi = CameraApiCache.GetSuccessfulApi(i);
                    if (cachedApi.HasValue)
                    {
                        using var capCached = new VideoCapture(i, cachedApi.Value);
                        if (capCached.IsOpened())
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebcamService] Found camera {i} via cached API {cachedApi.Value}");
                            cameras.Add((i, $"Camera {i} ({cachedApi.Value})"));
                            continue;
                        }
                        // Cache invalid, clear it
                        CameraApiCache.Clear();
                    }

                    // Try DirectShow first on Windows - more reliable for webcams
                    using (var capDshow = new VideoCapture(i, VideoCaptureAPIs.DSHOW))
                    {
                        if (capDshow.IsOpened())
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebcamService] Found camera {i} via DSHOW");
                            CameraApiCache.SetSuccessfulApi(i, VideoCaptureAPIs.DSHOW);
                            cameras.Add((i, $"Camera {i} (DirectShow)"));
                            continue;
                        }
                    }

                    // Fallback to MSMF (Media Foundation)
                    using (var capMsmf = new VideoCapture(i, VideoCaptureAPIs.MSMF))
                    {
                        if (capMsmf.IsOpened())
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebcamService] Found camera {i} via MSMF");
                            CameraApiCache.SetSuccessfulApi(i, VideoCaptureAPIs.MSMF);
                            cameras.Add((i, $"Camera {i} (MediaFoundation)"));
                            continue;
                        }
                    }

                    // Last fallback to default API
                    using (var capAny = new VideoCapture(i))
                    {
                        if (capAny.IsOpened())
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebcamService] Found camera {i} via default API");
                            CameraApiCache.SetSuccessfulApi(i, VideoCaptureAPIs.ANY);
                            cameras.Add((i, $"Camera {i}"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebcamService] Camera probe {i} exception: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[WebcamService] Enumeration complete. Found {cameras.Count} cameras.");
        });

        return cameras;
    }

    /// <summary>
    /// Sets the camera index to use (call before InitializeAsync)
    /// </summary>
    public void SetCameraIndex(int cameraIndex)
    {
        _selectedCameraIndex = cameraIndex;
    }

    public Task<bool> InitializeAsync()
    {
        try
        {
            Console.WriteLine($"[WebcamService] Initializing camera index {_selectedCameraIndex}...");

            // Try cached API first for faster initialization
            var cachedApi = CameraApiCache.GetSuccessfulApi(_selectedCameraIndex);
            if (cachedApi.HasValue)
            {
                Console.WriteLine($"[WebcamService] Trying cached API {cachedApi.Value}...");
                _capture = new VideoCapture(_selectedCameraIndex, cachedApi.Value);

                if (_capture.IsOpened())
                {
                    Console.WriteLine($"[WebcamService] Camera opened with cached API {cachedApi.Value}");
                    goto CameraOpened; // Skip fallback attempts
                }

                // Cache invalid, clear and try fallbacks
                _capture.Dispose();
                CameraApiCache.Clear();
            }

            // Try DirectShow first on Windows - more reliable for webcams
            Console.WriteLine($"[WebcamService] Trying DirectShow...");
            _capture = new VideoCapture(_selectedCameraIndex, VideoCaptureAPIs.DSHOW);

            if (!_capture.IsOpened())
            {
                Console.WriteLine($"[WebcamService] DirectShow failed, trying MSMF...");
                _capture.Dispose();
                _capture = new VideoCapture(_selectedCameraIndex, VideoCaptureAPIs.MSMF);
            }

            if (!_capture.IsOpened())
            {
                Console.WriteLine($"[WebcamService] MSMF failed, trying default API...");
                _capture.Dispose();
                _capture = new VideoCapture(_selectedCameraIndex);
            }

            CameraOpened:

            if (!_capture.IsOpened())
            {
                Console.WriteLine($"[WebcamService] All APIs failed for camera {_selectedCameraIndex}");
                _capture.Dispose();
                _capture = null;
                IsInitialized = false;
                return Task.FromResult(false);
            }

            Console.WriteLine($"[WebcamService] Camera opened successfully");

            // Try to set camera properties from configuration
            // Some cameras don't support property changes and will crash if we try
            try
            {
                _capture.Set(VideoCaptureProperties.FrameWidth, _config.DefaultWidth);
                _capture.Set(VideoCaptureProperties.FrameHeight, _config.DefaultHeight);
                _capture.Set(VideoCaptureProperties.Fps, _config.DefaultFps);
                Console.WriteLine($"[WebcamService] Camera properties set successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebcamService] Warning: Could not set camera properties (camera may use defaults): {ex.Message}");
            }

            // Verify actual resolution (some cameras ignore requested settings)
            var actualWidth = _capture.Get(VideoCaptureProperties.FrameWidth);
            var actualHeight = _capture.Get(VideoCaptureProperties.FrameHeight);
            var actualFps = _capture.Get(VideoCaptureProperties.Fps);
            Console.WriteLine($"[WebcamService] Camera reported: {actualWidth}x{actualHeight} @ {actualFps} FPS");

            // Test frame capture with warm-up period
            // Many cameras need several frames to adjust exposure and focus
            Console.WriteLine($"[WebcamService] Testing frame capture with {_config.InitializationWarmupFrames} warm-up frames...");

            bool captureWorking = false;
            for (int warmupFrame = 0; warmupFrame < _config.InitializationWarmupFrames; warmupFrame++)
            {
                using var testFrame = new Mat();
                if (_capture.Read(testFrame) && !testFrame.Empty())
                {
                    Console.WriteLine($"[WebcamService] Warm-up frame {warmupFrame + 1}/{_config.InitializationWarmupFrames}: {testFrame.Width}x{testFrame.Height}");
                    captureWorking = true;
                }
                else
                {
                    Console.WriteLine($"[WebcamService] Warm-up frame {warmupFrame + 1} failed to read");
                    // Don't fail immediately - some cameras have intermittent issues during warm-up
                }

                // Small delay to allow camera to stabilize
                if (warmupFrame < _config.InitializationWarmupFrames - 1)
                {
                    Thread.Sleep(100);
                }
            }

            if (!captureWorking)
            {
                Console.WriteLine($"[WebcamService] Camera opened but cannot read frames after warm-up - capture test failed");
                _capture.Dispose();
                _capture = null;
                IsInitialized = false;
                return Task.FromResult(false);
            }

            Console.WriteLine($"[WebcamService] Camera verified successfully");

            IsInitialized = true;
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebcamService] InitializeAsync failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[WebcamService] Stack: {ex.StackTrace}");
            IsInitialized = false;
            return Task.FromResult(false);
        }
    }

    public Task<bool> StartCaptureAsync()
    {
        if (!IsInitialized || _capture == null)
            return Task.FromResult(false);

        try
        {
            _isCapturing = true;
            _captureThread = new Thread(CaptureLoop)
            {
                IsBackground = true,
                Name = "WebcamCaptureThread"
            };
            _captureThread.Start();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebcamService: StartCaptureAsync failed: {ex.Message}");
            _isCapturing = false;
            return Task.FromResult(false);
        }
    }

    public Task StopCaptureAsync()
    {
        _isCapturing = false;
        _captureThread?.Join(1000); // Wait up to 1 second for thread to finish
        return Task.CompletedTask;
    }

    private void CaptureLoop()
    {
        using var frame = new Mat();
        int frameCount = 0;
        int consecutiveFailures = 0;
        int maxFailures = _config.MaxConsecutiveFailures; // From configuration

        Console.WriteLine("[WebcamService] Capture loop started");

        while (_isCapturing && _capture != null)
        {
            try
            {
                bool readSuccess;
                lock (_lock)
                {
                    readSuccess = _capture.Read(frame);
                }

                if (!readSuccess || frame.Empty())
                {
                    consecutiveFailures++;
                    if (consecutiveFailures == 1 || consecutiveFailures % 10 == 0)
                    {
                        Console.WriteLine($"[WebcamService] Frame read failed (attempt {consecutiveFailures})");
                    }

                    if (consecutiveFailures >= maxFailures)
                    {
                        Console.WriteLine($"[WebcamService] Too many failures ({maxFailures}), stopping capture");
                        _isCapturing = false;
                        break;
                    }

                    Thread.Sleep(_config.CaptureThreadSleepMs);
                    continue;
                }

                // Reset failure counter on success
                consecutiveFailures = 0;
                frameCount++;

                if (frameCount == 1 || frameCount % 30 == 0)
                {
                    Console.WriteLine($"[WebcamService] Frame {frameCount}: {frame.Width}x{frame.Height}");
                }

                var bitmap = ConvertMatToSkBitmap(frame);
                if (bitmap != null)
                {
                    if (FrameCaptured != null)
                    {
                        FrameCaptured.Invoke(this, bitmap);
                    }
                    else if (frameCount == 1)
                    {
                        Console.WriteLine("[WebcamService] WARNING: No FrameCaptured subscribers!");
                    }
                }
                else
                {
                    Console.WriteLine("[WebcamService] Bitmap conversion returned null");
                }

                // Limit to ~30 FPS
                Thread.Sleep(33);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebcamService] Capture loop error: {ex.Message}");
            }
        }

        Console.WriteLine($"[WebcamService] Capture loop ended after {frameCount} frames");
    }

    private SKBitmap? ConvertMatToSkBitmap(Mat mat)
    {
        try
        {
            if (mat.Empty() || mat.Width == 0 || mat.Height == 0)
            {
                System.Diagnostics.Debug.WriteLine("WebcamService: Mat is empty or has zero dimensions");
                return null;
            }

            var width = mat.Width;
            var height = mat.Height;


            // Create bitmap with BGRA color type (native for most cameras)
            var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            
            // Convert BGR to BGRA and copy directly
            using var bgra = new Mat();
            Cv2.CvtColor(mat, bgra, ColorConversionCodes.BGR2BGRA);
            
            // Copy using row-by-row to ensure proper stride alignment
            unsafe
            {
                var dstPtr = (byte*)skBitmap.GetPixels();
                var srcPtr = bgra.Data;
                var srcStride = (int)bgra.Step(); // OpenCV row stride
                var dstStride = skBitmap.RowBytes;   // SkiaSharp row stride
                
                for (int y = 0; y < height; y++)
                {
                    var srcRow = srcPtr + (y * srcStride);
                    var dstRow = dstPtr + (y * dstStride);
                    Buffer.MemoryCopy(srcRow.ToPointer(), dstRow, dstStride, width * 4);
                }
            }

            return skBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebcamService: Conversion error: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _isCapturing = false;
        _captureThread?.Join(1000);
        
        lock (_lock)
        {
            _capture?.Dispose();
            _capture = null;
        }
        
        IsInitialized = false;
    }
}

using System;
using System.Diagnostics;
using System.Threading;
using VoxelWarehouse.Models;
using VoxelWarehouse.Services;

namespace VoxelWarehouse.ML;

/// <summary>
/// Background frame processing loop with temporal smoothing and adaptive frame pacing.
/// </summary>
public sealed class HandTrackingLoop : IDisposable
{
    private const long TargetFrameTimeMs = 33; // ~30fps
    private const int NoHandSkipFrames = 5;     // After N frames with no hand, slow down detection

    private readonly OnnxHandTracker _tracker;
    private readonly ICameraService _camera;
    private readonly GestureSmoothing _smoother = new();
    private Thread? _workerThread;
    private volatile bool _running;
    private bool _disposed;
    private int _noHandFrames;

    public event Action<HandTrackingResult>? ResultReady;

    public double LastInferenceMs { get; private set; }
    public long FramesProcessed { get; private set; }
    public long FramesSkipped { get; private set; }

    public HandTrackingLoop(OnnxHandTracker tracker, ICameraService camera)
    {
        _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    public void Start()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(HandTrackingLoop));
        if (_running) return;

        _running = true;
        FramesProcessed = 0;
        FramesSkipped = 0;
        _noHandFrames = 0;

        if (!_camera.IsCapturing)
            _camera.StartCapture();

        _workerThread = new Thread(ProcessingLoop)
        {
            Name = "HandTracking",
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        _workerThread.Start();
    }

    public void Stop()
    {
        _running = false;
        _workerThread?.Join(TimeSpan.FromSeconds(2));
        _workerThread = null;
    }

    private void ProcessingLoop()
    {
        var sw = new Stopwatch();

        // Warmup: first ONNX inference is slow (JIT compile). Run a dummy frame.
        WarmupInference();

        while (_running)
        {
            sw.Restart();

            try
            {
                var frame = _camera.GetLatestFrame();
                if (frame is null)
                {
                    Thread.Sleep(5);
                    continue;
                }

                // Adaptive pacing: if no hand detected for a while, slow down
                if (_noHandFrames > NoHandSkipFrames)
                {
                    // Only process every other frame when no hand is visible
                    FramesSkipped++;
                    _noHandFrames++;
                    if (_noHandFrames % 2 != 0)
                    {
                        Thread.Sleep((int)TargetFrameTimeMs);
                        continue;
                    }
                }

                var rawResult = _tracker.ProcessFrame(frame, _camera.FrameWidth, _camera.FrameHeight);

                // Track hand presence for adaptive pacing
                if (rawResult.HandDetected)
                    _noHandFrames = 0;
                else
                    _noHandFrames++;

                // Apply temporal smoothing
                var smoothed = _smoother.Smooth(rawResult);

                sw.Stop();
                LastInferenceMs = sw.Elapsed.TotalMilliseconds;
                FramesProcessed++;

                ResultReady?.Invoke(smoothed);

                long elapsed = sw.ElapsedMilliseconds;
                if (elapsed < TargetFrameTimeMs)
                    Thread.Sleep((int)(TargetFrameTimeMs - elapsed));
                else
                    FramesSkipped++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HandTrackingLoop] Error: {ex.Message}");
                Thread.Sleep(100);
            }
        }
    }

    private void WarmupInference()
    {
        try
        {
            // Run a dummy frame to trigger ONNX session JIT compilation
            var dummyFrame = new byte[_camera.FrameWidth * _camera.FrameHeight * 3];
            _tracker.ProcessFrame(dummyFrame, _camera.FrameWidth, _camera.FrameHeight);
            Debug.WriteLine("[HandTrackingLoop] Warmup complete");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandTrackingLoop] Warmup failed (non-fatal): {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}

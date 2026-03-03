using SkiaSharp;

namespace UnoVox.Services;

/// <summary>
/// Thread-safe buffer for webcam frames with proper disposal and async synchronization.
/// Replaces lock-based frame swapping to prevent race conditions.
/// </summary>
public class ThreadSafeFrameBuffer : IDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SKBitmap? _currentFrame;
    private int _frameCount;
    private bool _disposed;

    /// <summary>
    /// Gets the total number of frames that have been stored in this buffer.
    /// </summary>
    public int FrameCount => _frameCount;

    /// <summary>
    /// Gets a copy of the current frame asynchronously.
    /// Returns null if no frame is available.
    /// </summary>
    /// <returns>A copy of the current frame, or null if no frame is stored</returns>
    public async Task<SKBitmap?> GetCurrentFrameAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFrameBuffer));

        await _lock.WaitAsync();
        try
        {
            return _currentFrame?.Copy();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets a copy of the current frame synchronously.
    /// Returns null if no frame is available or if acquisition times out.
    /// Use this only when async is not possible (e.g., paint handlers).
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait for lock in milliseconds</param>
    /// <returns>A copy of the current frame, or null</returns>
    public SKBitmap? GetCurrentFrameSync(int timeoutMs = 100)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFrameBuffer));

        if (!_lock.Wait(timeoutMs))
            return null;

        try
        {
            return _currentFrame?.Copy();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Updates the frame buffer with a new frame asynchronously.
    /// The previous frame is disposed automatically.
    /// </summary>
    /// <param name="newFrame">The new frame to store (will be owned by this buffer)</param>
    public async Task UpdateFrameAsync(SKBitmap newFrame)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFrameBuffer));

        if (newFrame == null)
            throw new ArgumentNullException(nameof(newFrame));

        await _lock.WaitAsync();
        try
        {
            _currentFrame?.Dispose();
            _currentFrame = newFrame;
            _frameCount++;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Executes an action with the current frame without copying.
    /// Use this for read-only operations to avoid allocation overhead.
    /// </summary>
    /// <param name="action">Action to execute with the frame</param>
    public async Task WithFrameAsync(Action<SKBitmap> action)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadSafeFrameBuffer));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        await _lock.WaitAsync();
        try
        {
            if (_currentFrame != null)
                action(_currentFrame);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clears the current frame and resets the frame count.
    /// </summary>
    public async Task ClearAsync()
    {
        if (_disposed)
            return;

        await _lock.WaitAsync();
        try
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
            _frameCount = 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Disposes the frame buffer and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _lock.Wait();
        try
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
            _disposed = true;
        }
        finally
        {
            _lock.Release();
            _lock.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

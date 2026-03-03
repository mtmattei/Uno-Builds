using SkiaSharp;
using System.Collections.Concurrent;

namespace UnoVox.Services;

/// <summary>
/// Thread-safe bitmap pool for webcam frame processing
/// Dynamically adjusts to frame size changes
/// </summary>
public class ThreadSafeSKBitmapPool : IDisposable
{
    private readonly ConcurrentDictionary<(int width, int height), SKBitmapPool> _pools = new();
    private readonly int _maxPoolSize;

    public ThreadSafeSKBitmapPool(int maxPoolSize = 8)
    {
        _maxPoolSize = maxPoolSize;
    }

    public SKBitmap Rent(int width, int height, SKColorType colorType = SKColorType.Bgra8888)
    {
        var pool = _pools.GetOrAdd((width, height), _ => new SKBitmapPool(width, height, colorType, _maxPoolSize));
        return pool.Rent();
    }

    public void Return(SKBitmap bitmap)
    {
        if (bitmap == null) return;

        var key = (bitmap.Width, bitmap.Height);
        if (_pools.TryGetValue(key, out var pool))
        {
            pool.Return(bitmap);
        }
        else
        {
            bitmap.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear();
        }
        _pools.Clear();
    }
}

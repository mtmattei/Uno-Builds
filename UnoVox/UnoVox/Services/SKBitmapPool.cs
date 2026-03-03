using SkiaSharp;

namespace UnoVox.Services;

/// <summary>
/// Object pool for SKBitmap frames to reduce allocation overhead in video processing
/// </summary>
public class SKBitmapPool
{
    private readonly ObjectPool<SKBitmap> _pool;
    private readonly int _width;
    private readonly int _height;
    private readonly SKColorType _colorType;

    public SKBitmapPool(int width, int height, SKColorType colorType = SKColorType.Bgra8888, int maxPoolSize = 8)
    {
        _width = width;
        _height = height;
        _colorType = colorType;
        _pool = new ObjectPool<SKBitmap>(
            objectFactory: () => new SKBitmap(width, height, colorType, SKAlphaType.Premul),
            resetAction: null, // Bitmaps don't need reset, they'll be overwritten
            maxPoolSize: maxPoolSize
        );
    }

    public SKBitmap Rent() => _pool.Rent();

    public void Return(SKBitmap bitmap)
    {
        if (bitmap != null &&
            bitmap.Width == _width && bitmap.Height == _height &&
            bitmap.ColorType == _colorType)
        {
            _pool.Return(bitmap);
        }
        else
        {
            bitmap?.Dispose();
        }
    }

    public void Clear() => _pool.Clear();
}

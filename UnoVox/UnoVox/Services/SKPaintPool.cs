using SkiaSharp;

namespace UnoVox.Services;

/// <summary>
/// Object pool for SKPaint objects to reduce allocation overhead in rendering
/// </summary>
public class SKPaintPool
{
    private readonly ObjectPool<SKPaint> _pool;

    public SKPaintPool(int maxPoolSize = 32)
    {
        _pool = new ObjectPool<SKPaint>(
            objectFactory: () => new SKPaint { IsAntialias = true },
            resetAction: paint =>
            {
                paint.Reset();
                paint.IsAntialias = true;
            },
            maxPoolSize: maxPoolSize
        );
    }

    public SKPaint Rent() => _pool.Rent();

    public void Return(SKPaint paint) => _pool.Return(paint);

    public void Clear() => _pool.Clear();
}

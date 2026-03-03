using SkiaSharp;

namespace UnoVox.Services;

/// <summary>
/// Object pool for SKPath objects to reduce allocation overhead in rendering
/// </summary>
public class SKPathPool
{
    private readonly ObjectPool<SKPath> _pool;

    public SKPathPool(int maxPoolSize = 32)
    {
        _pool = new ObjectPool<SKPath>(
            objectFactory: () => new SKPath(),
            resetAction: path => path.Reset(),
            maxPoolSize: maxPoolSize
        );
    }

    public SKPath Rent() => _pool.Rent();

    public void Return(SKPath path) => _pool.Return(path);

    public void Clear() => _pool.Clear();
}

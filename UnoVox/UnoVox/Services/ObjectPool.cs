using System.Collections.Concurrent;

namespace UnoVox.Services;

/// <summary>
/// Generic object pool for reusing expensive objects to reduce GC pressure
/// </summary>
public class ObjectPool<T> where T : class
{
    private readonly ConcurrentBag<T> _pool = new();
    private readonly Func<T> _objectFactory;
    private readonly Action<T>? _resetAction;
    private readonly int _maxPoolSize;

    public ObjectPool(Func<T> objectFactory, Action<T>? resetAction = null, int maxPoolSize = 32)
    {
        _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        _resetAction = resetAction;
        _maxPoolSize = maxPoolSize;
    }

    public T Rent()
    {
        if (_pool.TryTake(out var item))
        {
            return item;
        }

        return _objectFactory();
    }

    public void Return(T item)
    {
        if (item == null) return;

        if (_pool.Count < _maxPoolSize)
        {
            _resetAction?.Invoke(item);
            _pool.Add(item);
        }
        else if (item is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public void Clear()
    {
        while (_pool.TryTake(out var item))
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

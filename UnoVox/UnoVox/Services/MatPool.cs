using OpenCvSharp;

namespace UnoVox.Services;

/// <summary>
/// Object pool for OpenCV Mat objects to reduce allocation overhead
/// </summary>
public class MatPool
{
    private readonly ObjectPool<Mat> _pool;

    public MatPool(int maxPoolSize = 16)
    {
        _pool = new ObjectPool<Mat>(
            objectFactory: () => new Mat(),
            resetAction: mat => mat.Release(),
            maxPoolSize: maxPoolSize
        );
    }

    public Mat Rent() => _pool.Rent();

    public Mat Rent(Size size, MatType type)
    {
        var mat = Rent();
        mat.Create(size, type);
        return mat;
    }

    public void Return(Mat mat)
    {
        if (mat != null && !mat.IsDisposed)
        {
            _pool.Return(mat);
        }
    }

    public void Clear() => _pool.Clear();
}

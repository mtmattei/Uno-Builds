using OpenCvSharp;

namespace UnoVox.Services;

/// <summary>
/// Caches successful camera API preferences to avoid repeated probing
/// </summary>
public static class CameraApiCache
{
    private static readonly Dictionary<int, VideoCaptureAPIs> _successfulApis = new();
    private static readonly object _lock = new();

    public static void SetSuccessfulApi(int cameraIndex, VideoCaptureAPIs api)
    {
        lock (_lock)
        {
            _successfulApis[cameraIndex] = api;
        }
    }

    public static VideoCaptureAPIs? GetSuccessfulApi(int cameraIndex)
    {
        lock (_lock)
        {
            return _successfulApis.TryGetValue(cameraIndex, out var api) ? api : null;
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _successfulApis.Clear();
        }
    }
}

using VoxelWarehouse.Models;

namespace VoxelWarehouse.ML;

/// <summary>
/// Temporal smoothing for hand tracking results.
/// - Exponential moving average on landmark positions (reduces jitter)
/// - Gesture debouncing (requires N consecutive frames before switching)
/// - Cursor position smoothing (weighted blend of current + previous)
/// </summary>
public sealed class GestureSmoothing
{
    private const float LandmarkAlpha = 0.45f;  // EMA weight for new landmarks (lower = smoother)
    private const float CursorAlpha = 0.35f;    // EMA weight for cursor position
    private const int DebouncFrames = 3;         // Consecutive frames before gesture switch

    private Landmark3D[]? _smoothedLandmarks;
    private float _smoothedCursorX;
    private float _smoothedCursorY;
    private GestureType _currentGesture = GestureType.None;
    private GestureType _candidateGesture = GestureType.None;
    private int _candidateCount;
    private bool _initialized;

    /// <summary>
    /// Smooth the incoming hand tracking result and return a stabilized version.
    /// </summary>
    public HandTrackingResult Smooth(HandTrackingResult raw)
    {
        if (!raw.HandDetected)
        {
            _initialized = false;
            _currentGesture = GestureType.None;
            _candidateCount = 0;
            return raw;
        }

        // Smooth landmarks with EMA
        var smoothedLm = SmoothLandmarks(raw.Landmarks);

        // Smooth cursor position
        float cx, cy;
        if (!_initialized)
        {
            cx = raw.CursorX;
            cy = raw.CursorY;
            _smoothedCursorX = cx;
            _smoothedCursorY = cy;
            _initialized = true;
        }
        else
        {
            cx = _smoothedCursorX = _smoothedCursorX * (1f - CursorAlpha) + raw.CursorX * CursorAlpha;
            cy = _smoothedCursorY = _smoothedCursorY * (1f - CursorAlpha) + raw.CursorY * CursorAlpha;
        }

        // Debounce gesture — require consecutive agreement before switching
        var gesture = DebounceGesture(raw.Gesture);

        return new HandTrackingResult(
            raw.HandDetected,
            raw.Confidence,
            smoothedLm,
            gesture,
            cx, cy,
            raw.IsLeftHand);
    }

    private Landmark3D[] SmoothLandmarks(Landmark3D[] raw)
    {
        if (_smoothedLandmarks is null || _smoothedLandmarks.Length != raw.Length)
        {
            _smoothedLandmarks = new Landmark3D[raw.Length];
            Array.Copy(raw, _smoothedLandmarks, raw.Length);
            return _smoothedLandmarks;
        }

        for (int i = 0; i < raw.Length; i++)
        {
            var prev = _smoothedLandmarks[i];
            var curr = raw[i];
            _smoothedLandmarks[i] = new Landmark3D(
                prev.X * (1f - LandmarkAlpha) + curr.X * LandmarkAlpha,
                prev.Y * (1f - LandmarkAlpha) + curr.Y * LandmarkAlpha,
                prev.Z * (1f - LandmarkAlpha) + curr.Z * LandmarkAlpha);
        }

        return _smoothedLandmarks;
    }

    private GestureType DebounceGesture(GestureType raw)
    {
        if (raw == _currentGesture)
        {
            _candidateGesture = raw;
            _candidateCount = 0;
            return _currentGesture;
        }

        if (raw == _candidateGesture)
        {
            _candidateCount++;
            if (_candidateCount >= DebouncFrames)
            {
                _currentGesture = raw;
                _candidateCount = 0;
            }
        }
        else
        {
            _candidateGesture = raw;
            _candidateCount = 1;
        }

        return _currentGesture;
    }
}

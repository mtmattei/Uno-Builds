using System;
using System.Numerics;
using SkiaSharp;

namespace FibonacciSphere.Models;

/// <summary>
/// Represents a single point on the Fibonacci sphere.
/// </summary>
public class SpherePoint
{
    private const int MaxBufferSize = 64;
    private readonly Vector2[] _trailBuffer = new Vector2[MaxBufferSize];
    private int _trailHead;
    private int _trailCount;

    public int Index { get; init; }
    public Vector3 BasePosition { get; init; }
    public Vector3 CurrentPosition { get; set; }
    public float WobblePhase { get; init; }
    public Vector3 RandomWobbleDirection { get; init; }
    public float Size { get; set; }
    public SKColor Color { get; set; }
    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }
    public int MaxTrailLength { get; set; } = 20;
    public float Depth { get; set; }

    public int TrailCount => _trailCount;

    /// <summary>
    /// Adds a screen position to the trail history using O(1) circular buffer.
    /// </summary>
    public void AddToTrail(Vector2 screenPosition)
    {
        int maxLength = System.Math.Min(MaxTrailLength, MaxBufferSize);

        _trailBuffer[_trailHead] = screenPosition;
        _trailHead = (_trailHead + 1) % maxLength;

        if (_trailCount < maxLength)
        {
            _trailCount++;
        }
    }

    /// <summary>
    /// Gets trail position at index (0 = oldest, TrailCount-1 = newest).
    /// </summary>
    public Vector2 GetTrailPosition(int index)
    {
        int maxLength = System.Math.Min(MaxTrailLength, MaxBufferSize);
        int actualIndex = (_trailHead - _trailCount + index + maxLength) % maxLength;
        return _trailBuffer[actualIndex];
    }

    /// <summary>
    /// Copies trail data to a span for efficient iteration.
    /// </summary>
    public void CopyTrailTo(Span<Vector2> destination)
    {
        int maxLength = System.Math.Min(MaxTrailLength, MaxBufferSize);
        int start = (_trailHead - _trailCount + maxLength) % maxLength;

        for (int i = 0; i < _trailCount && i < destination.Length; i++)
        {
            destination[i] = _trailBuffer[(start + i) % maxLength];
        }
    }

    public void ClearTrail()
    {
        _trailHead = 0;
        _trailCount = 0;
    }
}

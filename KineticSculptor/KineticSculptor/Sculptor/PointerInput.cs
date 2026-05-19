using System.Diagnostics;

namespace KineticSculptor.Sculptor;

internal sealed class PointerInput
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    public float X { get; private set; } = -9999f;
    public float Y { get; private set; } = -9999f;
    public bool IsDown { get; private set; }
    public float DownX { get; private set; }
    public float DownY { get; private set; }
    public float MovedSinceDown { get; private set; }
    public float SmoothedSpeed { get; private set; }

    private long _downTimeMs;
    private long _lastMoveTimeMs;

    public long HeldMs => IsDown ? _sw.ElapsedMilliseconds - _downTimeMs : 0;

    /// <summary>
    /// Hooked up to PointerPressed. Always succeeds; never spawns disturbances on its own.
    /// </summary>
    public void OnDown(float x, float y)
    {
        IsDown = true;
        DownX = x;
        DownY = y;
        _downTimeMs = _sw.ElapsedMilliseconds;
        MovedSinceDown = 0f;
        X = x;
        Y = y;
    }

    /// <summary>
    /// Hooked up to PointerMoved. Returns a candidate Push impulse if this move
    /// should disturb the field; null when motion is too small for a hover wake.
    /// </summary>
    public PushImpulse? OnMove(float x, float y)
    {
        long now = _sw.ElapsedMilliseconds;
        PushImpulse? impulse = null;

        if (X > -9000f)
        {
            float dx = x - X;
            float dy = y - Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            float dt = MathF.Max(now - _lastMoveTimeMs, 1f);
            float speed = dist / dt * 1000f;
            SmoothedSpeed = SmoothedSpeed * 0.55f + speed * 0.45f;

            if (IsDown)
            {
                MovedSinceDown += dist;
                impulse = new PushImpulse(x, y, dx, dy, 1.0f, speed);
            }
            else if (dist > 1.5f)
            {
                // Hover wake — JS prototype scales dx/dy * 0.35 and speed * 0.5.
                impulse = new PushImpulse(x, y, dx * 0.35f, dy * 0.35f, 0.45f, speed * 0.5f);
            }
        }

        X = x;
        Y = y;
        _lastMoveTimeMs = now;
        return impulse;
    }

    /// <summary>
    /// Hooked up to PointerReleased. Returns true if the gesture qualifies as a tap
    /// (movement &lt; 7px, duration &lt; 260ms) — caller spawns a shockwave.
    /// </summary>
    public bool OnUp()
    {
        bool wasTap = false;
        if (IsDown)
        {
            long dur = _sw.ElapsedMilliseconds - _downTimeMs;
            wasTap = MovedSinceDown < 7f && dur < 260L;
        }
        IsDown = false;
        return wasTap;
    }

    /// <summary>
    /// Hooked up to PointerCanceled / PointerCaptureLost / PointerExited / window blur.
    /// </summary>
    public void Cancel()
    {
        IsDown = false;
        X = -9999f;
        Y = -9999f;
        SmoothedSpeed = 0f;
    }
}

internal readonly record struct PushImpulse(float X, float Y, float Dx, float Dy, float Life, float Speed);

using System;
using SkiaSharp;

namespace PrecisionDial.Controls;

internal struct Particle
{
    public float X, Y;
    public float Vx, Vy;
    public float Life;
    public float Decay;
    public float Size;
}

internal sealed class ParticleRenderer
{
    private readonly Particle[] _particles = new Particle[32];
    private int _activeCount;
    private readonly Random _rng = new();

    private readonly SKPaint _glowPaint = new()
    {
        IsAntialias = true,
    };

    private readonly SKPaint _corePaint = new()
    {
        IsAntialias = true,
    };

    // Cached 2dp blur — previously recreated on every particle draw call,
    // leaking a native mask filter 32× per frame.
    private readonly SKMaskFilter _glowBlur =
        SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2f);

    public void Emit(float x, float y, float velocityScale, SKColor accent)
    {
        var count = Math.Min(4, (int)(velocityScale * 4));
        for (int i = 0; i < count; i++)
        {
            if (_activeCount >= _particles.Length) break;

            var angle = (float)(_rng.NextDouble() * Math.PI * 2);
            var speed = (float)(1.0 + _rng.NextDouble() * 2.0) * velocityScale;

            _particles[_activeCount] = new Particle
            {
                X = x,
                Y = y,
                Vx = MathF.Cos(angle) * speed,
                Vy = MathF.Sin(angle) * speed - 1.5f, // upward bias
                Life = 1f,
                Decay = 0.02f + (float)_rng.NextDouble() * 0.03f,
                Size = 1.5f + (float)_rng.NextDouble() * 1.5f,
            };
            _activeCount++;
        }
    }

    public void Step()
    {
        int write = 0;
        for (int i = 0; i < _activeCount; i++)
        {
            ref var p = ref _particles[i];
            p.X += p.Vx;
            p.Y += p.Vy;
            p.Vy += 0.15f; // gravity
            p.Life -= p.Decay;
            p.Size *= 0.97f;

            if (p.Life > 0)
            {
                if (write != i) _particles[write] = p;
                write++;
            }
        }
        _activeCount = write;
    }

    public void Draw(SKCanvas canvas, SKColor accent)
    {
        if (_activeCount == 0) return;

        // Apply the cached blur filter once, outside the per-particle loop.
        _glowPaint.MaskFilter = _glowBlur;

        for (int i = 0; i < _activeCount; i++)
        {
            ref var p = ref _particles[i];
            var alpha = (byte)(p.Life * 60);

            // Outer glow
            _glowPaint.Color = accent.WithAlpha(alpha);
            canvas.DrawCircle(p.X, p.Y, p.Size * 2f, _glowPaint);

            // Inner core — no blur, same paint each particle, only alpha varies
            var coreAlpha = (byte)(p.Life * 180);
            _corePaint.Color = new SKColor(255, 230, 180, coreAlpha);
            canvas.DrawCircle(p.X, p.Y, p.Size * 0.5f, _corePaint);
        }

        _glowPaint.MaskFilter = null;
    }

    public bool HasActiveParticles => _activeCount > 0;
}

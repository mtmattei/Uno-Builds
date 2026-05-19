using SkiaSharp;
using Windows.Foundation;

namespace KineticSculptor.Sculptor;

internal sealed class Renderer : IDisposable
{
    // Pre-allocated paints — never construct in the per-frame path.
    private readonly SKPaint _fadePaint = new() { Color = new SKColor(7, 9, 15, (byte)(0.085f * 255)) };
    private readonly SKPaint _particlePaint = new() { BlendMode = SKBlendMode.Plus, IsAntialias = true };
    private readonly SKPaint _haloPaint = new() { BlendMode = SKBlendMode.Plus, IsAntialias = true };
    private readonly SKPaint _ringPaint = new()
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1.2f,
        BlendMode = SKBlendMode.Plus,
        IsAntialias = true
    };
    private readonly SKPaint _cursorPaint = new() { BlendMode = SKBlendMode.Plus };
    private readonly SKPaint _blitPaint = new();

    private SKSurface? _accumulator;
    private int _accW, _accH;

    public void Draw(SKCanvas canvas, Size area, Simulation sim, PointerInput input)
    {
        int w = (int)MathF.Ceiling((float)area.Width);
        int h = (int)MathF.Ceiling((float)area.Height);
        if (w <= 0 || h <= 0) return;

        if (_accumulator is null || _accW != w || _accH != h)
        {
            _accumulator?.Dispose();
            // Opaque accumulator — first frame must show the dark background, not transparent.
            var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
            _accumulator = SKSurface.Create(info);
            _accW = w;
            _accH = h;
            _accumulator.Canvas.Clear(new SKColor(7, 9, 15));
        }

        var acc = _accumulator!.Canvas;

        // 1. Trail fade — semi-transparent fill over the accumulator.
        acc.DrawRect(0, 0, w, h, _fadePaint);

        // 2. Particles + halos (additive).
        var particles = sim.Particles;
        for (int i = 0; i < particles.Length; i++)
        {
            ref readonly Particle p = ref particles[i];
            byte alpha;
            SKColor color = RampColor(p.E, out alpha);
            float speed = MathF.Sqrt(p.Vx * p.Vx + p.Vy * p.Vy);
            float size = 0.55f + p.E * 2.4f + MathF.Min(speed, 220f) * 0.0045f;

            _particlePaint.Color = color.WithAlpha(alpha);
            acc.DrawCircle(p.X, p.Y, size, _particlePaint);

            if (p.E > 0.35f)
            {
                float haloA = (p.E - 0.35f) * 0.18f;
                _haloPaint.Color = color.WithAlpha((byte)(haloA * 255));
                acc.DrawCircle(p.X, p.Y, size * 4.5f, _haloPaint);
            }
        }

        // 3. Shockwave rings.
        var shocks = sim.Shocks;
        for (int i = 0; i < shocks.Count; i++)
        {
            var s = shocks[i];
            _ringPaint.Color = new SKColor(255, 198, 132, (byte)(s.Strength * 0.22f * 255));
            acc.DrawCircle(s.X, s.Y, s.R, _ringPaint);
        }

        // 4. Cursor glow — drawn into the accumulator so it leaves a brief trail.
        if (input.X > -9000f)
        {
            bool holding = input.IsDown && input.MovedSinceDown < 6f && input.HeldMs > 180;
            float r1 = holding ? 70f : 32f;
            float aIn = holding ? 0.32f : 0.14f;

            using var glow = SKShader.CreateRadialGradient(
                new SKPoint(input.X, input.Y),
                r1,
                new[] { new SKColor(255, 218, 168, (byte)(aIn * 255)), new SKColor(255, 218, 168, 0) },
                new[] { 0f, 1f },
                SKShaderTileMode.Clamp);

            _cursorPaint.Shader = glow;
            acc.DrawRect(input.X - r1, input.Y - r1, r1 * 2, r1 * 2, _cursorPaint);
            _cursorPaint.Shader = null;
        }

        acc.Flush();

        // 5. Blit accumulator snapshot to the host canvas.
        using var snap = _accumulator.Snapshot();
        canvas.DrawImage(snap, 0, 0, _blitPaint);
    }

    private static SKColor RampColor(float e, out byte alphaByte)
    {
        byte r, g, b;
        float alpha;
        if (e < 0.32f)
        {
            float k = e / 0.32f;
            r = (byte)(200 + k * 35);
            g = (byte)(195 + k * 25);
            b = (byte)(178 + k * 18);
            alpha = 0.16f + e * 0.55f;
        }
        else if (e < 0.72f)
        {
            float k = (e - 0.32f) / 0.4f;
            r = (byte)(235 + k * 20);
            g = (byte)(220 - k * 50);
            b = (byte)(196 - k * 130);
            alpha = 0.42f + e * 0.42f;
        }
        else
        {
            float k = (e - 0.72f) / 0.28f;
            r = 255;
            g = (byte)(170 + k * 70);
            b = (byte)(66 + k * 150);
            alpha = 0.78f + e * 0.22f;
        }
        alphaByte = (byte)Math.Clamp((int)(alpha * 255), 0, 255);
        return new SKColor(r, g, b);
    }

    public void Dispose()
    {
        _accumulator?.Dispose();
        _fadePaint.Dispose();
        _particlePaint.Dispose();
        _haloPaint.Dispose();
        _ringPaint.Dispose();
        _cursorPaint.Dispose();
        _blitPaint.Dispose();
    }
}

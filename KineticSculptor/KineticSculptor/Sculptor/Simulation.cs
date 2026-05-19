using KineticSculptor.Sculptor.Internal;

namespace KineticSculptor.Sculptor;

internal struct Particle
{
    public float Ax, Ay;
    public float X, Y;
    public float Vx, Vy;
    public float E;
    public float Seed;
}

internal struct Push
{
    public float X, Y;
    public float Dx, Dy;
    public float Life;
    public float Speed;
}

internal struct Shock
{
    public float X, Y;
    public float R;
    public float MaxR;
    public float Strength;
}

internal sealed class Simulation
{
    private const int MaxPushes = 80;

    private Particle[] _particles = Array.Empty<Particle>();
    private float _w, _h;
    private readonly List<Push> _pushes = new(MaxPushes);
    private readonly List<Shock> _shocks = new(8);

    public ReadOnlySpan<Particle> Particles => _particles;
    public IReadOnlyList<Shock> Shocks => _shocks;
    public float Width => _w;
    public float Height => _h;

    public void Resize(float w, float h)
    {
        _w = w;
        _h = h;
        // Always reinit on resize so the field repopulates the new area immediately
        // (matches JS prototype's resize() → initParticles()).
        int target = ParticleCount(w, h);
        _particles = new Particle[target];
        var rng = Random.Shared;
        for (int i = 0; i < target; i++)
        {
            float ax = rng.NextSingle() * w;
            float ay = rng.NextSingle() * h;
            _particles[i] = new Particle
            {
                Ax = ax,
                Ay = ay,
                X = ax + (rng.NextSingle() - 0.5f) * 24f,
                Y = ay + (rng.NextSingle() - 0.5f) * 24f,
                Vx = 0f,
                Vy = 0f,
                E = 0.04f + rng.NextSingle() * 0.08f,
                Seed = rng.NextSingle() * 1000f
            };
        }
    }

    private static int ParticleCount(float w, float h)
    {
        // Base formula from spec §5; reduce density on WASM.
#if __WASM__
        return Math.Clamp((int)((w * h) / 1800.0), 600, 1400);
#else
        return Math.Clamp((int)((w * h) / 900.0), 900, 2400);
#endif
    }

    public void AddPush(float x, float y, float dx, float dy, float life, float speed)
    {
        if (_pushes.Count >= MaxPushes)
            _pushes.RemoveAt(0);
        _pushes.Add(new Push { X = x, Y = y, Dx = dx, Dy = dy, Life = life, Speed = speed });
    }

    public void AddShock(float x, float y)
    {
        _shocks.Add(new Shock
        {
            X = x,
            Y = y,
            R = 0f,
            MaxR = MathF.Sqrt(_w * _w + _h * _h) * 0.85f,
            Strength = 1f
        });
    }

    public void Step(float dt, float t, PointerInput input)
    {
        if (_particles.Length == 0) return;

        // Decay shockwaves (back-to-front so RemoveAt is cheap).
        for (int i = _shocks.Count - 1; i >= 0; i--)
        {
            var s = _shocks[i];
            s.R += dt * 760f;
            s.Strength *= MathF.Pow(0.55f, dt);
            if (s.R > s.MaxR || s.Strength < 0.02f)
                _shocks.RemoveAt(i);
            else
                _shocks[i] = s;
        }

        // Decay pushes.
        for (int i = _pushes.Count - 1; i >= 0; i--)
        {
            var p = _pushes[i];
            p.Life -= dt * 2.2f;
            if (p.Life <= 0f)
                _pushes.RemoveAt(i);
            else
                _pushes[i] = p;
        }

        // Hold detection — stationary press becomes a gravity well.
        long heldMs = input.HeldMs;
        bool holding = input.IsDown && input.MovedSinceDown < 6f && heldMs > 180;
        float holdStrength = holding ? MathF.Min(1f, (heldMs - 180f) / 600f) : 0f;
        float holdTangScale = MathF.Min(1f, heldMs / 1500f);
        float holdX = input.DownX, holdY = input.DownY;

        float w = _w, h = _h;

        for (int i = 0; i < _particles.Length; i++)
        {
            ref Particle p = ref _particles[i];

            // Anchor drift with ambient flow.
            var (afx, afy) = Flow.Sample(p.Ax, p.Ay, t * 0.5f);
            p.Ax += afx * 14f * dt;
            p.Ay += afy * 14f * dt;
            if (p.Ax < -30f) p.Ax += w + 60f;
            else if (p.Ax > w + 30f) p.Ax -= w + 60f;
            if (p.Ay < -30f) p.Ay += h + 60f;
            else if (p.Ay > h + 30f) p.Ay -= h + 60f;

            float fx = 0f, fy = 0f;

            // Spring back to anchor.
            float sx = p.Ax - p.X;
            float sy = p.Ay - p.Y;
            float stretch = MathF.Sqrt(sx * sx + sy * sy);
            fx += sx * 1.6f;
            fy += sy * 1.6f;

            // Local higher-frequency ambient flow.
            var (lfx, lfy) = Flow.Sample(p.X, p.Y, t);
            fx += lfx * 32f;
            fy += lfy * 32f;

            // Drag pushes.
            for (int j = 0; j < _pushes.Count; j++)
            {
                var ps = _pushes[j];
                float ddx = p.X - ps.X;
                float ddy = p.Y - ps.Y;
                float d2 = ddx * ddx + ddy * ddy;
                const float R = 130f;
                if (d2 < R * R)
                {
                    float d = MathF.Sqrt(d2) + 0.001f;
                    float falloff = (1f - d / R) * ps.Life;
                    float turb = Math.Clamp((ps.Speed - 350f) / 1400f, 0f, 1f);

                    float smoothK = (1f - turb) * falloff * 14f;
                    fx += ps.Dx * smoothK;
                    fy += ps.Dy * smoothK;

                    if (turb > 0.01f)
                    {
                        float perpX = -ps.Dy;
                        float perpY = ps.Dx;
                        float swirlPhase = MathF.Sin(d * 0.13f + p.Seed + t * 7f);
                        float swirlK = turb * falloff * 26f * swirlPhase;
                        fx += perpX * swirlK;
                        fy += perpY * swirlK;
                        fx += MathF.Sin(p.Seed * 3.7f + t * 11f) * 22f * turb * falloff;
                        fy += MathF.Cos(p.Seed * 4.1f + t * 13f) * 22f * turb * falloff;
                    }

                    p.E = MathF.Min(1f, p.E + falloff * (0.18f + turb * 0.55f) * dt * 7f);
                }
            }

            // Shockwave rings.
            for (int j = 0; j < _shocks.Count; j++)
            {
                var s = _shocks[j];
                float ddx = p.X - s.X;
                float ddy = p.Y - s.Y;
                float d = MathF.Sqrt(ddx * ddx + ddy * ddy) + 0.001f;
                const float ringW = 55f;
                float off = d - s.R;
                if (MathF.Abs(off) < ringW)
                {
                    float k = (1f - MathF.Abs(off) / ringW) * s.Strength;
                    float push = k * 950f;
                    fx += ddx / d * push;
                    fy += ddy / d * push;
                    p.E = MathF.Min(1f, p.E + k * 0.7f);
                }
            }

            // Hold gravity well + tangential swirl.
            if (holdStrength > 0f)
            {
                float ddx = holdX - p.X;
                float ddy = holdY - p.Y;
                float d = MathF.Sqrt(ddx * ddx + ddy * ddy) + 0.001f;
                const float R = 260f;
                if (d < R)
                {
                    float k = (1f - d / R) * holdStrength;
                    float pull = k * 380f;
                    fx += ddx / d * pull;
                    fy += ddy / d * pull;
                    float tangK = k * 220f * holdTangScale;
                    fx += -ddy / d * tangK;
                    fy += ddx / d * tangK;
                    p.E = MathF.Min(1f, p.E + k * 0.4f * dt);
                }
            }

            // Stretch glow.
            if (stretch > 30f)
            {
                p.E = MathF.Max(p.E, MathF.Min(0.85f, (stretch - 30f) / 180f));
            }

            // Integrate.
            p.Vx += fx * dt;
            p.Vy += fy * dt;
            float damp = MathF.Pow(0.86f, dt * 60f);
            p.Vx *= damp;
            p.Vy *= damp;
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;

            p.E *= MathF.Pow(0.91f, dt * 60f);
        }
    }
}

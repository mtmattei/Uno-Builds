namespace KineticSculptor.Sculptor.Internal;

internal static class Flow
{
    public static (float fx, float fy) Sample(float x, float y, float t)
    {
        const float s = 0.0013f;
        float a = MathF.Sin(x * s + t * 0.32f) * MathF.Cos(y * s * 1.21f + t * 0.27f);
        float b = MathF.Cos(x * s * 1.07f - t * 0.24f) * MathF.Sin(y * s * 0.91f - t * 0.19f);
        return (-b, a);
    }
}

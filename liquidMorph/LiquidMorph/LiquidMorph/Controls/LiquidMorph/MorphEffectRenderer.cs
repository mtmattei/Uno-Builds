using System;
using System.Threading.Tasks;

namespace LiquidMorph.Controls.LiquidMorph;

/// <summary>
/// Effect pipeline: Turbulence noise -> Pixel displacement -> Gaussian blur.
/// Pre-generates noise bitmaps at 4 discrete frequencies and steps through
/// them during the animation, matching the reference spec's frequency ramping.
///
/// Performance optimizations:
/// - Cached SKPaint/SKImageFilter (no per-frame allocations)
/// - Pre-multiplied displacement scale (one multiply per frame, not per pixel)
/// - Parallel.For over scanlines for multi-core displacement
/// - Zero-centered noise to eliminate directional drift
/// </summary>
public sealed class MorphEffectRenderer : IDisposable
{
    private SKBitmap?[] _noiseBitmaps = new SKBitmap?[4];
    private SKBitmap? _outputBitmap;
    private int _width;
    private int _height;
    private const int NumOctaves = 3;
    private int _seed = Random.Shared.Next();

    private static readonly float[] Frequencies = [0.015f, 0.025f, 0.04f, 0.06f];

    private readonly float[] _meanR = new float[4];
    private readonly float[] _meanG = new float[4];

    // Cached paint/filter - avoid per-frame allocation
    private readonly SKPaint _paint = new() { IsAntialias = true };
    private SKImageFilter? _cachedBlurFilter;
    private float _cachedBlurAmount = -1f;

    public float DisplacementAmount { get; set; }
    public float BlurAmount { get; set; }
    public int EdgePadding { get; set; }
    public float AnimationProgress { get; set; }
    public float ContentOpacity { get; set; } = 1f;

    public void SetSize(int width, int height)
    {
        if (_width == width && _height == height) return;
        _width = width;
        _height = height;
        _outputBitmap?.Dispose();
        _outputBitmap = null;
        RegenerateAllNoise();
    }

    /// <summary>
    /// Randomize seed and regenerate noise for a fresh transition.
    /// </summary>
    public void Reseed()
    {
        _seed = Random.Shared.Next();
        RegenerateAllNoise();
    }

    private unsafe void RegenerateAllNoise()
    {
        if (_width <= 0 || _height <= 0) return;

        for (int i = 0; i < Frequencies.Length; i++)
        {
            _noiseBitmaps[i]?.Dispose();

            var info = new SKImageInfo(_width, _height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _noiseBitmaps[i] = new SKBitmap(info);

            using var canvas = new SKCanvas(_noiseBitmaps[i]);
            using var paint = new SKPaint();
            float freq = Frequencies[i];
            paint.Shader = SKShader.CreatePerlinNoiseTurbulence(freq, freq, NumOctaves, _seed);
            canvas.DrawRect(0, 0, _width, _height, paint);

            var ptr = (byte*)_noiseBitmaps[i]!.GetPixels();
            int rowBytes = _noiseBitmaps[i]!.RowBytes;
            long sumR = 0, sumG = 0;
            long count = (long)_width * _height;
            const int bpp = 4;

            for (int y = 0; y < _height; y++)
            {
                var row = ptr + y * rowBytes;
                for (int x = 0; x < _width; x++)
                {
                    int off = x * bpp;
                    sumR += row[off];
                    sumG += row[off + 1];
                }
            }

            _meanR[i] = (float)sumR / count / 255f;
            _meanG[i] = (float)sumG / count / 255f;
        }
    }

    private int SelectNoiseIndex()
    {
        int stepCount = Frequencies.Length;

        if (AnimationProgress <= 0.5f)
        {
            float localT = AnimationProgress / 0.5f;
            return Math.Clamp((int)(localT * stepCount), 0, stepCount - 1);
        }
        else
        {
            float localT = (AnimationProgress - 0.5f) / 0.5f;
            return Math.Clamp(stepCount - 1 - (int)(localT * stepCount), 0, stepCount - 1);
        }
    }

    public void Render(SKCanvas canvas, SKBitmap source)
    {
        if (source is null) return;
        if (_width <= 0 || _height <= 0) return;

        int pad = EdgePadding;

        // Fast path: no effect, draw source centered (no SKImage wrapper needed)
        if (DisplacementAmount < 0.5f && BlurAmount < 0.5f)
        {
            canvas.DrawBitmap(source, pad, pad);
            return;
        }

        int noiseIndex = SelectNoiseIndex();
        var noiseBitmap = _noiseBitmaps[noiseIndex];
        if (noiseBitmap is null) return;

        // Ensure output bitmap
        if (_outputBitmap is null ||
            _outputBitmap.Width != _width ||
            _outputBitmap.Height != _height)
        {
            _outputBitmap?.Dispose();
            var info = new SKImageInfo(_width, _height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _outputBitmap = new SKBitmap(info);
        }

        // Apply displacement with parallel scanlines
        ApplyDisplacement(source, noiseBitmap, _outputBitmap, DisplacementAmount,
            _meanR[noiseIndex], _meanG[noiseIndex], pad);

        // Update cached blur filter only when blur amount changes meaningfully
        float roundedBlur = MathF.Round(BlurAmount * 2f) / 2f; // snap to 0.5 steps
        if (roundedBlur != _cachedBlurAmount)
        {
            _cachedBlurFilter?.Dispose();
            _cachedBlurFilter = roundedBlur > 0.5f
                ? SKImageFilter.CreateBlur(roundedBlur, roundedBlur)
                : null;
            _cachedBlurAmount = roundedBlur;
        }

        _paint.ImageFilter = _cachedBlurFilter;
        byte alpha = (byte)(255 * Math.Clamp(ContentOpacity, 0f, 1f));
        _paint.Color = new SKColor(255, 255, 255, alpha);
        canvas.DrawBitmap(_outputBitmap, 0, 0, _paint);
    }

    private static unsafe void ApplyDisplacement(
        SKBitmap source, SKBitmap noise, SKBitmap output, float amount,
        float meanR, float meanG, int edgePad)
    {
        int w = output.Width;
        int h = output.Height;

        var srcPtr = (byte*)source.GetPixels();
        var nzPtr = (byte*)noise.GetPixels();
        var dstPtr = (byte*)output.GetPixels();

        int srcRowBytes = source.RowBytes;
        int nzRowBytes = noise.RowBytes;
        int dstRowBytes = output.RowBytes;
        int srcW = source.Width;
        int srcH = source.Height;
        const int bpp = 4;

        // Pre-multiply scale once instead of per-pixel
        float scale = amount * 3.5f;

        Parallel.For(0, h, y =>
        {
            var nzRow = nzPtr + y * nzRowBytes;
            var dstRow = dstPtr + y * dstRowBytes;

            for (int x = 0; x < w; x++)
            {
                int nzOff = x * bpp;
                float nr = nzRow[nzOff] / 255f;
                float ng = nzRow[nzOff + 1] / 255f;

                float ox = (nr - meanR) * scale;
                float oy = (ng - meanG) * scale;

                int sx = (int)(x - edgePad + ox);
                int sy = (int)(y - edgePad + oy);

                // Clamp to edge - border pixels stretch outward
                sx = Math.Clamp(sx, 0, srcW - 1);
                sy = Math.Clamp(sy, 0, srcH - 1);

                int dstOff = x * bpp;
                int srcOff = sy * srcRowBytes + sx * bpp;
                *(int*)(dstRow + dstOff) = *(int*)(srcPtr + srcOff);
            }
        });
    }

    public void Dispose()
    {
        for (int i = 0; i < _noiseBitmaps.Length; i++)
        {
            _noiseBitmaps[i]?.Dispose();
            _noiseBitmaps[i] = null;
        }
        _outputBitmap?.Dispose();
        _outputBitmap = null;
        _cachedBlurFilter?.Dispose();
        _cachedBlurFilter = null;
        _paint.Dispose();
    }
}

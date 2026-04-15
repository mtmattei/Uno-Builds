using System;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PhosphorProtocol.SkiaControls;

public sealed class SpectrumVisualizer : SKXamlCanvas
{
    private int _tick;
    private readonly DispatcherTimer _timer;
    private readonly float[] _currentLevels = new float[28];
    private readonly float[] _targetLevels = new float[28];
    private readonly Random _random = new();

    private readonly SKPaint _barPaint;
    private readonly SKColor _peakColor = SKColor.Parse("#6FFCF6");
    private readonly SKColor _brightColor = SKColor.Parse("#3AABA6");
    private readonly SKColor _glowColor = SKColor.Parse("#247070");

    public SpectrumVisualizer()
    {
        _barPaint = new SKPaint { IsAntialias = true };

        // Initialize levels
        for (int i = 0; i < 28; i++)
        {
            _currentLevels[i] = 0.1f;
            _targetLevels[i] = (float)_random.NextDouble();
        }

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(70) };
        _timer.Tick += (_, _) =>
        {
            _tick++;
            // Generate new targets periodically
            if (_tick % 3 == 0)
            {
                for (int i = 0; i < 28; i++)
                    _targetLevels[i] = (float)(0.1 + _random.NextDouble() * 0.9);
            }
            // Interpolate toward targets
            for (int i = 0; i < 28; i++)
                _currentLevels[i] += (_targetLevels[i] - _currentLevels[i]) * 0.25f;

            Invalidate();
        };

        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
        PaintSurface += OnPaintSurface;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColor.Parse("#020707"));

        float w = info.Width;
        float h = info.Height;
        float barGap = 2.5f;
        float barMaxWidth = Math.Min(12f, (w - barGap * 27) / 28f);
        float totalBarsWidth = 28 * barMaxWidth + 27 * barGap;
        float startX = (w - totalBarsWidth) / 2f;
        float minHeight = h * 0.06f;

        for (int i = 0; i < 28; i++)
        {
            float level = _currentLevels[i];
            float barH = minHeight + level * (h - minHeight);
            float barX = startX + i * (barMaxWidth + barGap);
            float barY = h - barH;

            SKColor color;
            if (level > 0.5f) color = _peakColor;
            else if (level > 0.3f) color = _brightColor;
            else color = _glowColor;

            _barPaint.Color = color;
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(barX, barY, barX + barMaxWidth, h), 2, 0), _barPaint);
        }
    }
}

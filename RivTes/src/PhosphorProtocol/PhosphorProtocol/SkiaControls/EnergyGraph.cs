using System;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PhosphorProtocol.SkiaControls;

public sealed class EnergyGraph : SKXamlCanvas
{
    private int _tick;
    private readonly DispatcherTimer _timer;
    private readonly float[] _data = new float[40];
    private readonly Random _random = new();

    private readonly SKPaint _gridPaint;
    private readonly SKPaint _areaFillPaint;
    private readonly SKPaint _linePaint;
    private readonly SKPaint _cursorPaint;
    private readonly SKPaint _labelPaint;

    public EnergyGraph()
    {
        _gridPaint = new SKPaint
        {
            Color = SKColor.Parse("#0A2222"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.5f,
            PathEffect = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0),
            IsAntialias = true
        };
        _areaFillPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6").WithAlpha(10),
            IsAntialias = true
        };
        _linePaint = new SKPaint
        {
            Color = SKColor.Parse("#3AABA6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.8f,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            IsAntialias = true
        };
        _cursorPaint = new SKPaint
        {
            Color = SKColor.Parse("#6FFCF6"),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };
        _labelPaint = new SKPaint
        {
            Color = SKColor.Parse("#143838"),
            TextSize = 8,
            IsAntialias = true
        };

        // Generate initial data
        for (int i = 0; i < 40; i++)
            _data[i] = 180f + (float)_random.NextDouble() * 120f;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += (_, _) =>
        {
            _tick++;
            // Shift data left and add new point
            if (_tick % 10 == 0)
            {
                for (int i = 0; i < 39; i++) _data[i] = _data[i + 1];
                _data[39] = 180f + (float)_random.NextDouble() * 120f;
            }
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
        float padding = 30;
        float graphW = w - padding * 2;
        float graphH = h - padding;
        float maxVal = 400;

        // Grid lines
        for (int i = 0; i <= 4; i++)
        {
            float y = padding / 2 + graphH * i / 4f;
            canvas.DrawLine(padding, y, w - padding, y, _gridPaint);
            int val = (int)(maxVal * (1 - (float)i / 4));
            canvas.DrawText(val.ToString(), 2, y + 3, _labelPaint);
        }

        // Data path
        using var linePath = new SKPath();
        using var areaPath = new SKPath();

        float baseY = padding / 2 + graphH;
        areaPath.MoveTo(padding, baseY);

        for (int i = 0; i < 40; i++)
        {
            float x = padding + graphW * i / 39f;
            float y = padding / 2 + graphH * (1 - _data[i] / maxVal);
            y = Math.Clamp(y, padding / 2, baseY);

            if (i == 0)
            {
                linePath.MoveTo(x, y);
                areaPath.LineTo(x, y);
            }
            else
            {
                linePath.LineTo(x, y);
                areaPath.LineTo(x, y);
            }
        }

        areaPath.LineTo(padding + graphW, baseY);
        areaPath.Close();

        // Draw area fill
        canvas.DrawPath(areaPath, _areaFillPaint);
        // Draw line
        canvas.DrawPath(linePath, _linePaint);

        // Animated cursor at right edge
        float cursorX = padding + graphW;
        float cursorAlpha = (float)(0.3 + Math.Sin(_tick * 0.1) * 0.2);
        _cursorPaint.Color = SKColor.Parse("#6FFCF6").WithAlpha((byte)(cursorAlpha * 255));
        canvas.DrawLine(cursorX, padding / 2, cursorX, baseY, _cursorPaint);
    }
}

using System;
using Microsoft.UI.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Windows;

namespace PhosphorProtocol.SkiaControls;

public sealed class VectorNavMap : SKXamlCanvas
{
    private int _tick;
    private readonly DispatcherTimer _timer;

    // Reusable paints
    private readonly SKPaint _gridMinorPaint;
    private readonly SKPaint _gridMajorPaint;
    private readonly SKPaint _buildingPaint;
    private readonly SKPaint _routePaint;
    private readonly SKPaint _carDotPaint;
    private readonly SKPaint _carConePaint;
    private readonly SKPaint _turnMarkerPaint;
    private readonly SKPaint _etaBackgroundPaint;
    private readonly SKPaint _etaTextPaint;
    private readonly SKPaint _roadLabelPaint;

    public VectorNavMap()
    {
        _gridMinorPaint = new SKPaint { Color = SKColor.Parse("#0A2222"), Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f, IsAntialias = true };
        _gridMajorPaint = new SKPaint { Color = SKColor.Parse("#143838"), Style = SKPaintStyle.Stroke, StrokeWidth = 1f, IsAntialias = true };
        _buildingPaint = new SKPaint { Color = SKColor.Parse("#051212"), IsAntialias = true };
        _routePaint = new SKPaint { Color = SKColor.Parse("#3AABA6"), Style = SKPaintStyle.Stroke, StrokeWidth = 3.5f, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round, IsAntialias = true };
        _carDotPaint = new SKPaint { Color = SKColor.Parse("#6FFCF6"), IsAntialias = true };
        _carConePaint = new SKPaint { Color = SKColor.Parse("#6FFCF6").WithAlpha(80), IsAntialias = true };
        _turnMarkerPaint = new SKPaint { Color = SKColor.Parse("#3AABA6"), Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
        _etaBackgroundPaint = new SKPaint { Color = SKColor.Parse("#020707").WithAlpha(230), IsAntialias = true };
        _etaTextPaint = new SKPaint { Color = SKColor.Parse("#6FFCF6"), TextSize = 11, IsAntialias = true };
        _roadLabelPaint = new SKPaint { Color = SKColor.Parse("#0A2222"), TextSize = 7, IsAntialias = true };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += (_, _) => { _tick++; Invalidate(); };
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

        // Grid
        float gridSpacing = 40;
        for (float x = 0; x < w; x += gridSpacing)
        {
            var paint = ((int)(x / gridSpacing) % 4 == 0) ? _gridMajorPaint : _gridMinorPaint;
            canvas.DrawLine(x, 0, x, h, paint);
        }
        for (float y = 0; y < h; y += gridSpacing)
        {
            var paint = ((int)(y / gridSpacing) % 4 == 0) ? _gridMajorPaint : _gridMinorPaint;
            canvas.DrawLine(0, y, w, y, paint);
        }

        // Buildings (scattered rounded rects)
        var buildings = new[]
        {
            new SKRect(w * 0.1f, h * 0.1f, w * 0.2f, h * 0.2f),
            new SKRect(w * 0.7f, h * 0.05f, w * 0.85f, h * 0.15f),
            new SKRect(w * 0.05f, h * 0.6f, w * 0.15f, h * 0.75f),
            new SKRect(w * 0.6f, h * 0.5f, w * 0.75f, h * 0.65f),
            new SKRect(w * 0.35f, h * 0.15f, w * 0.5f, h * 0.25f),
            new SKRect(w * 0.8f, h * 0.7f, w * 0.95f, h * 0.85f),
        };
        foreach (var b in buildings)
            canvas.DrawRoundRect(new SKRoundRect(b, 4), _buildingPaint);

        // Road labels
        canvas.DrawText("RUE SHERBROOKE", w * 0.25f, h * 0.38f, _roadLabelPaint);
        canvas.DrawText("BLVD ST-LAURENT", w * 0.48f, h * 0.5f, _roadLabelPaint);

        // Route polyline
        using var routePath = new SKPath();
        routePath.MoveTo(w * 0.5f, h * 0.9f);
        routePath.LineTo(w * 0.5f, h * 0.6f);
        routePath.LineTo(w * 0.7f, h * 0.4f);
        routePath.LineTo(w * 0.7f, h * 0.1f);

        // Route gradient
        _routePaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(w * 0.5f, h * 0.9f),
            new SKPoint(w * 0.7f, h * 0.1f),
            new[] { SKColor.Parse("#247070"), SKColor.Parse("#3AABA6"), SKColor.Parse("#6FFCF6") },
            new[] { 0f, 0.5f, 1f },
            SKShaderTileMode.Clamp);
        canvas.DrawPath(routePath, _routePaint);

        // Turn markers
        canvas.DrawCircle(w * 0.5f, h * 0.6f, 8, _turnMarkerPaint);
        canvas.DrawText("1", w * 0.5f - 3, h * 0.6f + 3, _etaTextPaint);
        canvas.DrawCircle(w * 0.7f, h * 0.4f, 8, _turnMarkerPaint);
        canvas.DrawText("2", w * 0.7f - 3, h * 0.4f + 3, _etaTextPaint);

        // Car position (pulsing dot)
        float carX = w * 0.5f;
        float carY = h * 0.85f;
        float dotRadius = 5 + (float)Math.Sin(_tick * 0.08) * 1.8f;
        canvas.DrawCircle(carX, carY, dotRadius, _carDotPaint);

        // Direction cone
        using var conePath = new SKPath();
        conePath.MoveTo(carX, carY - 12);
        conePath.LineTo(carX - 6, carY - 2);
        conePath.LineTo(carX + 6, carY - 2);
        conePath.Close();
        canvas.DrawPath(conePath, _carConePaint);

        // ETA overlay (bottom-right)
        float etaW = 70, etaH = 28;
        float etaX = w - etaW - 10, etaY = h - etaH - 10;
        canvas.DrawRoundRect(new SKRoundRect(new SKRect(etaX, etaY, etaX + etaW, etaY + etaH), 6), _etaBackgroundPaint);
        canvas.DrawText("ETA 11:42", etaX + 8, etaY + 18, _etaTextPaint);
    }
}

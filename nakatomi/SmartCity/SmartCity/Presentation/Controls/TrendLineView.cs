using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SmartCity.Models;

namespace SmartCity.Presentation.Controls;

/// <summary>
/// Monthly-savings trend line drawn with SkiaSharp (custom, Tier 4: bespoke neon look, no Toolkit
/// equivalent). Renders a teal polyline with a soft gradient area fill and a glowing end marker.
/// </summary>
public sealed partial class TrendLineView : SKCanvasElement
{
    private static readonly SKColor Teal = new(0x2E, 0xE0, 0xE0);
    private static readonly SKColor TealSoft = new(0x2E, 0xE0, 0xE0);
    private static readonly SKColor Muted = new(0x8A, 0x94, 0xA6);

    private readonly SKPaint _label = new() { IsAntialias = true, Color = Muted };
    private readonly SKFont _font = new(SKTypeface.Default, 9.5f);

    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points), typeof(IEnumerable<TrendPoint>), typeof(TrendLineView),
            new PropertyMetadata(null, static (d, _) => ((TrendLineView)d).Invalidate()));

    public IEnumerable<TrendPoint>? Points
    {
        get => (IEnumerable<TrendPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var w = double.IsInfinity(availableSize.Width) ? 320 : availableSize.Width;
        var h = double.IsInfinity(availableSize.Height) ? 96 : availableSize.Height;
        return new Size(w, h);
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        if (!IsSupportedOnCurrentPlatform()) return;
        var data = Points?.ToList();
        if (data is null || data.Count < 2) return;

        var w = (float)area.Width;
        var h = (float)area.Height;
        const float pad = 10f, padBottom = 18f;
        var plotW = w - pad * 2f;
        var plotH = h - pad - padBottom;
        var baseY = pad + plotH;

        var min = data.Min(p => p.Value);
        var max = data.Max(p => p.Value);
        var range = max - min <= 0 ? 1 : max - min;

        var pts = new SKPoint[data.Count];
        for (var i = 0; i < data.Count; i++)
        {
            var x = pad + plotW * i / (data.Count - 1);
            var y = pad + plotH * (1f - (float)((data[i].Value - min) / range));
            pts[i] = new SKPoint(x, y);
        }

        // Gradient area fill under the line.
        using var area1 = new SKPath();
        area1.MoveTo(pts[0].X, baseY);
        foreach (var p in pts) area1.LineTo(p.X, p.Y);
        area1.LineTo(pts[^1].X, baseY);
        area1.Close();
        using var fill = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, pad), new SKPoint(0, h - pad),
                new[] { TealSoft.WithAlpha(0x55), TealSoft.WithAlpha(0x00) },
                null, SKShaderTileMode.Clamp),
        };
        canvas.DrawPath(area1, fill);

        // The line itself.
        using var line = new SKPath();
        line.MoveTo(pts[0]);
        for (var i = 1; i < pts.Length; i++) line.LineTo(pts[i]);
        using var stroke = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f,
            StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round, Color = Teal,
        };
        canvas.DrawPath(line, stroke);

        // Glowing end marker on the most recent point.
        using var glow = new SKPaint
        {
            IsAntialias = true, Style = SKPaintStyle.Fill, Color = Teal.WithAlpha(0x66),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f),
        };
        using var dot = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = Teal };
        canvas.DrawCircle(pts[^1], 7f, glow);
        canvas.DrawCircle(pts[^1], 3.2f, dot);

        // Axis labels: max value (top-left) + first / mid / last date along the bottom.
        canvas.DrawText(Format.Compact(max), pad, pad + 7f, SKTextAlign.Left, _font, _label);
        canvas.DrawText(data[0].DateLabel, pad, h - 5f, SKTextAlign.Left, _font, _label);
        canvas.DrawText(data[data.Count / 2].DateLabel, w / 2f, h - 5f, SKTextAlign.Center, _font, _label);
        canvas.DrawText(data[^1].DateLabel, w - pad, h - 5f, SKTextAlign.Right, _font, _label);
    }
}

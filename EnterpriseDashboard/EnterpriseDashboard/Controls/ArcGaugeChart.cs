using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class ArcGaugeChart : SKCanvasElement
{
    private IImmutableList<(string Label, double Value, double Max)>? _data;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    public ArcGaugeChart()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_data != null && !_animStarted)
        {
            StartAnimation();
        }
    }

    public void SetData(IImmutableList<(string Label, double Value, double Max)> data, bool terminal = false)
    {
        _data = data;
        _isTerminal = terminal;
        _animStarted = false;
        if (IsLoaded)
        {
            StartAnimation();
        }
    }

    public void SetTheme(bool terminal)
    {
        _isTerminal = terminal;
        if (_animProgress >= 1f) Invalidate();
    }

    private void StartAnimation()
    {
        _animStarted = true;
        _animProgress = 0f;
        _timer?.Stop();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (s, e) =>
        {
            _animProgress = Math.Min(1f, _animProgress + 0.02f);
            Invalidate();
            if (_animProgress >= 1f) _timer.Stop();
        };
        _timer.Start();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        canvas.Clear(SKColors.Transparent);
        if (_data == null || _data.Count == 0 || area.Width < 1 || area.Height < 1) return;

        float w = (float)area.Width;
        float h = (float)area.Height;
        float cx = w / 2f;
        float cy = h / 2f;
        float maxRadius = Math.Min(cx, cy) - 16f;

        int ringCount = _data.Count;
        float strokeWidth = maxRadius / (ringCount * 2.5f);
        float startAngle = 135f;
        float sweepMax = 270f;
        float progress = _animStarted ? _animProgress : 1f;

        SKColor[] ringColors = _isTerminal
            ? [new(0x00, 0xFF, 0x88), new(0x00, 0xDD, 0x66), new(0x00, 0xBB, 0x55), new(0x00, 0x99, 0x44)]
            : [new(0xFF, 0xFF, 0xFF), new(0xCC, 0xCC, 0xCC), new(0x99, 0x99, 0x99), new(0x66, 0x66, 0x66)];

        using var bgPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            Color = ObservatoryColors.GetGridLine(_isTerminal)
        };

        using var fgPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        for (int i = 0; i < ringCount; i++)
        {
            float radius = maxRadius - i * (strokeWidth * 2.2f);
            var rect = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);

            canvas.DrawArc(rect, startAngle, sweepMax, false, bgPaint);

            double pct = _data[i].Value / _data[i].Max;
            float sweep = (float)(sweepMax * pct * progress);
            fgPaint.Color = ringColors[i % ringColors.Length];
            canvas.DrawArc(rect, startAngle, sweep, false, fgPaint);
        }

        double totalPct = _data.Average(d => d.Value / d.Max) * 100;
        using var titleFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 28f);
        using var textPaint = new SKPaint
        {
            Color = ObservatoryColors.GetHighContrast(_isTerminal),
            IsAntialias = true
        };
        canvas.DrawText($"{totalPct * progress:F0}%", cx, cy + 10, SKTextAlign.Center, titleFont, textPaint);

        using var labelFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 10f);
        using var labelPaint = new SKPaint
        {
            Color = ObservatoryColors.GetText(_isTerminal),
            IsAntialias = true
        };
        var labelsStr = string.Join("  ", _data.Select(d => d.Label));
        canvas.DrawText(labelsStr, cx, cy + 30, SKTextAlign.Center, labelFont, labelPaint);
    }
}

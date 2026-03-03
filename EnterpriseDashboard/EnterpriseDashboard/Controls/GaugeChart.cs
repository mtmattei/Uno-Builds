using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class GaugeChart : SKCanvasElement
{
    private double _value;
    private double _min;
    private double _max;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;
    private bool _hasData;

    public GaugeChart()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_hasData && !_animStarted)
        {
            StartAnimation();
        }
    }

    public void SetData(double value, double min, double max, bool terminal = false)
    {
        _value = value;
        _min = min;
        _max = max;
        _isTerminal = terminal;
        _hasData = true;
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
            _animProgress = Math.Min(1f, _animProgress + 0.015f);
            Invalidate();
            if (_animProgress >= 1f) _timer.Stop();
        };
        _timer.Start();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        canvas.Clear(SKColors.Transparent);
        if (!_hasData || area.Width < 1 || area.Height < 1) return;

        float w = (float)area.Width;
        float h = (float)area.Height;
        float cx = w / 2f;
        float cy = h * 0.6f;
        float radius = Math.Min(cx, cy) - 20f;

        float arcStartAngle = 180f;
        float progress = _animStarted ? _animProgress : 1f;

        float strokeWidth = 12f;
        using var bgPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Butt,
            Color = ObservatoryColors.GetGridLine(_isTerminal)
        };

        var arcRect = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);
        canvas.DrawArc(arcRect, arcStartAngle, 180f, false, bgPaint);

        DrawZone(canvas, arcRect, arcStartAngle, 0f, 0.6f, ObservatoryColors.GetMid(_isTerminal), strokeWidth);
        DrawZone(canvas, arcRect, arcStartAngle, 0.6f, 0.8f, ObservatoryColors.GetBright(_isTerminal), strokeWidth);
        DrawZone(canvas, arcRect, arcStartAngle, 0.8f, 1.0f, ObservatoryColors.GetHighContrast(_isTerminal), strokeWidth);

        using var tickPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Color = ObservatoryColors.GetSubtle(_isTerminal)
        };

        for (int i = 0; i <= 10; i++)
        {
            float angle = (float)(Math.PI + Math.PI * i / 10.0);
            float innerR = radius - strokeWidth / 2 - 8;
            float outerR = radius - strokeWidth / 2 - 2;
            float x1 = cx + innerR * (float)Math.Cos(angle);
            float y1 = cy + innerR * (float)Math.Sin(angle);
            float x2 = cx + outerR * (float)Math.Cos(angle);
            float y2 = cy + outerR * (float)Math.Sin(angle);
            canvas.DrawLine(x1, y1, x2, y2, tickPaint);
        }

        double pct = (_value - _min) / (_max - _min);
        float needleAngle = (float)(Math.PI + Math.PI * pct * progress);
        float needleLen = radius - 30f;

        using var needlePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round,
            Color = ObservatoryColors.GetHighContrast(_isTerminal)
        };

        float nx = cx + needleLen * (float)Math.Cos(needleAngle);
        float ny = cy + needleLen * (float)Math.Sin(needleAngle);
        canvas.DrawLine(cx, cy, nx, ny, needlePaint);

        using var dotPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = ObservatoryColors.GetHighContrast(_isTerminal)
        };
        canvas.DrawCircle(cx, cy, 5, dotPaint);

        double displayVal = _min + (_value - _min) * progress;
        using var valueFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 28f);
        using var valuePaint = new SKPaint
        {
            Color = ObservatoryColors.GetHighContrast(_isTerminal),
            IsAntialias = true
        };
        canvas.DrawText($"{displayVal:F1}", cx, cy + 36, SKTextAlign.Center, valueFont, valuePaint);

        using var labelFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 10f);
        using var labelPaint = new SKPaint
        {
            Color = ObservatoryColors.GetText(_isTerminal),
            IsAntialias = true
        };
        canvas.DrawText($"{_min:F0}", cx - radius, cy + 16, SKTextAlign.Left, labelFont, labelPaint);
        canvas.DrawText($"{_max:F0}", cx + radius, cy + 16, SKTextAlign.Right, labelFont, labelPaint);
    }

    private static void DrawZone(SKCanvas canvas, SKRect rect, float startAngle, float fromPct, float toPct, SKColor color, float strokeWidth)
    {
        float zoneStart = startAngle + fromPct * 180f;
        float zoneSweep = (toPct - fromPct) * 180f;
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Butt,
            Color = color.WithAlpha(80)
        };
        canvas.DrawArc(rect, zoneStart, zoneSweep, false, paint);
    }
}

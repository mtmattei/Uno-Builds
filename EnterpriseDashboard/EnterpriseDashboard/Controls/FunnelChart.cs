using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class FunnelChart : SKCanvasElement
{
    private IImmutableList<FunnelStage>? _data;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    public FunnelChart()
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

    public void SetData(IImmutableList<FunnelStage> data, bool terminal = false)
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
        float progress = _animStarted ? _animProgress : 1f;

        int count = _data.Count;
        float padding = 12f;
        float rightMargin = 120f;
        float availW = w - padding * 2 - rightMargin;
        float availH = h - padding * 2;
        float stageH = availH / count;
        float gap = 3f;

        double maxValue = _data.Max(s => s.Value);

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var edgePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f
        };
        using var labelFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Medium, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 12f);
        using var valueFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 10f);
        using var labelPaint = new SKPaint { IsAntialias = true };
        using var pctFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 14f);

        int visibleStages = (int)(count * progress);
        if (visibleStages < 1 && progress > 0) visibleStages = 1;

        for (int i = 0; i < visibleStages; i++)
        {
            var stage = _data[i];
            float stageAlpha = Math.Min(1f, (progress * count - i) * 2f);

            // Tapered trapezoid: wider at top, narrower at bottom
            float topWidth = (float)(stage.Value / maxValue) * availW;
            float nextValue = (i + 1 < count) ? (float)_data[i + 1].Value : (float)(stage.Value * 0.3);
            float botWidth = (float)(nextValue / maxValue) * availW;

            // Animate width from 0 to target
            float animFactor = Math.Min(1f, stageAlpha);
            topWidth *= animFactor;
            botWidth *= animFactor;

            float cx = padding + availW / 2f;
            float y1 = padding + i * stageH + gap / 2;
            float y2 = padding + (i + 1) * stageH - gap / 2;

            // Trapezoid path
            using var path = new SKPath();
            path.MoveTo(cx - topWidth / 2, y1);
            path.LineTo(cx + topWidth / 2, y1);
            path.LineTo(cx + botWidth / 2, y2);
            path.LineTo(cx - botWidth / 2, y2);
            path.Close();

            // Brightness decreases per stage (brighter = bigger)
            float brightness = 1f - (float)i / count * 0.8f;
            fillPaint.Color = ObservatoryColors.MapValue(brightness, _isTerminal).WithAlpha((byte)(stageAlpha * 200));
            canvas.DrawPath(path, fillPaint);

            // Glowing edge line at top of each stage
            edgePaint.Color = ObservatoryColors.GetEmphasis(_isTerminal).WithAlpha((byte)(stageAlpha * 100));
            canvas.DrawLine(cx - topWidth / 2, y1, cx + topWidth / 2, y1, edgePaint);

            // Labels on the right side
            float labelX = padding + availW + 12f;
            float labelY = (y1 + y2) / 2f;

            // Stage name
            labelPaint.Color = ObservatoryColors.GetHighContrast(_isTerminal).WithAlpha((byte)(stageAlpha * 255));
            canvas.DrawText(stage.Label, labelX, labelY - 4, SKTextAlign.Left, labelFont, labelPaint);

            // Value + percentage
            labelPaint.Color = ObservatoryColors.GetText(_isTerminal).WithAlpha((byte)(stageAlpha * 200));
            canvas.DrawText($"{stage.Value:N0} ({stage.Percentage:F1}%)", labelX, labelY + 12, SKTextAlign.Left, valueFont, labelPaint);
        }

        // Connecting accent line on the left edge
        if (visibleStages > 1)
        {
            float cx = padding + availW / 2f;
            using var accentPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true,
                Color = ObservatoryColors.GetSubtle(_isTerminal),
                PathEffect = SKPathEffect.CreateDash([4f, 4f], 0)
            };

            for (int i = 0; i < visibleStages - 1; i++)
            {
                float topW = (float)(_data[i].Value / maxValue) * availW;
                float botW = (float)(_data[i + 1].Value / maxValue) * availW;
                float y = padding + (i + 1) * stageH;

                // Dashed lines connecting narrowing edges
                canvas.DrawLine(cx - topW / 2, y, cx - botW / 2, y + gap, accentPaint);
                canvas.DrawLine(cx + topW / 2, y, cx + botW / 2, y + gap, accentPaint);
            }
        }
    }
}

using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class HeatmapChart : SKCanvasElement
{
    private double[,]? _data;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    private static readonly string[] DayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public HeatmapChart()
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

    public void SetData(double[,] data, bool terminal = false)
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
            _animProgress = Math.Min(1f, _animProgress + 0.025f);
            Invalidate();
            if (_animProgress >= 1f)
            {
                _timer.Stop();
            }
        };
        _timer.Start();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        canvas.Clear(SKColors.Transparent);
        if (_data == null || area.Width < 1 || area.Height < 1) return;

        float w = (float)area.Width;
        float h = (float)area.Height;
        int rows = _data.GetLength(0);
        int cols = _data.GetLength(1);

        float labelLeft = 32f;
        float labelTop = 16f;
        float gridW = w - labelLeft - 8f;
        float gridH = h - labelTop - 8f;
        float cellW = gridW / cols;
        float cellH = gridH / rows;
        float gap = 1.5f;

        float progress = _animStarted ? _animProgress : 1f;
        int visibleCols = (int)(cols * progress);

        using var labelFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 10f);
        using var labelPaint = new SKPaint
        {
            Color = ObservatoryColors.GetText(_isTerminal),
            IsAntialias = true
        };

        for (int d = 0; d < rows; d++)
        {
            float cy = labelTop + d * cellH + cellH / 2 + 3;
            canvas.DrawText(DayLabels[d], 2, cy, SKTextAlign.Left, labelFont, labelPaint);
        }

        for (int hh = 0; hh < cols; hh += 3)
        {
            if (hh >= visibleCols) break;
            float cx = labelLeft + hh * cellW + cellW / 2;
            canvas.DrawText(hh.ToString(), cx, 12, SKTextAlign.Center, labelFont, labelPaint);
        }

        using var cellPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        for (int d = 0; d < rows; d++)
        {
            for (int hh = 0; hh < visibleCols; hh++)
            {
                float x = labelLeft + hh * cellW + gap / 2;
                float y = labelTop + d * cellH + gap / 2;
                float cw = cellW - gap;
                float ch = cellH - gap;

                cellPaint.Color = ObservatoryColors.MapValue(_data[d, hh], _isTerminal);
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(x, y, x + cw, y + ch), 2), cellPaint);
            }
        }
    }
}

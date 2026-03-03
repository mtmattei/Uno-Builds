using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class WaffleChart : SKCanvasElement
{
    private int _filled;
    private int _total;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    public WaffleChart()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_total > 0 && !_animStarted)
        {
            StartAnimation();
        }
    }

    public void SetData(int filled, int total, bool terminal = false)
    {
        _filled = filled;
        _total = total;
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
        if (_total <= 0 || area.Width < 1 || area.Height < 1) return;

        float w = (float)area.Width;
        float h = (float)area.Height;
        int cols = 10;
        int rows = (_total + cols - 1) / cols;

        float padding = 24f;
        float bottomPad = 32f;
        float gridW = w - padding * 2;
        float gridH = h - padding - bottomPad;
        float cellSize = Math.Min(gridW / cols, gridH / rows);
        float gap = 3f;
        float squareSize = cellSize - gap;

        float offsetX = (w - cols * cellSize) / 2f;
        float offsetY = padding;

        float progress = _animStarted ? _animProgress : 1f;
        int animatedFilled = (int)(_filled * progress);

        using var filledPaint = new SKPaint
        {
            Color = ObservatoryColors.GetHighContrast(_isTerminal),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var emptyPaint = new SKPaint
        {
            Color = ObservatoryColors.GetGridLine(_isTerminal),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        for (int i = 0; i < _total; i++)
        {
            int row = i / cols;
            int col = i % cols;
            float x = offsetX + col * cellSize + gap / 2;
            float y = offsetY + row * cellSize + gap / 2;

            var paint = i < animatedFilled ? filledPaint : emptyPaint;
            canvas.DrawRoundRect(new SKRoundRect(new SKRect(x, y, x + squareSize, y + squareSize), 3), paint);
        }

        int displayPct = (int)(((double)animatedFilled / _total) * 100);
        using var textFont = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 14f);
        using var textPaint = new SKPaint
        {
            Color = ObservatoryColors.GetHighContrast(_isTerminal),
            IsAntialias = true
        };
        canvas.DrawText($"{displayPct}% Complete ({animatedFilled}/{_total})", w / 2, h - 10, SKTextAlign.Center, textFont, textPaint);
    }
}

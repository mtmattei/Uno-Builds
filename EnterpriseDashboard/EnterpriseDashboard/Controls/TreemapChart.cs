using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class TreemapChart : SKCanvasElement
{
    private IImmutableList<TreemapItem>? _data;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    // Group-to-brightness mapping for visual distinction
    private static readonly Dictionary<string, float> GroupBrightness = new()
    {
        ["Infrastructure"] = 0.85f,
        ["Data"] = 0.65f,
        ["Frontend"] = 0.50f,
        ["Compute"] = 0.35f,
        ["Ops"] = 0.20f,
    };

    public TreemapChart()
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

    public void SetData(IImmutableList<TreemapItem> data, bool terminal = false)
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

        // Sort descending by value for squarified layout
        var sorted = _data.OrderByDescending(d => d.Value).ToList();
        double totalValue = sorted.Sum(d => d.Value);

        // Squarified treemap layout
        var rects = ComputeTreemapLayout(sorted, totalValue, 4f, 4f, w - 8f, h - 8f);

        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = ObservatoryColors.GetCardSurface(_isTerminal)
        };
        using var labelFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Medium, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 11f);
        using var labelPaint = new SKPaint { IsAntialias = true };
        using var valueFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 9f);

        int visibleCount = (int)(rects.Count * progress);

        for (int i = 0; i < visibleCount; i++)
        {
            var (item, rect) = rects[i];
            float brightness = GroupBrightness.GetValueOrDefault(item.Group, 0.4f);
            float cellAlpha = Math.Min(1f, (progress * rects.Count - i) * 2f);

            fillPaint.Color = ObservatoryColors.MapValue(brightness, _isTerminal).WithAlpha((byte)(cellAlpha * 200));
            canvas.DrawRoundRect(new SKRoundRect(rect, 4), fillPaint);
            canvas.DrawRoundRect(new SKRoundRect(rect, 4), strokePaint);

            // Only draw labels if cell is large enough
            if (rect.Width > 50 && rect.Height > 30)
            {
                // Determine text color based on fill brightness for readability
                bool darkText = brightness > 0.55f && !_isTerminal;
                labelPaint.Color = darkText
                    ? ObservatoryColors.Background.WithAlpha((byte)(cellAlpha * 220))
                    : ObservatoryColors.GetHighContrast(_isTerminal).WithAlpha((byte)(cellAlpha * 220));

                float textX = rect.Left + 8;
                float textY = rect.Top + 18;
                canvas.DrawText(item.Label, textX, textY, SKTextAlign.Left, labelFont, labelPaint);

                if (rect.Height > 44)
                {
                    labelPaint.Color = labelPaint.Color.WithAlpha((byte)(cellAlpha * 140));
                    canvas.DrawText($"{item.Value:F0}", textX, textY + 14, SKTextAlign.Left, valueFont, labelPaint);
                }
            }
        }
    }

    private static List<(TreemapItem Item, SKRect Rect)> ComputeTreemapLayout(
        List<TreemapItem> items, double totalValue, float x, float y, float w, float h)
    {
        var result = new List<(TreemapItem, SKRect)>();
        SliceAndDice(items, 0, items.Count, x, y, w, h, totalValue, true, result);
        return result;
    }

    private static void SliceAndDice(
        List<TreemapItem> items, int start, int end,
        float x, float y, float w, float h,
        double totalValue, bool horizontal,
        List<(TreemapItem, SKRect)> result)
    {
        if (start >= end) return;
        if (end - start == 1)
        {
            float gap = 1.5f;
            result.Add((items[start], new SKRect(x + gap, y + gap, x + w - gap, y + h - gap)));
            return;
        }

        double rangeValue = 0;
        for (int i = start; i < end; i++) rangeValue += items[i].Value;

        // Split at roughly half the value
        double half = rangeValue / 2.0;
        double running = 0;
        int mid = start;
        for (int i = start; i < end; i++)
        {
            running += items[i].Value;
            if (running >= half) { mid = i + 1; break; }
        }
        if (mid <= start) mid = start + 1;
        if (mid >= end) mid = end - 1;

        double firstHalf = 0;
        for (int i = start; i < mid; i++) firstHalf += items[i].Value;
        float ratio = (float)(firstHalf / rangeValue);

        if (horizontal)
        {
            float splitW = w * ratio;
            SliceAndDice(items, start, mid, x, y, splitW, h, totalValue, !horizontal, result);
            SliceAndDice(items, mid, end, x + splitW, y, w - splitW, h, totalValue, !horizontal, result);
        }
        else
        {
            float splitH = h * ratio;
            SliceAndDice(items, start, mid, x, y, w, splitH, totalValue, !horizontal, result);
            SliceAndDice(items, mid, end, x, y + splitH, w, h - splitH, totalValue, !horizontal, result);
        }
    }
}

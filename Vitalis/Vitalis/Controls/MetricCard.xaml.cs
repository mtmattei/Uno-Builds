using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Vitalis.Models;
using Windows.UI;

namespace Vitalis.Controls;

public sealed partial class MetricCard : UserControl
{
    public MetricCard()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is Metric metric)
        {
            UpdateTrendIndicator(metric.Trend, metric.Status);
        }
    }

    private void UpdateTrendIndicator(TrendDirection trend, HealthStatus status)
    {
        // Status colors: Optimal=Cyan, Warning=Yellow, Critical=Red
        var (color, bgColor) = status switch
        {
            HealthStatus.Optimal => (Color.FromArgb(255, 34, 211, 238), Color.FromArgb(21, 34, 211, 238)),    // Cyan
            HealthStatus.Warning => (Color.FromArgb(255, 234, 179, 8), Color.FromArgb(21, 234, 179, 8)),      // Yellow
            HealthStatus.Critical => (Color.FromArgb(255, 239, 68, 68), Color.FromArgb(21, 239, 68, 68)),     // Red
            _ => (Color.FromArgb(255, 34, 211, 238), Color.FromArgb(21, 34, 211, 238))
        };

        var brush = new SolidColorBrush(color);
        var bgBrush = new SolidColorBrush(bgColor);

        TrendIcon.Foreground = brush;
        TrendText.Foreground = brush;
        ProgressFill.Background = brush;

        // Find and update the trend badge background
        if (TrendIcon.Parent is StackPanel sp && sp.Parent is Border badge)
        {
            badge.Background = bgBrush;
        }

        TrendIcon.Glyph = trend switch
        {
            TrendDirection.Up => "\uE74A",
            TrendDirection.Down => "\uE74B",
            _ => "\uE738"
        };

        TrendText.Text = trend switch
        {
            TrendDirection.Up => "Rising",
            TrendDirection.Down => "Falling",
            _ => "Stable"
        };

        // Update progress fill width based on implied health (inverse)
        var progressWidth = status switch
        {
            HealthStatus.Optimal => 80,
            HealthStatus.Warning => 50,
            HealthStatus.Critical => 25,
            _ => 60
        };
        ProgressFill.Width = progressWidth;
    }
}

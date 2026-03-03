using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Wellmetrix.Models;

namespace Wellmetrix.Controls;

public sealed partial class TrendChart : UserControl
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(IReadOnlyList<TrendDataPoint>), typeof(TrendChart),
            new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(TrendChart),
            new PropertyMetadata(null));

    public IReadOnlyList<TrendDataPoint> Data
    {
        get => (IReadOnlyList<TrendDataPoint>)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public TrendChart()
    {
        this.InitializeComponent();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrendChart control && e.NewValue is IReadOnlyList<TrendDataPoint> data)
        {
            control.UpdateChart(data);
        }
    }

    private void UpdateChart(IReadOnlyList<TrendDataPoint> data)
    {
        BarsContainer.ItemsSource = data;

        // Calculate week-over-week change for display
        if (data.Count >= 7)
        {
            var lastWeekAvg = data.Take(3).Average(d => d.Value);
            var thisWeekAvg = data.Skip(4).Average(d => d.Value);
            var change = ((thisWeekAvg - lastWeekAvg) / lastWeekAvg) * 100;

            var sign = change >= 0 ? "+" : "";
            ChangeText.Text = $"{sign}{change:F1}% vs last week";
            ChangeText.Foreground = change >= 0
                ? (Brush)Application.Current.Resources["KidneysAccentBrush"]
                : (Brush)Application.Current.Resources["HeartAccentBrush"];
        }
    }
}

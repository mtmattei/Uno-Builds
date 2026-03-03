using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Wellmetrix.Models;

namespace Wellmetrix.Controls;

public sealed partial class MetricCard : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(MetricCard),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(MetricCard),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(MetricCard),
            new PropertyMetadata(string.Empty, OnPropertyChanged));

    public static readonly DependencyProperty TrendProperty =
        DependencyProperty.Register(nameof(Trend), typeof(TrendDirection), typeof(MetricCard),
            new PropertyMetadata(TrendDirection.Stable, OnPropertyChanged));

    public static readonly DependencyProperty ChangePercentageProperty =
        DependencyProperty.Register(nameof(ChangePercentage), typeof(double), typeof(MetricCard),
            new PropertyMetadata(0.0, OnPropertyChanged));

    public static readonly DependencyProperty MinRangeProperty =
        DependencyProperty.Register(nameof(MinRange), typeof(double), typeof(MetricCard),
            new PropertyMetadata(0.0, OnPropertyChanged));

    public static readonly DependencyProperty MaxRangeProperty =
        DependencyProperty.Register(nameof(MaxRange), typeof(double), typeof(MetricCard),
            new PropertyMetadata(100.0, OnPropertyChanged));

    public static readonly DependencyProperty SparklineDataProperty =
        DependencyProperty.Register(nameof(SparklineData), typeof(IReadOnlyList<double>), typeof(MetricCard),
            new PropertyMetadata(null, OnSparklineDataChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(MetricCard),
            new PropertyMetadata(null, OnAccentBrushChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public TrendDirection Trend
    {
        get => (TrendDirection)GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
    }

    public double ChangePercentage
    {
        get => (double)GetValue(ChangePercentageProperty);
        set => SetValue(ChangePercentageProperty, value);
    }

    public double MinRange
    {
        get => (double)GetValue(MinRangeProperty);
        set => SetValue(MinRangeProperty, value);
    }

    public double MaxRange
    {
        get => (double)GetValue(MaxRangeProperty);
        set => SetValue(MaxRangeProperty, value);
    }

    public IReadOnlyList<double> SparklineData
    {
        get => (IReadOnlyList<double>)GetValue(SparklineDataProperty);
        set => SetValue(SparklineDataProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public MetricCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateDisplay();
        UpdateSparkline();
        UpdateAccentColor();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MetricCard control)
        {
            control.UpdateDisplay();
        }
    }

    private static void OnSparklineDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MetricCard control)
        {
            control.UpdateSparkline();
        }
    }

    private static void OnAccentBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MetricCard control)
        {
            control.UpdateAccentColor();
        }
    }

    private void UpdateDisplay()
    {
        if (LabelText != null)
            LabelText.Text = Label?.ToUpperInvariant() ?? string.Empty;

        if (ValueText != null)
            ValueText.Text = Value ?? string.Empty;

        if (UnitText != null)
            UnitText.Text = Unit ?? string.Empty;

        if (RangeText != null)
            RangeText.Text = $"Range: {MinRange:0}-{MaxRange:0}";

        if (TrendText != null && TrendBadge != null)
        {
            var percentage = Math.Abs(ChangePercentage);
            string trendSymbol;
            Brush trendColor;

            switch (Trend)
            {
                case TrendDirection.Up:
                    trendSymbol = $"+{percentage:0}%";
                    trendColor = (Brush)Application.Current.Resources["KidneysAccentBrush"];
                    break;
                case TrendDirection.Down:
                    trendSymbol = $"-{percentage:0}%";
                    trendColor = (Brush)Application.Current.Resources["HeartAccentBrush"];
                    break;
                default:
                    trendSymbol = "Stable";
                    trendColor = (Brush)Application.Current.Resources["TextTertiaryBrush"];
                    break;
            }

            TrendText.Text = trendSymbol;
            TrendText.Foreground = trendColor;

            TrendBadge.Visibility = (Trend == TrendDirection.Stable && ChangePercentage == 0)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private void UpdateSparkline()
    {
        if (SparklineChart != null && SparklineData != null)
        {
            SparklineChart.Data = SparklineData;
        }
    }

    private void UpdateAccentColor()
    {
        if (SparklineChart != null && AccentBrush != null)
        {
            SparklineChart.AccentBrush = AccentBrush;
        }
    }
}

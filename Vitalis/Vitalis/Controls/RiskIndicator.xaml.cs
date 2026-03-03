using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace Vitalis.Controls;

public sealed partial class RiskIndicator : UserControl
{
    public static readonly DependencyProperty RiskLevelProperty =
        DependencyProperty.Register(nameof(RiskLevel), typeof(double), typeof(RiskIndicator),
            new PropertyMetadata(0.0, OnRiskLevelChanged));

    public double RiskLevel
    {
        get => (double)GetValue(RiskLevelProperty);
        set => SetValue(RiskLevelProperty, value);
    }

    public RiskIndicator()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateBars();
    }

    private static void OnRiskLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RiskIndicator indicator)
        {
            indicator.UpdateBars();
        }
    }

    private void UpdateBars()
    {
        BarsPanel.Children.Clear();
        var totalBars = 20;
        var filledBars = (int)(RiskLevel * totalBars);
        var limeGreen = Color.FromArgb(255, 163, 230, 53);
        var dimGray = Color.FromArgb(255, 38, 38, 38);

        for (int i = 0; i < totalBars; i++)
        {
            var height = 8 + (i * 0.8); // Progressive height
            var bar = new Rectangle
            {
                Width = 6,
                Height = height,
                RadiusX = 2,
                RadiusY = 2,
                Fill = new SolidColorBrush(i < filledBars ? limeGreen : dimGray),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            BarsPanel.Children.Add(bar);
        }
    }
}

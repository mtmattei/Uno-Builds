using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace Wellmetrix.Controls;

public sealed partial class Sparkline : UserControl
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(IReadOnlyList<double>), typeof(Sparkline),
            new PropertyMetadata(null, OnDataChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(Sparkline),
            new PropertyMetadata(null, OnAccentBrushChanged));

    public IReadOnlyList<double> Data
    {
        get => (IReadOnlyList<double>)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public Sparkline()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateChart();
        UpdateAccentColor();
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Sparkline control)
        {
            control.UpdateChart();
        }
    }

    private static void OnAccentBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Sparkline control)
        {
            control.UpdateAccentColor();
        }
    }

    private void UpdateChart()
    {
        if (Data == null || Data.Count < 2 || SparklinePath == null || SparklineArea == null)
            return;

        var width = 80.0;
        var height = 24.0;
        var padding = 2.0;

        var min = Data.Min();
        var max = Data.Max();
        var range = max - min;
        if (range == 0) range = 1;

        var points = new PointCollection();
        var areaPoints = new PointCollection();

        // Add bottom-left corner for area
        areaPoints.Add(new Point(padding, height - padding));

        for (int i = 0; i < Data.Count; i++)
        {
            var x = padding + (i * (width - 2 * padding) / (Data.Count - 1));
            var normalizedValue = (Data[i] - min) / range;
            var y = height - padding - (normalizedValue * (height - 2 * padding));

            points.Add(new Point(x, y));
            areaPoints.Add(new Point(x, y));
        }

        // Add bottom-right corner for area
        areaPoints.Add(new Point(width - padding, height - padding));

        SparklinePath.Points = points;
        SparklineArea.Points = areaPoints;
    }

    private void UpdateAccentColor()
    {
        if (AccentBrush != null)
        {
            if (SparklinePath != null)
                SparklinePath.Stroke = AccentBrush;
            if (SparklineArea != null)
                SparklineArea.Fill = AccentBrush;
        }
    }
}

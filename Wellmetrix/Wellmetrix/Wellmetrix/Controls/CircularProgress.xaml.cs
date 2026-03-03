using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace Wellmetrix.Controls;

public sealed partial class CircularProgress : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularProgress),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(CircularProgress),
            new PropertyMetadata(null, OnAccentBrushChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public CircularProgress()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateProgress();
        UpdateAccentColor();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularProgress control)
        {
            control.UpdateProgress();
        }
    }

    private static void OnAccentBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularProgress control)
        {
            control.UpdateAccentColor();
        }
    }

    private void UpdateProgress()
    {
        if (ValueText != null)
        {
            ValueText.Text = ((int)Value).ToString();
        }

        if (ArcSegment != null)
        {
            var percentage = Math.Clamp(Value / 100.0, 0, 1);
            var angle = percentage * 360;

            // Calculate end point of arc
            var centerX = 60.0;
            var centerY = 60.0;
            var radius = 50.0;
            var startAngle = -90; // Start from top

            var endAngle = startAngle + angle;
            var endAngleRad = endAngle * Math.PI / 180;

            var endX = centerX + radius * Math.Cos(endAngleRad);
            var endY = centerY + radius * Math.Sin(endAngleRad);

            // Handle full circle case
            if (percentage >= 0.99)
            {
                endX = centerX + radius * Math.Cos((startAngle + 359) * Math.PI / 180);
                endY = centerY + radius * Math.Sin((startAngle + 359) * Math.PI / 180);
            }

            ArcSegment.Point = new Point(endX, endY);
            ArcSegment.IsLargeArc = angle > 180;
        }
    }

    private void UpdateAccentColor()
    {
        if (ProgressArc != null && AccentBrush != null)
        {
            ProgressArc.Stroke = AccentBrush;
        }
    }
}

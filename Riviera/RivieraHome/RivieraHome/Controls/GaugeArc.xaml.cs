using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using Windows.Foundation;
using Windows.UI;

namespace RivieraHome.Controls;

public sealed partial class GaugeArc : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(GaugeArc),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(GaugeArc),
            new PropertyMetadata(100.0, OnValueChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(GaugeArc),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(GaugeArc),
            new PropertyMetadata(string.Empty, OnUnitChanged));

    public static readonly DependencyProperty GaugeSizeProperty =
        DependencyProperty.Register(nameof(GaugeSize), typeof(double), typeof(GaugeArc),
            new PropertyMetadata(105.0, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public double GaugeSize
    {
        get => (double)GetValue(GaugeSizeProperty);
        set => SetValue(GaugeSizeProperty, value);
    }

    public GaugeArc()
    {
        this.InitializeComponent();
        Loaded += (_, _) => DrawArc();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeArc gauge)
        {
            gauge.ValueText.Text = $"{gauge.Value:0}";
            gauge.DrawArc();
        }
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeArc gauge)
            gauge.LabelText.Text = gauge.Label;
    }

    private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeArc gauge)
            gauge.UnitText.Text = gauge.Unit;
    }

    private void DrawArc()
    {
        ArcCanvas.Children.Clear();

        var size = GaugeSize;
        var strokeWidth = 4.0;
        var radius = (size / 2) - strokeWidth - 2;
        var center = new Point(size / 2, size / 2);

        // Arc spans 240 degrees (from 150 to 390)
        const double startAngle = 150;
        const double totalSweep = 240;

        // Background track
        DrawArcPath(center, radius, startAngle, totalSweep, strokeWidth,
            Color.FromArgb(0x33, 0x33, 0xFF, 0x66)); // PhosphorSubtle

        // Value arc
        var fraction = Maximum > 0 ? Math.Clamp(Value / Maximum, 0, 1) : 0;
        var valueSweep = fraction * totalSweep;
        if (valueSweep > 0.5)
        {
            DrawArcPath(center, radius, startAngle, valueSweep, strokeWidth,
                Color.FromArgb(0xFF, 0x33, 0xFF, 0x66)); // PhosphorPrimary
        }
    }

    private void DrawArcPath(Point center, double radius, double startAngle, double sweepAngle, double strokeWidth, Color color)
    {
        var startRad = startAngle * Math.PI / 180;
        var endRad = (startAngle + sweepAngle) * Math.PI / 180;

        var start = new Point(
            center.X + radius * Math.Cos(startRad),
            center.Y + radius * Math.Sin(startRad));

        var end = new Point(
            center.X + radius * Math.Cos(endRad),
            center.Y + radius * Math.Sin(endRad));

        var isLargeArc = sweepAngle > 180;

        var figure = new PathFigure
        {
            StartPoint = start,
            IsClosed = false
        };
        figure.Segments.Add(new ArcSegment
        {
            Point = end,
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = isLargeArc
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        var path = new Microsoft.UI.Xaml.Shapes.Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = strokeWidth
        };

        ArcCanvas.Children.Add(path);
    }
}

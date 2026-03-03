namespace Caffe.Controls;

public sealed partial class ExtractionArc : UserControl
{
    private const int MinTime = 20;
    private const int MaxTime = 35;
    private const int DefaultTime = 27;
    private const double ArcCenterX = 50;
    private const double ArcCenterY = 45;
    private const double ArcRadius = 40;
    private const double ArcStartAngle = -120;
    private const double ArcSweepDegrees = 240;

    public static readonly DependencyProperty ExtractionTimeProperty =
        DependencyProperty.Register(
            nameof(ExtractionTime),
            typeof(int),
            typeof(ExtractionArc),
            new PropertyMetadata(DefaultTime, OnExtractionTimeChanged));

    public int ExtractionTime
    {
        get => (int)GetValue(ExtractionTimeProperty);
        set => SetValue(ExtractionTimeProperty, value);
    }

    public event EventHandler<int>? ExtractionTimeChanged;

    public ExtractionArc()
    {
        this.InitializeComponent();
        UpdateVisual(DefaultTime);
    }

    private static void OnExtractionTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ExtractionArc arc)
        {
            var time = (int)e.NewValue;
            arc.TimeSlider.Value = time;
            arc.UpdateVisual(time);
        }
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var time = (int)e.NewValue;
        ExtractionTime = time;
        UpdateVisual(time);
        ExtractionTimeChanged?.Invoke(this, time);
    }

    private void UpdateVisual(int time)
    {
        ArcValueText.Text = time.ToString();
        ValueText.Text = $"{time}s";

        var percentage = (time - MinTime) / (double)(MaxTime - MinTime);
        var angle = ArcStartAngle + (percentage * ArcSweepDegrees);
        var radians = angle * Math.PI / 180;

        var endX = ArcCenterX + ArcRadius * Math.Sin(radians);
        var endY = ArcCenterY - ArcRadius * Math.Cos(radians);

        ArcSegment.Point = new Windows.Foundation.Point(endX, endY);
        ArcSegment.IsLargeArc = percentage > 0.5;
    }
}

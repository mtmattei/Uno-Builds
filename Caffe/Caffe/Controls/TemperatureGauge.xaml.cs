namespace Caffe.Controls;

public sealed partial class TemperatureGauge : UserControl
{
    private const int MinTemperature = 88;
    private const int MaxTemperature = 96;
    private const int DefaultTemperature = 93;
    private const double MinFillHeight = 10;
    private const double MaxFillHeight = 50;

    public static readonly DependencyProperty TemperatureProperty =
        DependencyProperty.Register(
            nameof(Temperature),
            typeof(int),
            typeof(TemperatureGauge),
            new PropertyMetadata(DefaultTemperature, OnTemperatureChanged));

    public int Temperature
    {
        get => (int)GetValue(TemperatureProperty);
        set => SetValue(TemperatureProperty, value);
    }

    public event EventHandler<int>? TemperatureChanged;

    public TemperatureGauge()
    {
        this.InitializeComponent();
        UpdateVisual(DefaultTemperature);
    }

    private static void OnTemperatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TemperatureGauge gauge)
        {
            var temp = (int)e.NewValue;
            gauge.TempSlider.Value = temp;
            gauge.UpdateVisual(temp);
        }
    }

    private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var temp = (int)e.NewValue;
        Temperature = temp;
        UpdateVisual(temp);
        TemperatureChanged?.Invoke(this, temp);
    }

    private void UpdateVisual(int temp)
    {
        ValueText.Text = $"{temp}°C";

        var percentage = (temp - MinTemperature) / (double)(MaxTemperature - MinTemperature);
        TempFill.Height = MinFillHeight + (percentage * (MaxFillHeight - MinFillHeight));
    }
}

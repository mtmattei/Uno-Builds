namespace Caffe.Controls;

public sealed partial class SelectionOverview : UserControl
{
    public static readonly DependencyProperty EspressoNameProperty =
        DependencyProperty.Register(nameof(EspressoName), typeof(string), typeof(SelectionOverview),
            new PropertyMetadata("Espresso", OnEspressoNameChanged));

    public static readonly DependencyProperty TemperatureProperty =
        DependencyProperty.Register(nameof(Temperature), typeof(int), typeof(SelectionOverview),
            new PropertyMetadata(93, OnTemperatureChanged));

    public static readonly DependencyProperty GrindAbbreviationProperty =
        DependencyProperty.Register(nameof(GrindAbbreviation), typeof(string), typeof(SelectionOverview),
            new PropertyMetadata("F", OnGrindChanged));

    public static readonly DependencyProperty ExtractionTimeProperty =
        DependencyProperty.Register(nameof(ExtractionTime), typeof(int), typeof(SelectionOverview),
            new PropertyMetadata(27, OnTimeChanged));

    public string EspressoName
    {
        get => (string)GetValue(EspressoNameProperty);
        set => SetValue(EspressoNameProperty, value);
    }

    public int Temperature
    {
        get => (int)GetValue(TemperatureProperty);
        set => SetValue(TemperatureProperty, value);
    }

    public string GrindAbbreviation
    {
        get => (string)GetValue(GrindAbbreviationProperty);
        set => SetValue(GrindAbbreviationProperty, value);
    }

    public int ExtractionTime
    {
        get => (int)GetValue(ExtractionTimeProperty);
        set => SetValue(ExtractionTimeProperty, value);
    }

    public SelectionOverview()
    {
        this.InitializeComponent();
    }

    private static void OnEspressoNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectionOverview overview)
            overview.EspressoNameText.Text = (string)e.NewValue;
    }

    private static void OnTemperatureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectionOverview overview)
            overview.TempValueText.Text = $"{(int)e.NewValue}°";
    }

    private static void OnGrindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectionOverview overview)
            overview.GrindValueText.Text = (string)e.NewValue;
    }

    private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectionOverview overview)
            overview.TimeValueText.Text = $"{(int)e.NewValue}s";
    }
}

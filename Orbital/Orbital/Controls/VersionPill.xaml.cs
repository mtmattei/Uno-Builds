namespace Orbital.Controls;

public sealed partial class VersionPill : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(VersionPill),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(VersionPill),
            new PropertyMetadata(string.Empty, OnValueChanged));

    public VersionPill()
    {
        this.InitializeComponent();
    }

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

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VersionPill pill)
            pill.LabelText.Text = ((string)e.NewValue).ToUpperInvariant();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VersionPill pill)
            pill.ValueText.Text = (string)e.NewValue;
    }
}

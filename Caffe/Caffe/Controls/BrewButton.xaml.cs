namespace Caffe.Controls;

public sealed partial class BrewButton : UserControl
{
    private readonly SolidColorBrush _enabledBrush;
    private readonly SolidColorBrush _disabledBrush;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(BrewButton),
            new PropertyMetadata("Select your espresso", OnTextChanged));

    public static readonly DependencyProperty IsBrewEnabledProperty =
        DependencyProperty.Register(nameof(IsBrewEnabled), typeof(bool), typeof(BrewButton),
            new PropertyMetadata(false, OnIsBrewEnabledChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsBrewEnabled
    {
        get => (bool)GetValue(IsBrewEnabledProperty);
        set => SetValue(IsBrewEnabledProperty, value);
    }

    public event EventHandler? BrewRequested;

    public BrewButton()
    {
        this.InitializeComponent();

        _enabledBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"];
        _disabledBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryDisabledBrush"];

        UpdateVisual(false);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrewButton button)
        {
            var text = (string)e.NewValue;
            button.ButtonText.Text = text;
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, text);
        }
    }

    private static void OnIsBrewEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BrewButton button)
            button.UpdateVisual((bool)e.NewValue);
    }

    private void UpdateVisual(bool isEnabled)
    {
        MainButton.Background = isEnabled ? _enabledBrush : _disabledBrush;
        MainButton.IsEnabled = isEnabled;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (IsBrewEnabled)
            BrewRequested?.Invoke(this, EventArgs.Empty);
    }
}

namespace PhosphorProtocol.Controls;

public enum GCCButtonSize
{
    Standard,
    Tab,
    Large
}

public sealed partial class GCCButton : UserControl
{
    // Color constants matching the phosphor luminance ramp
    private static readonly Windows.UI.Color GhostColor = Windows.UI.Color.FromArgb(255, 10, 34, 34);
    private static readonly Windows.UI.Color DimColor = Windows.UI.Color.FromArgb(255, 20, 56, 56);
    private static readonly Windows.UI.Color GlowColor = Windows.UI.Color.FromArgb(255, 36, 112, 112);
    private static readonly Windows.UI.Color BrightColor = Windows.UI.Color.FromArgb(255, 58, 171, 166);
    private static readonly Windows.UI.Color PeakColor = Windows.UI.Color.FromArgb(255, 111, 252, 246);
    private static readonly Windows.UI.Color CRTColor = Windows.UI.Color.FromArgb(255, 2, 7, 7);
    private static readonly Windows.UI.Color PeakAt4Percent = Windows.UI.Color.FromArgb(10, 111, 252, 246);

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(GCCButton),
            new PropertyMetadata("", OnLabelChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(GCCButton),
            new PropertyMetadata(false, OnIsActiveChanged));

    public static readonly DependencyProperty ButtonSizeProperty =
        DependencyProperty.Register(nameof(ButtonSize), typeof(GCCButtonSize), typeof(GCCButton),
            new PropertyMetadata(GCCButtonSize.Standard));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(System.Windows.Input.ICommand), typeof(GCCButton),
            new PropertyMetadata(null));

    public event RoutedEventHandler? Click;

    public GCCButton()
    {
        this.InitializeComponent();
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        Tapped += OnTapped;
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public GCCButtonSize ButtonSize
    {
        get => (GCCButtonSize)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    public System.Windows.Input.ICommand? Command
    {
        get => (System.Windows.Input.ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GCCButton btn)
            btn.LabelText.Text = (string)e.NewValue;
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GCCButton btn)
        {
            if ((bool)e.NewValue)
                btn.ApplyActiveState();
            else
                btn.ApplyRestState();
        }
    }

    private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!IsActive) ApplyHoverState();
    }

    private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!IsActive) ApplyRestState();
    }

    private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ApplyPressedState();
    }

    private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (IsActive)
            ApplyActiveState();
        else
            ApplyRestState();

        Click?.Invoke(this, new RoutedEventArgs());
        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }

    private void OnTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        // Tapped fires after PointerReleased; avoid double-invoke by only raising
        // Click here if it wasn't already raised by PointerReleased on non-touch input.
    }

    private void ApplyRestState()
    {
        OuterBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        OuterBorder.BorderBrush = new SolidColorBrush(GhostColor);
        LabelText.Foreground = new SolidColorBrush(DimColor);
    }

    private void ApplyHoverState()
    {
        OuterBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        OuterBorder.BorderBrush = new SolidColorBrush(GlowColor);
        LabelText.Foreground = new SolidColorBrush(BrightColor);
    }

    private void ApplyActiveState()
    {
        OuterBorder.Background = new SolidColorBrush(PeakAt4Percent);
        OuterBorder.BorderBrush = new SolidColorBrush(BrightColor);
        LabelText.Foreground = new SolidColorBrush(PeakColor);
    }

    private void ApplyPressedState()
    {
        OuterBorder.Background = new SolidColorBrush(GhostColor);
        OuterBorder.BorderBrush = new SolidColorBrush(GlowColor);
        LabelText.Foreground = new SolidColorBrush(BrightColor);
    }
}

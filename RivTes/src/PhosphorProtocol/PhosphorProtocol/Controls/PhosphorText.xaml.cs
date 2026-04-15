namespace PhosphorProtocol.Controls;

public enum PhosphorLevel
{
    Ghost,
    Dim,
    Glow,
    Bright,
    Peak,
    Hot
}

public sealed partial class PhosphorText : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(PhosphorText),
            new PropertyMetadata("", OnTextChanged));

    public static readonly DependencyProperty GlowSpreadProperty =
        DependencyProperty.Register(nameof(GlowSpread), typeof(double), typeof(PhosphorText),
            new PropertyMetadata(0.0, OnGlowChanged));

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(PhosphorLevel), typeof(PhosphorText),
            new PropertyMetadata(PhosphorLevel.Peak, OnLevelChanged));

    public static readonly DependencyProperty TextStyleProperty =
        DependencyProperty.Register(nameof(TextStyle), typeof(Style), typeof(PhosphorText),
            new PropertyMetadata(null, OnTextStyleChanged));

    public PhosphorText()
    {
        this.InitializeComponent();
        Loaded += (_, _) => UpdateColors();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double GlowSpread
    {
        get => (double)GetValue(GlowSpreadProperty);
        set => SetValue(GlowSpreadProperty, value);
    }

    public PhosphorLevel Level
    {
        get => (PhosphorLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public Style? TextStyle
    {
        get => (Style?)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorText pt)
        {
            pt.MainText.Text = (string)e.NewValue;
            pt.GlowText.Text = (string)e.NewValue;
        }
    }

    private static void OnGlowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorText pt)
        {
            var spread = (double)e.NewValue;
            pt.GlowText.Visibility = spread > 0 ? Visibility.Visible : Visibility.Collapsed;
            pt.GlowText.Opacity = spread > 0 ? 0.4 : 0;
        }
    }

    private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorText pt) pt.UpdateColors();
    }

    private static void OnTextStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorText pt && e.NewValue is Style style)
        {
            pt.MainText.Style = style;
            pt.GlowText.Style = style;
        }
    }

    private void UpdateColors()
    {
        var color = Level switch
        {
            PhosphorLevel.Ghost => Windows.UI.Color.FromArgb(255, 10, 34, 34),
            PhosphorLevel.Dim => Windows.UI.Color.FromArgb(255, 20, 56, 56),
            PhosphorLevel.Glow => Windows.UI.Color.FromArgb(255, 36, 112, 112),
            PhosphorLevel.Bright => Windows.UI.Color.FromArgb(255, 58, 171, 166),
            PhosphorLevel.Peak => Windows.UI.Color.FromArgb(255, 111, 252, 246),
            PhosphorLevel.Hot => Windows.UI.Color.FromArgb(255, 158, 255, 250),
            _ => Windows.UI.Color.FromArgb(255, 111, 252, 246)
        };

        var brush = new SolidColorBrush(color);
        MainText.Foreground = brush;
        GlowText.Foreground = brush;
    }
}

namespace ClaudeDash.Controls;

public sealed partial class SectionHeader : UserControl
{
    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(SectionHeader),
            new PropertyMetadata("", OnIconChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SectionHeader),
            new PropertyMetadata("", OnTitleChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(SectionHeader),
            new PropertyMetadata("", OnSubtitleChanged));

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public SectionHeader()
    {
        this.InitializeComponent();
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // IconGlyph is now used as a text bullet character (e.g., "◈", "$", "▸", "◇")
        if (d is SectionHeader h) h.BulletText.Text = e.NewValue as string ?? "";
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SectionHeader h) h.TitleText.Text = e.NewValue as string ?? "";
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SectionHeader h) h.SubtitleText.Text = e.NewValue as string ?? "";
    }
}

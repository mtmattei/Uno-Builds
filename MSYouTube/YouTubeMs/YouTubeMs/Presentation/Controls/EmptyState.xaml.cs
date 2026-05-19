namespace YouTubeMs.Presentation.Controls;

public sealed partial class EmptyState : UserControl
{
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(EmptyState), new PropertyMetadata(""));
    public static readonly DependencyProperty HeadlineProperty = DependencyProperty.Register(
        nameof(Headline), typeof(string), typeof(EmptyState), new PropertyMetadata("Nothing to show yet"));
    public static readonly DependencyProperty HelperProperty = DependencyProperty.Register(
        nameof(Helper), typeof(string), typeof(EmptyState), new PropertyMetadata(""));
    public static readonly DependencyProperty CtaLabelProperty = DependencyProperty.Register(
        nameof(CtaLabel), typeof(string), typeof(EmptyState), new PropertyMetadata("", OnCtaLabelChanged));
    public static readonly DependencyProperty CtaVisibilityProperty = DependencyProperty.Register(
        nameof(CtaVisibility), typeof(Visibility), typeof(EmptyState), new PropertyMetadata(Visibility.Collapsed));

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public string Headline
    {
        get => (string)GetValue(HeadlineProperty);
        set => SetValue(HeadlineProperty, value);
    }

    public string Helper
    {
        get => (string)GetValue(HelperProperty);
        set => SetValue(HelperProperty, value);
    }

    public string CtaLabel
    {
        get => (string)GetValue(CtaLabelProperty);
        set => SetValue(CtaLabelProperty, value);
    }

    public Visibility CtaVisibility
    {
        get => (Visibility)GetValue(CtaVisibilityProperty);
        set => SetValue(CtaVisibilityProperty, value);
    }

    private static void OnCtaLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EmptyState es)
            es.CtaVisibility = string.IsNullOrEmpty(e.NewValue as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public EmptyState()
    {
        this.InitializeComponent();
    }
}

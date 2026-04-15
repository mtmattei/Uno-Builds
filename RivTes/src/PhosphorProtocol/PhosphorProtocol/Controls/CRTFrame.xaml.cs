namespace PhosphorProtocol.Controls;

public sealed partial class CRTFrame : UserControl
{
    public static readonly DependencyProperty FrameContentProperty =
        DependencyProperty.Register(
            nameof(FrameContent),
            typeof(UIElement),
            typeof(CRTFrame),
            new PropertyMetadata(null, OnFrameContentChanged));

    public CRTFrame()
    {
        this.InitializeComponent();
    }

    public UIElement? FrameContent
    {
        get => (UIElement?)GetValue(FrameContentProperty);
        set => SetValue(FrameContentProperty, value);
    }

    private static void OnFrameContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CRTFrame frame)
        {
            frame.InnerContent.Content = e.NewValue;
        }
    }
}

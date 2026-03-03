namespace FormaEspresso.Controls;

public sealed partial class SuccessIcon : UserControl
{
    public SuccessIcon()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var pulseAnimation = Resources["PulseAnimation"] as Storyboard;
        var floatAnimation = Resources["FloatAnimation"] as Storyboard;
        var checkmarkAnimation = Resources["CheckmarkAnimation"] as Storyboard;

        pulseAnimation?.Begin();
        floatAnimation?.Begin();
        checkmarkAnimation?.Begin();
    }
}

namespace Nexus.Presentation;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private bool _hoverAttached;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_hoverAttached)
        {
            LineItemHoverBehavior.AttachToTree(this);
            _hoverAttached = true;
        }
    }
}

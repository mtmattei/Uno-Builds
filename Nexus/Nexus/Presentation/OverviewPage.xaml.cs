namespace Nexus.Presentation;

public sealed partial class OverviewPage : Page
{
    public OverviewModel ViewModel { get; private set; } = new();

    public OverviewPage()
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

    public void ReplayChartAnimations()
    {
        ViewModel = new OverviewModel();
        Bindings.Update();
    }
}

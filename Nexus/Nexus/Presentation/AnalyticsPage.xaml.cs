namespace Nexus.Presentation;

public sealed partial class AnalyticsPage : Page
{
    public AnalyticsModel ViewModel { get; private set; } = new();

    public AnalyticsPage()
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
        ViewModel = new AnalyticsModel();
        Bindings.Update();
    }
}

namespace PhosphorProtocol.Views;

public sealed partial class DashboardShell : Page
{
    private readonly DispatcherTimer _clockTimer;

    public DashboardShell()
    {
        this.InitializeComponent();

        LiveClock.Text = DateTime.Now.ToString("h:mm tt");

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => LiveClock.Text = DateTime.Now.ToString("h:mm tt");
        _clockTimer.Start();

        Unloaded += (_, _) => _clockTimer.Stop();
    }

    private void OnAutopilotClick(object sender, RoutedEventArgs e)
    {
        // Select the A.I. tab (last item) which triggers region navigation
        MainTabBar.SelectedItem = MainTabBar.Items[^1];
    }

    public void NavigateToNav()
    {
        // Select the Nav tab (first item) which triggers region navigation back
        MainTabBar.SelectedItem = MainTabBar.Items[0];
    }
}

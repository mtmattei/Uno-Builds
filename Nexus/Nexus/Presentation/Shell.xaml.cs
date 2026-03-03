using Microsoft.UI.Dispatching;

namespace Nexus.Presentation;

public sealed partial class Shell : UserControl
{
    private DispatcherTimer? _clockTimer;
    private readonly Page[] _pages;

    public Shell()
    {
        this.InitializeComponent();
        this.Loaded += Shell_Loaded;

        // Cache page instances
        _pages = new Page[]
        {
            new OverviewPage(),
            new ProductionPage(),
            new AnalyticsPage(),
            new MaintenancePage(),
            new SettingsPage()
        };

        ContentArea.Content = _pages[0];
        MainTabBar.SelectedIndex = 0;

        // Attach hover events with lambda captures
        AttachHoverEvents();
    }

    private void AttachHoverEvents()
    {
        OverviewTab.PointerEntered += (s, e) => OverviewHoverIn.Begin();
        OverviewTab.PointerExited += (s, e) => OverviewHoverOut.Begin();
        ProductionTab.PointerEntered += (s, e) => ProductionHoverIn.Begin();
        ProductionTab.PointerExited += (s, e) => ProductionHoverOut.Begin();
        AnalyticsTab.PointerEntered += (s, e) => AnalyticsHoverIn.Begin();
        AnalyticsTab.PointerExited += (s, e) => AnalyticsHoverOut.Begin();
        MaintenanceTab.PointerEntered += (s, e) => MaintenanceHoverIn.Begin();
        MaintenanceTab.PointerExited += (s, e) => MaintenanceHoverOut.Begin();
        SettingsTab.PointerEntered += (s, e) => SettingsHoverIn.Begin();
        SettingsTab.PointerExited += (s, e) => SettingsHoverOut.Begin();
    }

    private void Shell_Loaded(object sender, RoutedEventArgs e)
    {
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (s, args) => ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        _clockTimer.Start();
    }

    private void MainTabBar_SelectionChanged(object sender, Uno.Toolkit.UI.TabBarSelectionChangedEventArgs e)
    {
        var index = MainTabBar.SelectedIndex;
        if (index >= 0 && index < _pages.Length)
        {
            ContentArea.Content = _pages[index];

            // Replay chart animations when revisiting a tab
            if (_pages[index] is OverviewPage overview)
                overview.ReplayChartAnimations();
            else if (_pages[index] is AnalyticsPage analytics)
                analytics.ReplayChartAnimations();
        }
    }
}

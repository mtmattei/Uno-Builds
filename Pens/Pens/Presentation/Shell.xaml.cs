namespace Pens.Presentation;

public sealed partial class Shell : Page
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<string, Func<Page>> _pageFactories;

    public Shell(IServiceProvider services)
    {
        _services = services;
        this.InitializeComponent();

        _pageFactories = new Dictionary<string, Func<Page>>
        {
            ["Schedule"] = () => new SchedulePage { DataContext = _services.GetRequiredService<ScheduleViewModel>() },
            ["Chat"] = () => new ChatPage { DataContext = _services.GetRequiredService<ChatViewModel>() },
            ["Beers"] = () => new BeersPage { DataContext = _services.GetRequiredService<BeersViewModel>() },
            ["Duties"] = () => new DutiesPage { DataContext = _services.GetRequiredService<DutiesViewModel>() },
            ["Roster"] = () => new RosterPage { DataContext = _services.GetRequiredService<RosterViewModel>() }
        };

        TabBar.SelectionChanged += OnTabSelectionChanged;

        // Load default page
        this.Loaded += (s, e) => NavigateToTab("Schedule");
    }

    private void OnTabSelectionChanged(object sender, Uno.Toolkit.UI.TabBarSelectionChangedEventArgs e)
    {
        if (e.NewItem is Uno.Toolkit.UI.TabBarItem tab)
        {
            var tabName = tab.Tag?.ToString();
            if (!string.IsNullOrEmpty(tabName))
            {
                NavigateToTab(tabName);
            }
        }
    }

    private void NavigateToTab(string tabName)
    {
        if (_pageFactories.TryGetValue(tabName, out var factory))
        {
            try
            {
                NavigationContent.Content = factory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error for {tabName}: {ex}");
            }
        }
    }
}

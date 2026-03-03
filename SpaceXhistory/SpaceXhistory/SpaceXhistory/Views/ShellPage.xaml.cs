using Uno.Toolkit.UI;

namespace SpaceXhistory.Views;

public sealed partial class ShellPage : Page
{
    private readonly Type[] _pageTypes = new[]
    {
        typeof(HomePage),
        typeof(UpcomingLaunchesPage),
        typeof(PastLaunchesPage)
    };

    public ShellPage()
    {
        this.InitializeComponent();

        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Navigate to the first page
        ContentFrame.Navigate(typeof(HomePage));
    }

    private void OnTabSelectionChanged(TabBar sender, TabBarSelectionChangedEventArgs args)
    {
        var selectedIndex = BottomTabBar.SelectedIndex;

        if (selectedIndex >= 0 && selectedIndex < _pageTypes.Length)
        {
            ContentFrame.Navigate(_pageTypes[selectedIndex]);
        }
    }
}

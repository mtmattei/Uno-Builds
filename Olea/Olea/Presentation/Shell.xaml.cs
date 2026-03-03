namespace Olea.Presentation;

public sealed partial class Shell : UserControl
{
    public Shell()
    {
        this.InitializeComponent();
    }

    private void SwitchToTab(int tabIndex)
    {
        // Update tab pill styles
        SetTabActive(TabNewTasting, tabIndex == 0);
        SetTabActive(TabJournal, tabIndex == 1);
        SetTabActive(TabExplore, tabIndex == 2);

        // Update nav link styles
        NavNewTasting.Foreground = tabIndex == 0
            ? (Microsoft.UI.Xaml.Media.Brush)Resources["OleaPrimaryBrush"]
                ?? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45))
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96));
        NavJournal.Foreground = tabIndex == 1
            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45))
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96));
        NavExplore.Foreground = tabIndex == 2
            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 45, 59, 45))
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 107, 101, 96));

        // Update panels
        NewTastingPanel.Visibility = tabIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        JournalPanel.Visibility = tabIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        ExplorePanel.Visibility = tabIndex == 2 ? Visibility.Visible : Visibility.Collapsed;

        // Update page header
        switch (tabIndex)
        {
            case 0:
                PageTitle.Text = "Record a Tasting";
                PageSubtitle.Text = "Capture every nuance of your olive oil experience";
                break;
            case 1:
                PageTitle.Text = "Your Journal";
                PageSubtitle.Text = "4 tastings recorded";
                break;
            case 2:
                PageTitle.Text = "Explore Regions";
                PageSubtitle.Text = "Discover the world's great olive oil terroirs";
                break;
        }
    }

    private void SetTabActive(Button tab, bool active)
    {
        if (active)
        {
            tab.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 45, 59, 45));
            tab.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 250, 246, 238));
            tab.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 45, 59, 45));
        }
        else
        {
            tab.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(0, 0, 0, 0));
            tab.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 107, 101, 96));
            tab.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 229, 221, 208));
        }
    }

    private void OnTabNewTastingClick(object sender, RoutedEventArgs e) => SwitchToTab(0);
    private void OnTabJournalClick(object sender, RoutedEventArgs e) => SwitchToTab(1);
    private void OnTabExploreClick(object sender, RoutedEventArgs e) => SwitchToTab(2);
    private void OnNavNewTastingTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SwitchToTab(0);
    private void OnNavJournalTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SwitchToTab(1);
    private void OnNavExploreTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SwitchToTab(2);
}

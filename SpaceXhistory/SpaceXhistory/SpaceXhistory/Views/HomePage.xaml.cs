using SpaceXhistory.ViewModels;

namespace SpaceXhistory.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;
        Unloaded -= OnPageUnloaded;
    }

    private HomePageViewModel? ViewModel => DataContext as HomePageViewModel;

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } vm)
        {
            await vm.GetNextLaunchAsync();
            await vm.GetLatestLaunchAsync();
            await vm.GetRoadsterInfoAsync();
        }
    }

    private async void OnNextLaunchTapped(object sender, RoutedEventArgs e)
    {
        await OpenUrl(ViewModel?.NextLaunch?.links?.webcast);
    }

    private async void OnLatestLaunchTapped(object sender, RoutedEventArgs e)
    {
        await OpenUrl(ViewModel?.LatestLaunch?.links?.webcast);
    }

    private async void OnRoadsterVideoTapped(object sender, RoutedEventArgs e)
    {
        await OpenUrl(ViewModel?.RoadsterInfo?.video);
    }

    private async void OnRoadsterWikiTapped(object sender, RoutedEventArgs e)
    {
        await OpenUrl(ViewModel?.RoadsterInfo?.wikipedia);
    }

    private async Task OpenUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            var dialog = new ContentDialog
            {
                Title = "Unavailable",
                Content = "This link is not available.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        try
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }
        catch (Exception)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = "Could not open the link.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}

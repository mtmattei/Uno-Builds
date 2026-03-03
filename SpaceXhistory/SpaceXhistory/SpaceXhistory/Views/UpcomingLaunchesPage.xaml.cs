using SpaceXhistory.Models;
using SpaceXhistory.ViewModels;

namespace SpaceXhistory.Views;

public sealed partial class UpcomingLaunchesPage : Page
{
    public UpcomingLaunchesPage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private UpcomingLaunchesViewModel? ViewModel => DataContext as UpcomingLaunchesViewModel;

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } vm)
        {
            await vm.PopulateNextLaunchesAsync();
        }
    }

    private async void OnLaunchItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Root launch)
        {
            var webcastUrl = launch.links?.webcast;

            if (string.IsNullOrEmpty(webcastUrl))
            {
                var dialog = new ContentDialog
                {
                    Title = "Unavailable",
                    Content = "Video for this launch is not available yet.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(webcastUrl));
            }
            catch (Exception)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Could not open the browser.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}

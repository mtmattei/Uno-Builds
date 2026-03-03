using SpaceXhistory.Models;
using SpaceXhistory.ViewModels;

namespace SpaceXhistory.Views;

public sealed partial class PastLaunchesPage : Page
{
    public PastLaunchesPage()
    {
        this.InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private PastLaunchesViewModel? ViewModel => DataContext as PastLaunchesViewModel;

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is { } vm)
        {
            await vm.PopulateLatestLaunchesAsync();
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
                    Content = "Video for this launch is not available.",
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

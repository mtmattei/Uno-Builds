using Microsoft.UI.Xaml.Input;

namespace InfiniteImage;

public sealed partial class LandingPage : Page
{
    public LandingPage()
    {
        this.InitializeComponent();
    }

    private void OnBeginClick(object sender, RoutedEventArgs e)
    {
        // Navigate to main page with random mode
        Frame.Navigate(typeof(MainPage), "random");
    }

    private async void OnUploadClick(object sender, RoutedEventArgs e)
    {
        // Navigate to main page with library mode
        Frame.Navigate(typeof(MainPage), "library");
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        base.OnKeyDown(e);

        // Press R to enter random mode
        if (e.Key == Windows.System.VirtualKey.R)
        {
            Frame.Navigate(typeof(MainPage), "random");
            e.Handled = true;
        }
    }
}

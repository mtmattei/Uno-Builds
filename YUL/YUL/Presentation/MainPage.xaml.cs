using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace YUL.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void OnSendNotificationClick(object sender, RoutedEventArgs e)
    {
        BoardingPassNotification.Visibility = Visibility.Visible;
        SlideInStoryboard.Begin();
    }
}

namespace HockeyBarn.Presentation;

public sealed partial class PlayerProfilePage : Page
{
    public PlayerProfilePage()
    {
        this.InitializeComponent();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}

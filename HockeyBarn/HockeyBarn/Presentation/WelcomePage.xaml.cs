namespace HockeyBarn.Presentation;

public sealed partial class WelcomePage : Page
{
    public WelcomePage()
    {
        this.InitializeComponent();
    }

    private void GetStarted_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SignUpPage));
    }

    private void LogIn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SignUpPage)); // For now, same page with Login tab
    }
}

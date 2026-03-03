namespace HockeyBarn.Presentation;

public sealed partial class SignUpPage : Page
{
    private bool _isSignUpMode = true;

    public SignUpPage()
    {
        this.InitializeComponent();
    }

    private void SignUpTab_Click(object sender, RoutedEventArgs e)
    {
        _isSignUpMode = true;
        SignUpForm.Visibility = Visibility.Visible;
        LoginForm.Visibility = Visibility.Collapsed;
        
        SignUpTabButton.Background = (SolidColorBrush)Application.Current.Resources["PrimaryContainerColor"];
        LoginTabButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        
        FooterText.Text = "Already have an account?";
        FooterLink.Content = "Log In";
    }

    private void LoginTab_Click(object sender, RoutedEventArgs e)
    {
        _isSignUpMode = false;
        SignUpForm.Visibility = Visibility.Collapsed;
        LoginForm.Visibility = Visibility.Visible;
        
        LoginTabButton.Background = (SolidColorBrush)Application.Current.Resources["PrimaryContainerColor"];
        SignUpTabButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        
        FooterText.Text = "Don't have an account?";
        FooterLink.Content = "Sign Up";
    }

    private void FooterLink_Click(object sender, RoutedEventArgs e)
    {
        if (_isSignUpMode)
        {
            LoginTab_Click(sender, e);
        }
        else
        {
            SignUpTab_Click(sender, e);
        }
    }

    private void CreateAccount_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to game listing
        Frame.Navigate(typeof(GameListingPage));
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to game listing
        Frame.Navigate(typeof(GameListingPage));
    }
}

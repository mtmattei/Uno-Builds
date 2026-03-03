namespace HockeyBarn.Presentation;

using HockeyBarn.Models;

public sealed partial class NotificationsPage : Page
{
    public List<Notification> Notifications { get; } = new()
    {
        new Notification
        {
            Id = "1",
            Title = "Game Invite from The Yetis",
            Message = "Tomorrow, 8:00 PM",
            Type = NotificationType.GameInvite,
            Timestamp = DateTime.Now.AddHours(-2),
            IsRead = false
        },
        new Notification
        {
            Id = "2",
            Title = "You're confirmed for The Hawks",
            Message = "Today, 9:00 PM",
            Type = NotificationType.Confirmation,
            Timestamp = DateTime.Now.AddHours(-2),
            IsRead = true
        }
    };

    public NotificationsPage()
    {
        this.InitializeComponent();
        NotificationsList.ItemsSource = Notifications;
    }

    private void NotificationsTab_Click(object sender, RoutedEventArgs e)
    {
        // Update UI to show notifications selected
    }

    private void ChatTab_Click(object sender, RoutedEventArgs e)
    {
        // Update UI to show chat selected
    }

    private void NavigateToHome_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(WelcomePage));
    }

    private void NavigateToFind_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GameListingPage));
    }

    private void NavigateToProfile_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PlayerProfilePage));
    }
}

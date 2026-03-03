namespace HockeyBarn.Presentation;

using HockeyBarn.Models;

public sealed partial class GameListingPage : Page
{
    public List<Game> Games { get; } = new()
    {
        new Game
        {
            Id = "1",
            Name = "Wednesday Night Shinny",
            Location = "Oakridge Arena",
            DateTime = new DateTime(2025, 11, 20, 21, 0, 0),
            TeamLogoUrl = "https://picsum.photos/100/100?random=1",
            SkatersNeeded = 2,
            GoaliesNeeded = 1,
            SkillLevel = SkillLevel.Intermediate,
            DistanceKm = 5.2,
            GameType = "5v5 Scrimmage",
            DressingRoom = "Room #4",
            JerseyColorDark = "Dark",
            JerseyColorLight = "Light",
            CostPerPlayer = 20.00m
        },
        new Game
        {
            Id = "2",
            Name = "Friday Night Hockey",
            Location = "North York Rink",
            DateTime = new DateTime(2025, 11, 23, 20, 30, 0),
            TeamLogoUrl = "https://picsum.photos/100/100?random=2",
            SkatersNeeded = 1,
            GoaliesNeeded = 0,
            SkillLevel = SkillLevel.Beginner,
            DistanceKm = 8.1,
            GameType = "3v3",
            DressingRoom = "Room #2",
            JerseyColorDark = "Dark",
            JerseyColorLight = "Light",
            CostPerPlayer = 15.00m
        },
        new Game
        {
            Id = "3",
            Name = "Saturday Late Night",
            Location = "Ice Palace Center",
            DateTime = new DateTime(2025, 11, 24, 22, 0, 0),
            TeamLogoUrl = "https://picsum.photos/100/100?random=3",
            SkatersNeeded = 0,
            GoaliesNeeded = 1,
            SkillLevel = SkillLevel.Advanced,
            DistanceKm = 12.5,
            GameType = "Full Game",
            DressingRoom = "Room #1",
            JerseyColorDark = "Dark",
            JerseyColorLight = "Light",
            CostPerPlayer = 25.00m
        }
    };

    public GameListingPage()
    {
        this.InitializeComponent();
        GamesList.ItemsSource = Games;
    }

    private void JoinGame_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string gameId)
        {
            var game = Games.FirstOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                Frame.Navigate(typeof(GameDetailsPage), game);
            }
        }
    }

    private void Notifications_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(NotificationsPage));
    }

    private void NavigateToFindGame_Click(object sender, RoutedEventArgs e)
    {
        // Already on this page
    }

    private void NavigateToProfile_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PlayerProfilePage));
    }
}

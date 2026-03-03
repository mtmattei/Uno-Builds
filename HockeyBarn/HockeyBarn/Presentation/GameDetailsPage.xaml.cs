namespace HockeyBarn.Presentation;

using HockeyBarn.Models;

public sealed partial class GameDetailsPage : Page
{
    public Game? Game { get; private set; }

    public GameDetailsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Game game)
        {
            Game = game;
            LoadGameDetails();
        }
    }

    private void LoadGameDetails()
    {
        if (Game == null) return;

        GameTitle.Text = Game.Name;
        LocationText.Text = Game.Location;
        GameTypeText.Text = Game.GameType;
        SkillLevelText.Text = Game.SkillLevel.ToString();
        DressingRoomText.Text = Game.DressingRoom;
        CostText.Text = $"${Game.CostPerPlayer:F2}";
        
        // Add sample roster if empty
        if (Game.Roster.Count == 0)
        {
            Game = Game with
            {
                Roster = new List<RosterPlayer>
                {
                    new RosterPlayer
                    {
                        Id = "1",
                        Name = "Jackson P.",
                        AvatarUrl = "https://picsum.photos/80/80?random=30",
                        Position = Position.Goalie,
                        PaymentStatus = PaymentStatus.Unpaid
                    },
                    new RosterPlayer
                    {
                        Id = "2",
                        Name = "Ava C.",
                        AvatarUrl = "https://picsum.photos/80/80?random=31",
                        Position = Position.Forward,
                        PaymentStatus = PaymentStatus.Paid
                    },
                    new RosterPlayer
                    {
                        Id = "3",
                        Name = "Defense",
                        AvatarUrl = "https://picsum.photos/80/80?random=32",
                        Position = Position.Defense,
                        PaymentStatus = PaymentStatus.Paid
                    }
                }
            };
        }
        
        RosterList.ItemsSource = Game.Roster;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void RequestToJoin_Click(object sender, RoutedEventArgs e)
    {
        // Show confirmation dialog or navigate back
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}

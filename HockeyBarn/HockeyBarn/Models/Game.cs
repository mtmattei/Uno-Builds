namespace HockeyBarn.Models;

public partial record Game
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime DateTime { get; init; }
    public string TeamLogoUrl { get; init; } = string.Empty;
    public int SkatersNeeded { get; init; }
    public int GoaliesNeeded { get; init; }
    public SkillLevel SkillLevel { get; init; }
    public double DistanceKm { get; init; }
    public string GameType { get; init; } = string.Empty;
    public string DressingRoom { get; init; } = string.Empty;
    public string JerseyColorDark { get; init; } = string.Empty;
    public string JerseyColorLight { get; init; } = string.Empty;
    public decimal CostPerPlayer { get; init; }
    public List<RosterPlayer> Roster { get; init; } = new();
}

public partial record RosterPlayer
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
    public Position Position { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
}

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum Position
{
    Forward,
    Defense,
    Goalie
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    PayAtRink
}

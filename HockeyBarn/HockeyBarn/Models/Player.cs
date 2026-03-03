namespace HockeyBarn.Models;

public partial record Player
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
    public List<Position> PreferredPositions { get; init; } = new();
    public SkillLevel SkillLevel { get; init; }
    public Handedness Handedness { get; init; }
    public int GamesPlayed { get; init; }
    public double ReliabilityPercent { get; init; }
    public double AverageRating { get; init; }
    public string HomeRink { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public List<DayOfWeek> AvailableDays { get; init; } = new();
    public List<PastTeam> PastTeams { get; init; } = new();
}

public partial record PastTeam
{
    public string Name { get; init; } = string.Empty;
    public string LogoUrl { get; init; } = string.Empty;
    public string YearRange { get; init; } = string.Empty;
}

public enum Handedness
{
    ShootsLeft,
    ShootsRight
}

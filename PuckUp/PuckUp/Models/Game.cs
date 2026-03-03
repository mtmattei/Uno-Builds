namespace PuckUp.Models;

public class Game
{
    public string Id { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string Arena { get; set; } = string.Empty;
    public string GameTime { get; set; } = string.Empty;
    public Dictionary<string, int> PositionsNeeded { get; set; } = new Dictionary<string, int>();
}

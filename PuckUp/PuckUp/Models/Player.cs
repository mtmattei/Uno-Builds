namespace PuckUp.Models;

public class Player
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string Arena { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public double Rating { get; set; }
}

namespace FormaEspresso.Models;

public record EspressoItem(
    string Id,
    string Name,
    string Volume,
    int Intensity,
    decimal Price,
    string Note,
    string Description,
    int TimeSeconds
)
{
    public string FormattedPrice => $"${Price:F2}";
    public string FormattedTime => $"{TimeSeconds} sec";
}

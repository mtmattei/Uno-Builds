namespace ClaudeDash.Models.Chat;

public record ContentCard(
    ContentCardType Type = ContentCardType.Text,
    string Title = "",
    string Body = "",
    string? Language = null,
    ImmutableList<(string Label, string Value)>? Stats = null,
    string? FilePath = null,
    string? NavigationKey = null,
    string? ActionLabel = null)
{
    public ImmutableList<(string Label, string Value)> Stats { get; init; } = Stats ?? ImmutableList<(string, string)>.Empty;
}

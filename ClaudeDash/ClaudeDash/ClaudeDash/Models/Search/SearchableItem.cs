namespace ClaudeDash.Models.Search;

public class SearchableItem
{
    public SearchResultType Type { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public string PageKey { get; set; } = string.Empty;

    // All searchable text tokens (lowercased, for matching)
    public List<string> SearchTokens { get; set; } = [];

    // The source object
    public object? SourceObject { get; set; }
}

namespace ClaudeDash.Models.Search;

public class SearchResult
{
    public SearchResultType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime LastActivity { get; set; }

    // Navigation target
    public string PageKey { get; set; } = string.Empty;
    public string? ItemId { get; set; }

    // The underlying object for rich display
    public object? SourceObject { get; set; }
}

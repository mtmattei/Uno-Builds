using ClaudeDash.Models.Search;

namespace ClaudeDash.Services;

public interface ISearchIndexService
{
    /// <summary>
    /// Build or rebuild the full search index from all services.
    /// </summary>
    Task BuildIndexAsync();

    /// <summary>
    /// Search the index with a query string. Returns results ranked by relevance and recency.
    /// </summary>
    List<SearchResult> Search(string query, int maxResults = 20);

    /// <summary>
    /// Get the current index size (number of indexed items).
    /// </summary>
    int IndexSize { get; }

    /// <summary>
    /// Whether the index has been built at least once.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Event raised when the index is rebuilt.
    /// </summary>
    event Action? IndexRebuilt;
}

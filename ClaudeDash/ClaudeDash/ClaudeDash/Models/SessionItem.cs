namespace ClaudeDash.Models;

public record SessionItem(
    string Status = "completed",
    string Description = "",
    string RepoName = "",
    DateTimeOffset Timestamp = default);

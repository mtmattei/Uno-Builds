using ClaudeDash.Models.Timeline;

namespace ClaudeDash.Services;

public interface ISessionParserService
{
    /// <summary>
    /// Parse a full session JSONL file into a rich timeline.
    /// </summary>
    Task<SessionTimeline?> ParseFullSessionAsync(string sessionFilePath);

    /// <summary>
    /// Parse a session by its ID, searching the projects directory.
    /// </summary>
    Task<SessionTimeline?> ParseSessionByIdAsync(string sessionId);

    /// <summary>
    /// Get lightweight metadata for recent sessions (faster than full parse).
    /// Reads the entire file but only extracts summary stats.
    /// </summary>
    Task<ClaudeSessionInfo> GetEnrichedSessionInfoAsync(string sessionFilePath);
}

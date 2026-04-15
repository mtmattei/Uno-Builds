namespace ClaudeDash.Models;

public record ProjectInfo(
    string Name = "",
    string Path = "",
    bool PathExists = false,
    bool IsGitRepo = false,
    bool HasClaudeMd = false,
    int SessionCount = 0,
    int WorktreeCount = 0,
    DateTime LastActivity = default,
    string CurrentBranch = "",
    bool IsPinned = false);

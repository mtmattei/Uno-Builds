namespace AdTokensIDE.Models;

public partial record ChatMessage(
    string Id,
    string Content,
    bool IsUser,
    DateTime Timestamp,
    string? SponsorName = null,
    int TokenCount = 0
);

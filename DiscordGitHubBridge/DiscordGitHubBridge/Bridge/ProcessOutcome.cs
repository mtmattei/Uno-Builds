namespace DiscordGitHubBridge.Bridge;

public enum OutcomeKind
{
    /// <summary>Issue created, mapping recorded, reply sent when configured.</summary>
    Created,

    /// <summary>Issue created and mapping recorded, but the Discord reply failed. Nothing to redo on GitHub.</summary>
    CreatedWithoutReply,

    /// <summary>Issue created but the mapping could not be updated. The Pending row blocks a duplicate; needs a manual look.</summary>
    CreatedUnrecorded,

    SkippedNotConfigured,
    SkippedRule,
    SkippedBotAuthor,
    AlreadyMapped,
    AlreadyInFlight,

    /// <summary>GitHub refused or kept failing. Reservation released so a later tag change can retry.</summary>
    Failed,
}

public sealed record ProcessOutcome(OutcomeKind Kind, string? Detail = null, long? IssueNumber = null, string? IssueUrl = null)
{
    public bool IssueExists => IssueNumber is not null;
}

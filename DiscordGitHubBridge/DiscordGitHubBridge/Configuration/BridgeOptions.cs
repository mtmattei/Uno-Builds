namespace DiscordGitHubBridge.Configuration;

/// <summary>Root options bound from the configuration root: the forum rules and bot behavior.</summary>
public sealed class BridgeOptions
{
    public List<ForumRuleOptions> Forums { get; set; } = [];

    public BehaviorOptions Behavior { get; set; } = new();
}

public sealed class BehaviorOptions
{
    public bool ReplyInThread { get; set; } = true;

    /// <summary>Supports the {issueUrl} and {issueNumber} placeholders.</summary>
    public string ReplyTemplate { get; set; } = "Tracked on GitHub: <{issueUrl}>";
}

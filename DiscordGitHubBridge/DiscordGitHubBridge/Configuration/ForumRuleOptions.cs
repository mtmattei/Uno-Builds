namespace DiscordGitHubBridge.Configuration;

public sealed class ForumRuleOptions
{
    /// <summary>Discord forum channel ID. The bridge only processes threads whose parent is one of these.</summary>
    public ulong ChannelId { get; set; }

    /// <summary>At least one of these tags must be applied for an issue to be created. Empty means any post qualifies.</summary>
    public List<string> RequiredTags { get; set; } = [];

    /// <summary>If any of these tags is applied, no issue is created.</summary>
    public List<string> IgnoredTags { get; set; } = [];

    /// <summary>Discord tag name to GitHub label name.</summary>
    public Dictionary<string, string> LabelMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Labels applied to every issue created from this forum.</summary>
    public List<string> DefaultLabels { get; set; } = [];
}

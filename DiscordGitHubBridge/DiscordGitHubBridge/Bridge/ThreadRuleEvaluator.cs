using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Bridge;

public enum RuleDecision
{
    /// <summary>The parent channel is not one of the configured forums.</summary>
    NotConfigured,

    /// <summary>The forum is configured but the thread's tags do not qualify.</summary>
    Ignored,

    /// <summary>An issue should be created with the returned labels.</summary>
    Qualifies,
}

public sealed record RuleResult(RuleDecision Decision, IReadOnlyList<string> Labels, string? Reason)
{
    public static RuleResult NotConfigured() => new(RuleDecision.NotConfigured, [], "parent channel is not configured");

    public static RuleResult Ignored(string reason) => new(RuleDecision.Ignored, [], reason);

    public static RuleResult Qualifies(IReadOnlyList<string> labels) => new(RuleDecision.Qualifies, labels, null);
}

/// <summary>Pure decision logic: does this thread get an issue, and with which labels.</summary>
public sealed class ThreadRuleEvaluator(IOptions<BridgeOptions> options)
{
    private static readonly StringComparer TagComparer = StringComparer.OrdinalIgnoreCase;

    public RuleResult Evaluate(ForumThreadSnapshot thread)
    {
        var forum = options.Value.Forums.FirstOrDefault(f => f.ChannelId == thread.ParentChannelId);
        if (forum is null)
        {
            return RuleResult.NotConfigured();
        }

        var applied = new HashSet<string>(thread.AppliedTagNames.Select(t => t.Trim()), TagComparer);

        var ignored = forum.IgnoredTags.FirstOrDefault(t => applied.Contains(t.Trim()));
        if (ignored is not null)
        {
            return RuleResult.Ignored($"tag '{ignored}' is in IgnoredTags");
        }

        if (forum.RequiredTags.Count > 0 && !forum.RequiredTags.Any(t => applied.Contains(t.Trim())))
        {
            return RuleResult.Ignored("none of the RequiredTags is applied");
        }

        return RuleResult.Qualifies(BuildLabels(forum, applied));
    }

    /// <summary>True when applying <paramref name="after"/> on top of <paramref name="before"/> newly satisfies the forum's trigger.</summary>
    public bool TriggerTagWasAdded(ulong parentChannelId, IReadOnlyList<string> before, IReadOnlyList<string> after)
    {
        var forum = options.Value.Forums.FirstOrDefault(f => f.ChannelId == parentChannelId);
        if (forum is null || forum.RequiredTags.Count == 0)
        {
            return false;
        }

        var beforeSet = new HashSet<string>(before.Select(t => t.Trim()), TagComparer);
        var afterSet = new HashSet<string>(after.Select(t => t.Trim()), TagComparer);

        var hadTrigger = forum.RequiredTags.Any(t => beforeSet.Contains(t.Trim()));
        var hasTrigger = forum.RequiredTags.Any(t => afterSet.Contains(t.Trim()));
        return !hadTrigger && hasTrigger;
    }

    private static IReadOnlyList<string> BuildLabels(ForumRuleOptions forum, HashSet<string> applied)
    {
        var labels = new List<string>();
        var seen = new HashSet<string>(TagComparer);

        foreach (var label in forum.DefaultLabels)
        {
            Add(label);
        }

        foreach (var tag in applied.OrderBy(t => t, TagComparer))
        {
            if (forum.LabelMap.TryGetValue(tag, out var label))
            {
                Add(label);
            }
        }

        return labels;

        void Add(string label)
        {
            var trimmed = label.Trim();
            if (trimmed.Length > 0 && seen.Add(trimmed))
            {
                labels.Add(trimmed);
            }
        }
    }
}

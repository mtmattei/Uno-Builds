using DiscordGitHubBridge.Bridge;
using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Tests;

internal static class TestData
{
    public const ulong Guild = 1182775715242967050UL;
    public const ulong Forum = 1547279690664771605UL;
    public const ulong OtherForum = 1549796979914313788UL;

    public static IOptions<BridgeOptions> Options(params ForumRuleOptions[] forums) =>
        Microsoft.Extensions.Options.Options.Create(new BridgeOptions { Forums = [.. forums] });

    public static ForumRuleOptions DefaultForum(ulong channelId = Forum) => new()
    {
        ChannelId = channelId,
        RequiredTags = ["Track on GitHub"],
        IgnoredTags = ["Duplicate"],
        LabelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bug"] = "bug",
            ["Feature Request"] = "enhancement",
        },
        DefaultLabels = ["from-discord"],
    };

    public static ForumThreadSnapshot Thread(
        ulong parent = Forum,
        string title = "Button does not render on Android",
        string? content = "Steps: open the page, tap the button. Nothing happens.",
        string[]? tags = null,
        string[]? attachments = null,
        string author = "matt",
        bool isBot = false,
        ulong threadId = 1550000000000000001UL) =>
        new(threadId, Guild, parent, title, tags ?? ["Track on GitHub"], content, attachments ?? [], author, isBot);
}

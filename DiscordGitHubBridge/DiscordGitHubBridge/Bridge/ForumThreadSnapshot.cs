namespace DiscordGitHubBridge.Bridge;

/// <summary>
/// Everything the bridge needs from a Discord forum thread, captured as plain data so the core
/// never depends on Discord.Net socket types.
/// </summary>
public sealed record ForumThreadSnapshot(
    ulong ThreadId,
    ulong GuildId,
    ulong ParentChannelId,
    string Title,
    IReadOnlyList<string> AppliedTagNames,
    string? StarterContent,
    IReadOnlyList<string> AttachmentUrls,
    string AuthorDisplayName,
    bool AuthorIsBot)
{
    public string ThreadUrl => $"https://discord.com/channels/{GuildId}/{ThreadId}";
}

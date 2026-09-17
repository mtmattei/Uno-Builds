namespace DiscordGitHubBridge.Bridge;

public interface IDiscordReplier
{
    Task ReplyAsync(ulong threadId, string message, CancellationToken cancellationToken);
}

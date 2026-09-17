using Discord;
using Discord.WebSocket;
using DiscordGitHubBridge.Bridge;

namespace DiscordGitHubBridge.Discord;

public sealed class DiscordThreadReplier(DiscordSocketClient client) : IDiscordReplier
{
    public async Task ReplyAsync(ulong threadId, string message, CancellationToken cancellationToken)
    {
        if (client.GetChannel(threadId) is not IMessageChannel channel)
        {
            throw new InvalidOperationException($"Discord thread {threadId} is not a message channel the bot can see.");
        }

        // AllowedMentions.None: the confirmation must never ping anyone.
        await channel.SendMessageAsync(message, allowedMentions: AllowedMentions.None, options: new RequestOptions { CancelToken = cancellationToken });
    }
}

using Discord;
using Discord.WebSocket;
using DiscordGitHubBridge.Bridge;

namespace DiscordGitHubBridge.Discord;

/// <summary>
/// Translates the two gateway thread events into processor calls.
/// ThreadCreated fires on creation and when the bot is added to a thread; ThreadUpdated fires for
/// every edit. Both are filtered here so only real work reaches the processor.
/// </summary>
public sealed class ForumThreadHandler(
    ForumThreadReader reader,
    ThreadRuleEvaluator rules,
    BridgeProcessor processor,
    TimeProvider timeProvider,
    ILogger<ForumThreadHandler> logger)
{
    public async Task OnThreadCreatedAsync(SocketThreadChannel thread, CancellationToken cancellationToken)
    {
        if (!IsForumThread(thread))
        {
            return;
        }

        await ProcessAsync(thread, cancellationToken);
    }

    public async Task OnThreadUpdatedAsync(SocketThreadChannel? before, SocketThreadChannel after, CancellationToken cancellationToken)
    {
        if (!IsForumThread(after))
        {
            return;
        }

        var afterTags = reader.ReadTagNames(after);

        if (before is not null)
        {
            var beforeTags = reader.ReadTagNames(before);
            if (!rules.TriggerTagWasAdded(after.ParentChannel!.Id, beforeTags, afterTags))
            {
                logger.LogDebug("Thread {DiscordThreadId} updated without gaining a trigger tag; ignoring", after.Id);
                return;
            }
        }
        else
        {
            // The pre-update state was not cached. Let the rule check and the mapping reservation decide.
            logger.LogDebug("Thread {DiscordThreadId} updated with no cached previous state; evaluating rules", after.Id);
        }

        await ProcessAsync(after, cancellationToken, afterTags);
    }

    private async Task ProcessAsync(SocketThreadChannel thread, CancellationToken cancellationToken, IReadOnlyList<string>? tagNames = null)
    {
        var tags = tagNames ?? reader.ReadTagNames(thread);
        var snapshot = await reader.ReadAsync(thread, tags, timeProvider, cancellationToken);
        await processor.ProcessAsync(snapshot, cancellationToken);
    }

    private static bool IsForumThread(SocketThreadChannel thread) =>
        thread.ParentChannel is SocketForumChannel;
}

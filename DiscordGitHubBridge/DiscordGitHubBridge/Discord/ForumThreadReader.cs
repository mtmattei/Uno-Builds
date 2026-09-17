using Discord;
using Discord.WebSocket;
using DiscordGitHubBridge.Bridge;

namespace DiscordGitHubBridge.Discord;

/// <summary>
/// Turns a live Discord thread into a plain <see cref="ForumThreadSnapshot"/>.
/// This is the only place that reads Discord.Net socket types.
/// </summary>
public sealed class ForumThreadReader(ILogger<ForumThreadReader> logger)
{
    private const int StarterMessageAttempts = 3;
    private static readonly TimeSpan StarterMessageDelay = TimeSpan.FromMilliseconds(500);

    /// <summary>Reads the applied tag names. Tags with no match in the parent forum are dropped with a warning.</summary>
    public IReadOnlyList<string> ReadTagNames(SocketThreadChannel thread)
    {
        if (thread.ParentChannel is not SocketForumChannel forum)
        {
            return [];
        }

        var byId = forum.Tags.ToDictionary(t => t.Id, t => t.Name);
        var names = new List<string>(thread.AppliedTags.Count);
        foreach (var tagId in thread.AppliedTags)
        {
            if (byId.TryGetValue(tagId, out var name))
            {
                names.Add(name);
            }
            else
            {
                logger.LogWarning("Thread {DiscordThreadId} has tag {TagId} which is not defined on forum {DiscordChannelId}", thread.Id, tagId, forum.Id);
            }
        }

        return names;
    }

    public async Task<ForumThreadSnapshot> ReadAsync(SocketThreadChannel thread, IReadOnlyList<string> tagNames, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        var starter = await GetStarterMessageAsync(thread, timeProvider, cancellationToken);

        var author = starter?.Author;
        var displayName = author?.GlobalName ?? author?.Username ?? thread.Owner?.DisplayName ?? "unknown";
        var attachments = starter?.Attachments.Select(a => a.Url).ToList() ?? [];

        return new ForumThreadSnapshot(
            thread.Id,
            thread.Guild.Id,
            thread.ParentChannel?.Id ?? 0,
            thread.Name ?? string.Empty,
            tagNames,
            starter?.Content,
            attachments,
            displayName,
            author?.IsBot ?? false);
    }

    /// <summary>
    /// In a forum the starter message shares the thread's ID. It can be momentarily absent right
    /// after ThreadCreated, so retry a few times before giving up.
    /// </summary>
    private async Task<IMessage?> GetStarterMessageAsync(SocketThreadChannel thread, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= StarterMessageAttempts; attempt++)
        {
            try
            {
                var message = await thread.GetMessageAsync(thread.Id);
                if (message is not null)
                {
                    if (string.IsNullOrEmpty(message.Content) && message.Attachments.Count == 0)
                    {
                        logger.LogWarning(
                            "Starter message for thread {DiscordThreadId} is empty. If this repeats, check that the Message Content privileged intent is enabled for the bot.",
                            thread.Id);
                    }
                    return message;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogDebug(ex, "Attempt {Attempt} to read the starter message of thread {DiscordThreadId} failed", attempt, thread.Id);
            }

            if (attempt < StarterMessageAttempts)
            {
                await Task.Delay(StarterMessageDelay, timeProvider, cancellationToken);
            }
        }

        logger.LogWarning("Starter message for thread {DiscordThreadId} was not available; the issue body will say so", thread.Id);
        return null;
    }
}

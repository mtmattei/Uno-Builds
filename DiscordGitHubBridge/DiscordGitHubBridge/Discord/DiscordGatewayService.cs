using Discord;
using Discord.WebSocket;
using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Discord;

/// <summary>
/// Owns the gateway connection and the event subscriptions. Handlers run detached so a slow GitHub
/// call never blocks Discord's event loop, and no handler exception can take the worker down.
/// </summary>
public sealed class DiscordGatewayService(
    DiscordSocketClient client,
    ForumThreadHandler handler,
    IOptions<DiscordOptions> discordOptions,
    IOptions<BridgeOptions> bridgeOptions,
    ILogger<DiscordGatewayService> logger) : IHostedService
{
    /// <summary>MessageContent is privileged and must be enabled on the Discord developer portal.</summary>
    public const GatewayIntents RequiredIntents =
        GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent;

    private static readonly TimeSpan ConnectionWarningDelay = TimeSpan.FromSeconds(90);

    private readonly CancellationTokenSource _stopping = new();
    private volatile bool _everReady;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += OnLogAsync;
        client.Ready += OnReadyAsync;
        client.Disconnected += OnDisconnectedAsync;
        client.ThreadCreated += OnThreadCreatedAsync;
        client.ThreadUpdated += OnThreadUpdatedAsync;

        await client.LoginAsync(TokenType.Bot, discordOptions.Value.Token);
        await client.StartAsync();
        logger.LogInformation("Discord gateway starting; watching {ForumCount} forum channel(s)", bridgeOptions.Value.Forums.Count);

        WarnIfNeverConnects();
    }

    /// <summary>
    /// A wrong token or a missing privileged intent shows up as an endless reconnect loop rather
    /// than an exception, so state the likely cause once instead of leaving it in gateway noise.
    /// </summary>
    private void WarnIfNeverConnects()
    {
        var token = _stopping.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(ConnectionWarningDelay, token);
                if (!_everReady)
                {
                    logger.LogError(
                        "Discord gateway has not become ready after {Seconds}s. Check the bot token, that the bot is invited to the server, and that the Message Content privileged intent is enabled.",
                        ConnectionWarningDelay.TotalSeconds);
                }
            }
            catch (OperationCanceledException)
            {
                // Shutting down.
            }
        }, CancellationToken.None);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync();

        client.ThreadCreated -= OnThreadCreatedAsync;
        client.ThreadUpdated -= OnThreadUpdatedAsync;
        client.Disconnected -= OnDisconnectedAsync;
        client.Ready -= OnReadyAsync;
        client.Log -= OnLogAsync;

        await client.StopAsync();
        await client.LogoutAsync();
        _stopping.Dispose();
    }

    private Task OnThreadCreatedAsync(SocketThreadChannel thread)
    {
        Detach(ct => handler.OnThreadCreatedAsync(thread, ct), thread.Id, nameof(OnThreadCreatedAsync));
        return Task.CompletedTask;
    }

    private Task OnThreadUpdatedAsync(Cacheable<SocketThreadChannel, ulong> before, SocketThreadChannel after)
    {
        var cached = before.HasValue ? before.Value : null;
        Detach(ct => handler.OnThreadUpdatedAsync(cached, after, ct), after.Id, nameof(OnThreadUpdatedAsync));
        return Task.CompletedTask;
    }

    /// <summary>Runs a handler off the gateway thread and swallows nothing silently.</summary>
    private void Detach(Func<CancellationToken, Task> work, ulong threadId, string source)
    {
        var token = _stopping.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await work(token);
            }
            catch (OperationCanceledException) when (_stopping.IsCancellationRequested)
            {
                // Shutting down.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Source} failed for Discord thread {DiscordThreadId}; the worker keeps running", source, threadId);
            }
        }, CancellationToken.None);
    }

    private Task OnReadyAsync()
    {
        _everReady = true;

        foreach (var rule in bridgeOptions.Value.Forums)
        {
            if (client.GetChannel(rule.ChannelId) is not SocketForumChannel forum)
            {
                logger.LogWarning("Configured channel {DiscordChannelId} is not visible as a forum channel. Check the bot's View Channel permission.", rule.ChannelId);
                continue;
            }

            var tagNames = forum.Tags.ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
            foreach (var required in rule.RequiredTags)
            {
                if (!tagNames.TryGetValue(required.Trim(), out var tag))
                {
                    logger.LogWarning("Forum '{ForumName}' ({DiscordChannelId}) has no tag named '{TagName}', so no thread there will ever qualify.", forum.Name, forum.Id, required);
                }
                else if (!tag.IsModerated)
                {
                    logger.LogInformation("Trigger tag '{TagName}' on forum '{ForumName}' is not moderator-only, so any member can create a GitHub issue.", tag.Name, forum.Name);
                }
            }

            logger.LogInformation("Watching forum '{ForumName}' ({DiscordChannelId}) with tags: {Tags}", forum.Name, forum.Id, string.Join(", ", forum.Tags.Select(t => t.Name)));
        }

        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(Exception exception)
    {
        logger.LogWarning(exception, "Discord gateway disconnected; Discord.Net will reconnect. Threads created or tagged while offline are not replayed.");
        return Task.CompletedTask;
    }

    private Task OnLogAsync(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            _ => LogLevel.Trace,
        };

        logger.Log(level, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
        return Task.CompletedTask;
    }
}

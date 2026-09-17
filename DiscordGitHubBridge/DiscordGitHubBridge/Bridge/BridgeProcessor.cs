using System.Collections.Concurrent;
using System.Globalization;
using DiscordGitHubBridge.Configuration;
using DiscordGitHubBridge.GitHub;
using DiscordGitHubBridge.Mapping;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Bridge;

/// <summary>
/// The V1 workflow (spec §5, steps 3–12): filter, reserve, build, create, record, reply.
/// Every step is idempotent from the outside: the same thread can be pushed through any number of
/// times and at most one GitHub issue results.
/// </summary>
public sealed class BridgeProcessor(
    ThreadRuleEvaluator rules,
    IssueFactory issueFactory,
    IMappingRepository mappings,
    IGitHubIssueClient gitHub,
    IDiscordReplier replier,
    IOptions<BridgeOptions> bridgeOptions,
    IOptions<GitHubOptions> gitHubOptions,
    ILogger<BridgeProcessor> logger)
{
    private readonly ConcurrentDictionary<ulong, byte> _inFlight = new();

    public async Task<ProcessOutcome> ProcessAsync(ForumThreadSnapshot thread, CancellationToken cancellationToken)
    {
        var repository = $"{gitHubOptions.Value.Owner}/{gitHubOptions.Value.Repository}";
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["DiscordThreadId"] = thread.ThreadId,
            ["DiscordChannelId"] = thread.ParentChannelId,
            ["GitHubRepository"] = repository,
        });

        var outcome = await ProcessCoreAsync(thread, cancellationToken);
        Log(outcome);
        return outcome;
    }

    private async Task<ProcessOutcome> ProcessCoreAsync(ForumThreadSnapshot thread, CancellationToken cancellationToken)
    {
        if (thread.AuthorIsBot)
        {
            return new ProcessOutcome(OutcomeKind.SkippedBotAuthor, "starter message was posted by a bot");
        }

        var rule = rules.Evaluate(thread);
        switch (rule.Decision)
        {
            case RuleDecision.NotConfigured:
                return new ProcessOutcome(OutcomeKind.SkippedNotConfigured, rule.Reason);
            case RuleDecision.Ignored:
                return new ProcessOutcome(OutcomeKind.SkippedRule, rule.Reason);
        }

        if (!_inFlight.TryAdd(thread.ThreadId, 0))
        {
            return new ProcessOutcome(OutcomeKind.AlreadyInFlight, "another event for this thread is being processed");
        }

        try
        {
            var reservation = await mappings.TryReserveAsync(thread.ThreadId, thread.ParentChannelId, cancellationToken);
            if (reservation.Outcome == ReservationOutcome.AlreadyMapped)
            {
                var existing = reservation.Existing;
                return new ProcessOutcome(
                    OutcomeKind.AlreadyMapped,
                    $"mapping exists with status {existing?.Status}",
                    existing?.GitHubIssueNumber,
                    existing?.GitHubIssueUrl);
            }

            var request = issueFactory.Create(thread, rule.Labels);

            CreatedIssue issue;
            try
            {
                issue = await gitHub.CreateIssueAsync(request, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "GitHub issue creation failed; releasing reservation");
                await ReleaseQuietlyAsync(thread.ThreadId);
                return new ProcessOutcome(OutcomeKind.Failed, ex.Message);
            }

            // Spec §13: log the issue immediately so a crash below still leaves a trail.
            logger.LogInformation("Created GitHub issue #{GitHubIssueNumber} {GitHubIssueUrl}", issue.Number, issue.HtmlUrl);

            try
            {
                await mappings.CompleteAsync(thread.ThreadId, issue.Number, issue.HtmlUrl, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Issue #{GitHubIssueNumber} exists at {GitHubIssueUrl} but the mapping could not be recorded; the Pending row prevents a duplicate", issue.Number, issue.HtmlUrl);
                return new ProcessOutcome(OutcomeKind.CreatedUnrecorded, ex.Message, issue.Number, issue.HtmlUrl);
            }

            if (!bridgeOptions.Value.Behavior.ReplyInThread)
            {
                return new ProcessOutcome(OutcomeKind.Created, "reply disabled", issue.Number, issue.HtmlUrl);
            }

            try
            {
                await replier.ReplyAsync(thread.ThreadId, FormatReply(issue), cancellationToken);
                await mappings.MarkNotifiedAsync(thread.ThreadId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Issue #{GitHubIssueNumber} created but the Discord reply failed", issue.Number);
                return new ProcessOutcome(OutcomeKind.CreatedWithoutReply, ex.Message, issue.Number, issue.HtmlUrl);
            }

            return new ProcessOutcome(OutcomeKind.Created, null, issue.Number, issue.HtmlUrl);
        }
        finally
        {
            _inFlight.TryRemove(thread.ThreadId, out _);
        }
    }

    private string FormatReply(CreatedIssue issue) =>
        bridgeOptions.Value.Behavior.ReplyTemplate
            .Replace("{issueUrl}", issue.HtmlUrl, StringComparison.OrdinalIgnoreCase)
            .Replace("{issueNumber}", issue.Number.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

    private async Task ReleaseQuietlyAsync(ulong threadId)
    {
        try
        {
            await mappings.ReleaseAsync(threadId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not release the Pending reservation; the thread will read as AlreadyMapped until the row is removed");
        }
    }

    private void Log(ProcessOutcome outcome)
    {
        switch (outcome.Kind)
        {
            case OutcomeKind.Created:
                logger.LogInformation("Bridged thread to issue #{GitHubIssueNumber}", outcome.IssueNumber);
                break;
            case OutcomeKind.SkippedNotConfigured:
            case OutcomeKind.SkippedRule:
            case OutcomeKind.SkippedBotAuthor:
            case OutcomeKind.AlreadyInFlight:
                logger.LogDebug("Skipped thread: {Outcome} ({Detail})", outcome.Kind, outcome.Detail);
                break;
            case OutcomeKind.AlreadyMapped:
                logger.LogInformation("Thread already mapped to issue #{GitHubIssueNumber} ({Detail})", outcome.IssueNumber, outcome.Detail);
                break;
            default:
                // Failed, CreatedUnrecorded and CreatedWithoutReply already logged their cause with the exception.
                logger.LogInformation("Outcome {Outcome} for thread", outcome.Kind);
                break;
        }
    }
}

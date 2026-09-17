using DiscordGitHubBridge.Bridge;
using DiscordGitHubBridge.Configuration;
using DiscordGitHubBridge.GitHub;
using DiscordGitHubBridge.Mapping;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Tests;

public sealed class BridgeProcessorTests : IDisposable
{
    private const ulong Thread = 1550000000000000001UL;

    private sealed class FakeGitHub : IGitHubIssueClient
    {
        public List<NewIssueRequest> Requests { get; } = [];
        public Exception? Throw { get; set; }

        public Task<CreatedIssue> CreateIssueAsync(NewIssueRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (Throw is not null)
            {
                throw Throw;
            }
            var number = 100 + Requests.Count;
            return Task.FromResult(new CreatedIssue(number, $"https://github.com/o/r/issues/{number}"));
        }
    }

    private sealed class FakeReplier : IDiscordReplier
    {
        public List<(ulong ThreadId, string Message)> Replies { get; } = [];
        public Exception? Throw { get; set; }

        public Task ReplyAsync(ulong threadId, string message, CancellationToken cancellationToken)
        {
            if (Throw is not null)
            {
                throw Throw;
            }
            Replies.Add((threadId, message));
            return Task.CompletedTask;
        }
    }

    /// <summary>Wraps the real SQLite repository so one call can be made to fail.</summary>
    private sealed class FlakyRepository(IMappingRepository inner) : IMappingRepository
    {
        public bool FailComplete { get; set; }

        public Task<ReservationResult> TryReserveAsync(ulong threadId, ulong channelId, CancellationToken ct) => inner.TryReserveAsync(threadId, channelId, ct);
        public Task CompleteAsync(ulong threadId, long issueNumber, string issueUrl, CancellationToken ct) =>
            FailComplete ? throw new InvalidOperationException("disk full") : inner.CompleteAsync(threadId, issueNumber, issueUrl, ct);
        public Task MarkNotifiedAsync(ulong threadId, CancellationToken ct) => inner.MarkNotifiedAsync(threadId, ct);
        public Task ReleaseAsync(ulong threadId, CancellationToken ct) => inner.ReleaseAsync(threadId, ct);
        public Task<ThreadIssueMapping?> FindAsync(ulong threadId, CancellationToken ct) => inner.FindAsync(threadId, ct);
        public Task<IReadOnlyList<ThreadIssueMapping>> GetStalePendingAsync(DateTimeOffset olderThan, CancellationToken ct) => inner.GetStalePendingAsync(olderThan, ct);
    }

    private readonly SqliteTestDatabase _db = new();
    private readonly FlakyRepository _repo;
    private readonly FakeGitHub _gitHub = new();
    private readonly FakeReplier _replier = new();
    private readonly BridgeOptions _bridgeOptions = new() { Forums = [TestData.DefaultForum()] };

    public BridgeProcessorTests()
    {
        _repo = new FlakyRepository(new MappingRepository(_db, TimeProvider.System));
    }

    public void Dispose() => _db.Dispose();

    private BridgeProcessor Processor() => new(
        new ThreadRuleEvaluator(Options.Create(_bridgeOptions)),
        new IssueFactory(),
        _repo,
        _gitHub,
        _replier,
        Options.Create(_bridgeOptions),
        Options.Create(new GitHubOptions { Owner = "o", Repository = "r" }),
        NullLogger<BridgeProcessor>.Instance);

    [Fact]
    public async Task Qualifying_thread_creates_one_issue_records_mapping_and_replies()
    {
        var outcome = await Processor().ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);

        Assert.Equal(OutcomeKind.Created, outcome.Kind);
        Assert.Equal(101, outcome.IssueNumber);
        var request = Assert.Single(_gitHub.Requests);
        Assert.Equal("Button does not render on Android", request.Title);
        Assert.Equal(["from-discord"], request.Labels);
        Assert.Contains("https://discord.com/channels/1182775715242967050/1550000000000000001", request.Body);

        var reply = Assert.Single(_replier.Replies);
        Assert.Equal(Thread, reply.ThreadId);
        Assert.Equal("Tracked on GitHub: <https://github.com/o/r/issues/101>", reply.Message);

        var mapping = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.Equal(MappingStatus.Notified, mapping!.Status);
        Assert.Equal(101, mapping.GitHubIssueNumber);
    }

    [Fact]
    public async Task Non_qualifying_thread_creates_nothing()
    {
        var processor = Processor();

        var notConfigured = await processor.ProcessAsync(TestData.Thread(parent: TestData.OtherForum), CancellationToken.None);
        var noTag = await processor.ProcessAsync(TestData.Thread(tags: ["Bug"]), CancellationToken.None);
        var bot = await processor.ProcessAsync(TestData.Thread(isBot: true), CancellationToken.None);

        Assert.Equal(OutcomeKind.SkippedNotConfigured, notConfigured.Kind);
        Assert.Equal(OutcomeKind.SkippedRule, noTag.Kind);
        Assert.Equal(OutcomeKind.SkippedBotAuthor, bot.Kind);
        Assert.Empty(_gitHub.Requests);
        Assert.Empty(_replier.Replies);
        Assert.Null(await _repo.FindAsync(Thread, CancellationToken.None));
    }

    [Fact]
    public async Task Reprocessing_same_thread_does_not_create_another_issue()
    {
        var processor = Processor();
        var thread = TestData.Thread(threadId: Thread);

        var first = await processor.ProcessAsync(thread, CancellationToken.None);
        var second = await processor.ProcessAsync(thread, CancellationToken.None);

        Assert.Equal(OutcomeKind.Created, first.Kind);
        Assert.Equal(OutcomeKind.AlreadyMapped, second.Kind);
        Assert.Equal(101, second.IssueNumber);
        Assert.Single(_gitHub.Requests);
        Assert.Single(_replier.Replies);
    }

    [Fact]
    public async Task Late_tag_path_creates_exactly_one_issue()
    {
        var processor = Processor();

        var untagged = await processor.ProcessAsync(TestData.Thread(threadId: Thread, tags: []), CancellationToken.None);
        var tagged = await processor.ProcessAsync(TestData.Thread(threadId: Thread, tags: ["Track on GitHub"]), CancellationToken.None);
        var again = await processor.ProcessAsync(TestData.Thread(threadId: Thread, tags: ["Track on GitHub", "Bug"]), CancellationToken.None);

        Assert.Equal(OutcomeKind.SkippedRule, untagged.Kind);
        Assert.Equal(OutcomeKind.Created, tagged.Kind);
        Assert.Equal(OutcomeKind.AlreadyMapped, again.Kind);
        Assert.Single(_gitHub.Requests);
    }

    [Fact]
    public async Task GitHub_failure_leaves_no_mapping_and_no_reply()
    {
        _gitHub.Throw = new InvalidOperationException("boom");

        var outcome = await Processor().ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);

        Assert.Equal(OutcomeKind.Failed, outcome.Kind);
        Assert.Null(await _repo.FindAsync(Thread, CancellationToken.None));
        Assert.Empty(_replier.Replies);
    }

    [Fact]
    public async Task After_github_failure_the_thread_can_be_retried()
    {
        var processor = Processor();
        _gitHub.Throw = new InvalidOperationException("boom");
        await processor.ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);

        _gitHub.Throw = null;
        var outcome = await processor.ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);

        Assert.Equal(OutcomeKind.Created, outcome.Kind);
        Assert.Equal(2, _gitHub.Requests.Count);
    }

    [Fact]
    public async Task Persistence_failure_after_creation_keeps_pending_row_and_blocks_duplicate()
    {
        var processor = Processor();
        _repo.FailComplete = true;

        var outcome = await processor.ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);
        Assert.Equal(OutcomeKind.CreatedUnrecorded, outcome.Kind);
        Assert.Equal(101, outcome.IssueNumber);
        Assert.Empty(_replier.Replies);

        var row = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.Equal(MappingStatus.Pending, row!.Status);

        _repo.FailComplete = false;
        var again = await processor.ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);
        Assert.Equal(OutcomeKind.AlreadyMapped, again.Kind);
        Assert.Single(_gitHub.Requests);
    }

    [Fact]
    public async Task Reply_failure_keeps_created_mapping()
    {
        _replier.Throw = new InvalidOperationException("missing permission");

        var outcome = await Processor().ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);

        Assert.Equal(OutcomeKind.CreatedWithoutReply, outcome.Kind);
        var row = await _repo.FindAsync(Thread, CancellationToken.None);
        Assert.Equal(MappingStatus.Created, row!.Status);
        Assert.Equal(101, row.GitHubIssueNumber);
    }

    [Fact]
    public async Task Reply_can_be_disabled_and_template_supports_issue_number()
    {
        _bridgeOptions.Behavior.ReplyInThread = false;
        var silent = await Processor().ProcessAsync(TestData.Thread(threadId: Thread), CancellationToken.None);
        Assert.Equal(OutcomeKind.Created, silent.Kind);
        Assert.Empty(_replier.Replies);
        Assert.Equal(MappingStatus.Created, (await _repo.FindAsync(Thread, CancellationToken.None))!.Status);

        _bridgeOptions.Behavior.ReplyInThread = true;
        _bridgeOptions.Behavior.ReplyTemplate = "Issue #{issueNumber}: {issueUrl}";
        await Processor().ProcessAsync(TestData.Thread(threadId: Thread + 1), CancellationToken.None);
        Assert.Equal("Issue #102: https://github.com/o/r/issues/102", Assert.Single(_replier.Replies).Message);
    }

    [Fact]
    public async Task Concurrent_events_for_same_thread_create_one_issue()
    {
        var processor = Processor();
        var thread = TestData.Thread(threadId: Thread);

        var outcomes = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() => processor.ProcessAsync(thread, CancellationToken.None))));

        Assert.Single(_gitHub.Requests);
        Assert.Equal(1, outcomes.Count(o => o.Kind == OutcomeKind.Created));
        Assert.All(outcomes.Where(o => o.Kind != OutcomeKind.Created), o => Assert.Contains(o.Kind, new[] { OutcomeKind.AlreadyInFlight, OutcomeKind.AlreadyMapped }));
    }
}

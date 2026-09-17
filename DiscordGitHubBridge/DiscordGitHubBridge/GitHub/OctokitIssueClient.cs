using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;
using Octokit;
using Polly;
using Polly.Retry;

namespace DiscordGitHubBridge.GitHub;

public sealed class OctokitIssueClient : IGitHubIssueClient
{
    public static readonly ProductHeaderValue ProductHeader = new("DiscordGitHubBridge", "1.0");

    private readonly GitHubOptions _options;
    private readonly IGitHubCredentialProvider _credentials;
    private readonly ILogger<OctokitIssueClient> _logger;
    private readonly GitHubClient _client = new(ProductHeader);
    private readonly ResiliencePipeline _retry;

    public OctokitIssueClient(IOptions<GitHubOptions> options, IGitHubCredentialProvider credentials, ILogger<OctokitIssueClient> logger)
    {
        _options = options.Value;
        _credentials = credentials;
        _logger = logger;
        _retry = BuildRetryPipeline(logger);
    }

    public async Task<CreatedIssue> CreateIssueAsync(NewIssueRequest request, CancellationToken cancellationToken)
    {
        var newIssue = new NewIssue(request.Title) { Body = request.Body };
        foreach (var label in request.Labels)
        {
            newIssue.Labels.Add(label);
        }

        var issue = await _retry.ExecuteAsync(async ct =>
        {
            _client.Credentials = await _credentials.GetCredentialsAsync(ct);
            return await _client.Issue.Create(_options.Owner, _options.Repository, newIssue);
        }, cancellationToken);

        return new CreatedIssue(issue.Number, issue.HtmlUrl);
    }

    internal static ResiliencePipeline BuildRetryPipeline(ILogger logger, TimeSpan? baseDelay = null) =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = baseDelay ?? TimeSpan.FromSeconds(2),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(GitHubErrors.IsTransient),
                OnRetry = args =>
                {
                    logger.LogWarning(args.Outcome.Exception,
                        "GitHub call failed with a transient error; retry {Attempt} in {Delay}",
                        args.AttemptNumber + 1, args.RetryDelay);
                    return ValueTask.CompletedTask;
                },
            })
            .Build();
}

using System.Net;
using DiscordGitHubBridge.GitHub;
using Microsoft.Extensions.Logging.Abstractions;
using Octokit;

namespace DiscordGitHubBridge.Tests;

public class GitHubErrorsTests
{
    private sealed class FakeResponse(HttpStatusCode status) : IResponse
    {
        public object? Body => null;
        public IReadOnlyDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public ApiInfo ApiInfo { get; } = new(new Dictionary<string, Uri>(), [], [], string.Empty, new RateLimit(5000, 0, 0));
        public HttpStatusCode StatusCode => status;
        public string? ContentType => "application/json";
    }

    public static TheoryData<Exception, bool> Cases => new()
    {
        { new RateLimitExceededException(new FakeResponse(HttpStatusCode.Forbidden)), true },
        { new SecondaryRateLimitExceededException(new FakeResponse(HttpStatusCode.Forbidden)), true },
        { new AbuseException(new FakeResponse(HttpStatusCode.Forbidden)), true },
        { new ApiException("bad gateway", HttpStatusCode.BadGateway), true },
        { new ApiException("unavailable", HttpStatusCode.ServiceUnavailable), true },
        { new HttpRequestException("socket"), true },
        { new AuthorizationException(HttpStatusCode.Unauthorized, null!), false },
        { new NotFoundException("nope", HttpStatusCode.NotFound), false },
        { new ApiValidationException(), false },
        { new ApiException("forbidden", HttpStatusCode.Forbidden), false },
        { new TaskCanceledException(), false },
        { new InvalidOperationException(), false },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Classifies_transient_errors(Exception exception, bool expected)
    {
        Assert.Equal(expected, GitHubErrors.IsTransient(exception));
    }

    [Fact]
    public async Task Retry_pipeline_retries_transient_then_succeeds()
    {
        var pipeline = OctokitIssueClient.BuildRetryPipeline(NullLogger.Instance, TimeSpan.Zero);
        var attempts = 0;

        var result = await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new ApiException("bad gateway", HttpStatusCode.BadGateway);
            }
            return ValueTask.FromResult("ok");
        }, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task Retry_pipeline_gives_up_after_bounded_attempts()
    {
        var pipeline = OctokitIssueClient.BuildRetryPipeline(NullLogger.Instance, TimeSpan.Zero);
        var attempts = 0;

        await Assert.ThrowsAsync<ApiException>(async () => await pipeline.ExecuteAsync<string>(_ =>
        {
            attempts++;
            throw new ApiException("bad gateway", HttpStatusCode.BadGateway);
        }, CancellationToken.None));

        Assert.Equal(4, attempts);
    }

    [Fact]
    public async Task Retry_pipeline_does_not_retry_permanent_errors()
    {
        var pipeline = OctokitIssueClient.BuildRetryPipeline(NullLogger.Instance, TimeSpan.Zero);
        var attempts = 0;

        await Assert.ThrowsAsync<AuthorizationException>(async () => await pipeline.ExecuteAsync<string>(_ =>
        {
            attempts++;
            throw new AuthorizationException(HttpStatusCode.Unauthorized, null!);
        }, CancellationToken.None));

        Assert.Equal(1, attempts);
    }
}

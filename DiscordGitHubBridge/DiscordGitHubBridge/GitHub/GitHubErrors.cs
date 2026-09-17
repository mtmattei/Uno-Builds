using System.Net;
using Octokit;

namespace DiscordGitHubBridge.GitHub;

public static class GitHubErrors
{
    /// <summary>
    /// Rate limits and server errors are worth a bounded retry. Authentication, validation and
    /// not-found are permanent until a human changes something, so they are never retried.
    /// </summary>
    public static bool IsTransient(Exception exception) => exception switch
    {
        RateLimitExceededException => true,
        SecondaryRateLimitExceededException => true,
        AbuseException => true,
        AuthorizationException => false,
        NotFoundException => false,
        ApiValidationException => false,
        ApiException api => (int)api.StatusCode >= 500 || api.StatusCode == HttpStatusCode.RequestTimeout,
        HttpRequestException => true,
        TaskCanceledException => false,
        _ => false,
    };
}

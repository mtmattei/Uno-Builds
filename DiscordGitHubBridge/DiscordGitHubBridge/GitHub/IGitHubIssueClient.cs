namespace DiscordGitHubBridge.GitHub;

public interface IGitHubIssueClient
{
    /// <summary>Creates the issue in the configured repository. Transient failures are retried inside; anything thrown is final.</summary>
    Task<CreatedIssue> CreateIssueAsync(NewIssueRequest request, CancellationToken cancellationToken);
}

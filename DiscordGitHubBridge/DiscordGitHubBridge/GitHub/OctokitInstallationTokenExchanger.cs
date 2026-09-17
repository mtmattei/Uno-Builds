using Octokit;

namespace DiscordGitHubBridge.GitHub;

public sealed class OctokitInstallationTokenExchanger : IInstallationTokenExchanger
{
    public async Task<long> ResolveInstallationIdAsync(string appJwt, string owner, string repository, CancellationToken cancellationToken)
    {
        var installation = await CreateClient(appJwt).GitHubApps.GetRepositoryInstallationForCurrent(owner, repository);
        return installation.Id;
    }

    public async Task<AccessToken> ExchangeAsync(string appJwt, long installationId, CancellationToken cancellationToken) =>
        await CreateClient(appJwt).GitHubApps.CreateInstallationToken(installationId);

    private static GitHubClient CreateClient(string appJwt) =>
        new(OctokitIssueClient.ProductHeader) { Credentials = new Credentials(appJwt, AuthenticationType.Bearer) };
}

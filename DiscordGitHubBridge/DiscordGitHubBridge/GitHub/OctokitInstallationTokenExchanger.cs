using Octokit;

namespace DiscordGitHubBridge.GitHub;

public sealed class OctokitInstallationTokenExchanger : IInstallationTokenExchanger
{
    public async Task<AccessToken> ExchangeAsync(string appJwt, long installationId, CancellationToken cancellationToken)
    {
        var client = new GitHubClient(OctokitIssueClient.ProductHeader)
        {
            Credentials = new Credentials(appJwt, AuthenticationType.Bearer),
        };
        return await client.GitHubApps.CreateInstallationToken(installationId);
    }
}

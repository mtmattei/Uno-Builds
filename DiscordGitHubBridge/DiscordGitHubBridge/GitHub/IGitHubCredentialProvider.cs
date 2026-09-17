using Octokit;

namespace DiscordGitHubBridge.GitHub;

public interface IGitHubCredentialProvider
{
    ValueTask<Credentials> GetCredentialsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// The two GitHub App calls the bridge makes with an App JWT rather than an installation token.
/// Abstracted so the caching provider can be tested without the network.
/// </summary>
public interface IInstallationTokenExchanger
{
    /// <summary>Finds the installation ID of the App on a specific repository, so it need not be configured by hand.</summary>
    Task<long> ResolveInstallationIdAsync(string appJwt, string owner, string repository, CancellationToken cancellationToken);

    Task<AccessToken> ExchangeAsync(string appJwt, long installationId, CancellationToken cancellationToken);
}

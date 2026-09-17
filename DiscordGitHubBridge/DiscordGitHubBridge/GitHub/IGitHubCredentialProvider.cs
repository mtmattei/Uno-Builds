using Octokit;

namespace DiscordGitHubBridge.GitHub;

public interface IGitHubCredentialProvider
{
    ValueTask<Credentials> GetCredentialsAsync(CancellationToken cancellationToken);
}

/// <summary>Exchanges an App JWT for an installation token. Abstracted so the caching provider can be tested without the network.</summary>
public interface IInstallationTokenExchanger
{
    Task<AccessToken> ExchangeAsync(string appJwt, long installationId, CancellationToken cancellationToken);
}

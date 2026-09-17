using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;
using Octokit;

namespace DiscordGitHubBridge.GitHub;

/// <summary>Local development only. Production uses <see cref="GitHubAppCredentialProvider"/>.</summary>
public sealed class PatCredentialProvider(IOptions<GitHubOptions> options) : IGitHubCredentialProvider
{
    private readonly Credentials _credentials = new(options.Value.Auth.Token!);

    public ValueTask<Credentials> GetCredentialsAsync(CancellationToken cancellationToken) => new(_credentials);
}

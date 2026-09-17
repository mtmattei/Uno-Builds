using System.Security.Cryptography;
using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;
using Octokit;

namespace DiscordGitHubBridge.GitHub;

/// <summary>Mints a GitHub App installation token and caches it until shortly before it expires.</summary>
public sealed class GitHubAppCredentialProvider : IGitHubCredentialProvider, IDisposable
{
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly GitHubOptions _options;
    private readonly GitHubAuthOptions _auth;
    private readonly IInstallationTokenExchanger _exchanger;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubAppCredentialProvider> _logger;
    private readonly RSA _privateKey;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private Credentials? _cached;
    private DateTimeOffset _expiresAt;
    private long _installationId;

    public GitHubAppCredentialProvider(
        IOptions<GitHubOptions> options,
        IInstallationTokenExchanger exchanger,
        TimeProvider timeProvider,
        ILogger<GitHubAppCredentialProvider> logger)
    {
        _options = options.Value;
        _auth = _options.Auth;
        _exchanger = exchanger;
        _timeProvider = timeProvider;
        _logger = logger;

        var pem = !string.IsNullOrWhiteSpace(_auth.PrivateKeyPem)
            ? _auth.PrivateKeyPem
            : File.ReadAllText(_auth.PrivateKeyPath!);
        _privateKey = GitHubAppJwt.LoadPrivateKey(pem);
    }

    public async ValueTask<Credentials> GetCredentialsAsync(CancellationToken cancellationToken)
    {
        if (TryGetFresh(out var fresh))
        {
            return fresh;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (TryGetFresh(out fresh))
            {
                return fresh;
            }

            var now = _timeProvider.GetUtcNow();
            var jwt = GitHubAppJwt.Create(_auth.AppId, _privateKey, now);
            var installationId = await GetInstallationIdAsync(jwt, cancellationToken);
            var token = await _exchanger.ExchangeAsync(jwt, installationId, cancellationToken);

            _cached = new Credentials(token.Token);
            _expiresAt = token.ExpiresAt;
            _logger.LogInformation("Minted GitHub App installation token for installation {InstallationId}, expires {ExpiresAt}", installationId, token.ExpiresAt);
            return _cached;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Uses the configured installation ID when there is one; otherwise asks GitHub which
    /// installation covers the target repository, so the ID never has to be looked up by hand.
    /// </summary>
    private async Task<long> GetInstallationIdAsync(string jwt, CancellationToken cancellationToken)
    {
        if (_auth.InstallationId > 0)
        {
            return _auth.InstallationId;
        }

        if (_installationId > 0)
        {
            return _installationId;
        }

        _installationId = await _exchanger.ResolveInstallationIdAsync(jwt, _options.Owner, _options.Repository, cancellationToken);
        _logger.LogInformation(
            "Resolved GitHub App installation {InstallationId} for {Owner}/{Repository}. Set GitHub:Auth:InstallationId to this value to skip the lookup.",
            _installationId, _options.Owner, _options.Repository);
        return _installationId;
    }

    private bool TryGetFresh(out Credentials credentials)
    {
        var cached = _cached;
        if (cached is not null && _timeProvider.GetUtcNow() < _expiresAt - RefreshMargin)
        {
            credentials = cached;
            return true;
        }

        credentials = null!;
        return false;
    }

    public void Dispose()
    {
        _privateKey.Dispose();
        _gate.Dispose();
    }
}

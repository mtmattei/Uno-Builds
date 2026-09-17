using System.Security.Cryptography;
using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Options;
using Octokit;

namespace DiscordGitHubBridge.GitHub;

/// <summary>Mints a GitHub App installation token and caches it until shortly before it expires.</summary>
public sealed class GitHubAppCredentialProvider : IGitHubCredentialProvider, IDisposable
{
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly GitHubAuthOptions _auth;
    private readonly IInstallationTokenExchanger _exchanger;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GitHubAppCredentialProvider> _logger;
    private readonly RSA _privateKey;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private Credentials? _cached;
    private DateTimeOffset _expiresAt;

    public GitHubAppCredentialProvider(
        IOptions<GitHubOptions> options,
        IInstallationTokenExchanger exchanger,
        TimeProvider timeProvider,
        ILogger<GitHubAppCredentialProvider> logger)
    {
        _auth = options.Value.Auth;
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
            var token = await _exchanger.ExchangeAsync(jwt, _auth.InstallationId, cancellationToken);

            _cached = new Credentials(token.Token);
            _expiresAt = token.ExpiresAt;
            _logger.LogInformation("Minted GitHub App installation token for installation {InstallationId}, expires {ExpiresAt}", _auth.InstallationId, token.ExpiresAt);
            return _cached;
        }
        finally
        {
            _gate.Release();
        }
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

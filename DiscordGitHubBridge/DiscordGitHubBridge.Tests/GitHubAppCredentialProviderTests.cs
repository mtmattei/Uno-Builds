using System.Security.Cryptography;
using DiscordGitHubBridge.Configuration;
using DiscordGitHubBridge.GitHub;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Octokit;

namespace DiscordGitHubBridge.Tests;

public class GitHubAppCredentialProviderTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 17, 12, 0, 0, TimeSpan.Zero);

    private sealed class FakeExchanger : IInstallationTokenExchanger
    {
        public int Calls { get; private set; }
        public DateTimeOffset ExpiresAt { get; set; } = T0.AddHours(1);
        public string? LastJwt { get; private set; }
        public long LastInstallationId { get; private set; }

        public Task<AccessToken> ExchangeAsync(string appJwt, long installationId, CancellationToken cancellationToken)
        {
            Calls++;
            LastJwt = appJwt;
            LastInstallationId = installationId;
            return Task.FromResult(new AccessToken($"ghs_token_{Calls}", ExpiresAt));
        }
    }

    private static (GitHubAppCredentialProvider Provider, FakeExchanger Exchanger, FakeTimeProvider Clock) Create()
    {
        using var rsa = RSA.Create(2048);
        var options = Options.Create(new GitHubOptions
        {
            Owner = "o",
            Repository = "r",
            Auth = new GitHubAuthOptions { Mode = GitHubAuthMode.App, AppId = 42, InstallationId = 99, PrivateKeyPem = rsa.ExportRSAPrivateKeyPem() },
        });
        var exchanger = new FakeExchanger();
        var clock = new FakeTimeProvider(T0);
        var provider = new GitHubAppCredentialProvider(options, exchanger, clock, NullLogger<GitHubAppCredentialProvider>.Instance);
        return (provider, exchanger, clock);
    }

    [Fact]
    public async Task First_call_exchanges_jwt_for_installation_token()
    {
        var (provider, exchanger, _) = Create();
        using (provider)
        {
            var credentials = await provider.GetCredentialsAsync(CancellationToken.None);

            Assert.Equal("ghs_token_1", credentials.Password);
            Assert.Equal(AuthenticationType.Oauth, credentials.AuthenticationType);
            Assert.Equal(99, exchanger.LastInstallationId);
            Assert.Equal(3, exchanger.LastJwt!.Split('.').Length);
        }
    }

    [Fact]
    public async Task Token_is_cached_until_refresh_margin_then_renewed()
    {
        var (provider, exchanger, clock) = Create();
        using (provider)
        {
            await provider.GetCredentialsAsync(CancellationToken.None);
            clock.Now = T0.AddMinutes(30);
            var cached = await provider.GetCredentialsAsync(CancellationToken.None);
            Assert.Equal("ghs_token_1", cached.Password);
            Assert.Equal(1, exchanger.Calls);

            clock.Now = T0.AddMinutes(56);
            var renewed = await provider.GetCredentialsAsync(CancellationToken.None);
            Assert.Equal("ghs_token_2", renewed.Password);
            Assert.Equal(2, exchanger.Calls);
        }
    }

    [Fact]
    public async Task Concurrent_callers_share_one_exchange()
    {
        var (provider, exchanger, _) = Create();
        using (provider)
        {
            var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => provider.GetCredentialsAsync(CancellationToken.None).AsTask()));

            Assert.All(results, c => Assert.Equal("ghs_token_1", c.Password));
            Assert.Equal(1, exchanger.Calls);
        }
    }
}

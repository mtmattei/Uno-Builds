using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DiscordGitHubBridge.GitHub;

namespace DiscordGitHubBridge.Tests;

public class GitHubAppJwtTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Token_has_rs256_header_expected_claims_and_valid_signature()
    {
        using var rsa = RSA.Create(2048);

        var jwt = GitHubAppJwt.Create(123456, rsa, Now);

        var parts = jwt.Split('.');
        Assert.Equal(3, parts.Length);

        using var header = JsonDocument.Parse(Base64UrlDecode(parts[0]));
        Assert.Equal("RS256", header.RootElement.GetProperty("alg").GetString());
        Assert.Equal("JWT", header.RootElement.GetProperty("typ").GetString());

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        Assert.Equal("123456", payload.RootElement.GetProperty("iss").GetString());
        Assert.Equal(Now.AddSeconds(-60).ToUnixTimeSeconds(), payload.RootElement.GetProperty("iat").GetInt64());
        Assert.Equal(Now.AddMinutes(9).ToUnixTimeSeconds(), payload.RootElement.GetProperty("exp").GetInt64());
        Assert.True(payload.RootElement.GetProperty("exp").GetInt64() - payload.RootElement.GetProperty("iat").GetInt64() <= 600);

        var signed = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
        Assert.True(rsa.VerifyData(signed, Base64UrlDecode(parts[2]), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void Private_key_loads_from_pem()
    {
        using var source = RSA.Create(2048);
        var pem = source.ExportRSAPrivateKeyPem();

        using var loaded = GitHubAppJwt.LoadPrivateKey(pem);

        Assert.Equal(source.ExportParameters(false).Modulus, loaded.ExportParameters(false).Modulus);
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}

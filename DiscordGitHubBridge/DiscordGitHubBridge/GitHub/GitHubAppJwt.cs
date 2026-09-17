using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DiscordGitHubBridge.GitHub;

/// <summary>
/// Builds the RS256 JSON Web Token a GitHub App presents to mint installation tokens.
/// Three claims and one signature; no dependency needed.
/// </summary>
public static class GitHubAppJwt
{
    /// <summary>GitHub rejects tokens valid for more than 10 minutes. Stay under it and back-date iat for clock skew.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(9);
    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(60);

    public static string Create(long appId, RSA privateKey, DateTimeOffset now)
    {
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "RS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iat = (now - ClockSkew).ToUnixTimeSeconds(),
            exp = (now + Lifetime).ToUnixTimeSeconds(),
            iss = appId.ToString(System.Globalization.CultureInfo.InvariantCulture),
        }));

        var signingInput = $"{header}.{payload}";
        var signature = privateKey.SignData(Encoding.ASCII.GetBytes(signingInput), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{signingInput}.{Base64Url(signature)}";
    }

    public static RSA LoadPrivateKey(string pem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return rsa;
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

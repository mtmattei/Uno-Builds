using System.ComponentModel.DataAnnotations;

namespace DiscordGitHubBridge.Configuration;

public enum GitHubAuthMode
{
    App,
    Pat,
}

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    [Required(AllowEmptyStrings = false)]
    public string Owner { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Repository { get; set; } = string.Empty;

    [Required]
    public GitHubAuthOptions Auth { get; set; } = new();
}

public sealed class GitHubAuthOptions
{
    public GitHubAuthMode Mode { get; set; } = GitHubAuthMode.App;

    /// <summary>Personal access token. Pat mode only, local development only.</summary>
    public string? Token { get; set; }

    public long AppId { get; set; }

    /// <summary>Optional. When unset, the bridge asks GitHub which installation covers Owner/Repository.</summary>
    public long InstallationId { get; set; }

    /// <summary>Path to the GitHub App private key (.pem). Alternative to <see cref="PrivateKeyPem"/>.</summary>
    public string? PrivateKeyPath { get; set; }

    /// <summary>Inline PEM contents of the GitHub App private key. Alternative to <see cref="PrivateKeyPath"/>.</summary>
    public string? PrivateKeyPem { get; set; }
}

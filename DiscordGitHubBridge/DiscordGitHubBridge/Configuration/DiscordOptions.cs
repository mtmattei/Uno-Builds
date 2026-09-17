using System.ComponentModel.DataAnnotations;

namespace DiscordGitHubBridge.Configuration;

public sealed class DiscordOptions
{
    public const string SectionName = "Discord";

    [Required(AllowEmptyStrings = false)]
    public string Token { get; set; } = string.Empty;
}

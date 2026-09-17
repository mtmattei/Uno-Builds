using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Configuration;

public sealed class BridgeOptionsValidator : IValidateOptions<BridgeOptions>
{
    public ValidateOptionsResult Validate(string? name, BridgeOptions options)
    {
        var failures = new List<string>();

        if (options.Forums.Count == 0)
        {
            failures.Add("Forums must contain at least one forum rule.");
        }

        var seen = new HashSet<ulong>();
        foreach (var forum in options.Forums)
        {
            if (forum.ChannelId == 0)
            {
                failures.Add("Forums[].ChannelId must be a non-zero Discord channel ID.");
            }
            else if (!seen.Add(forum.ChannelId))
            {
                failures.Add($"Forums[].ChannelId {forum.ChannelId} is listed more than once.");
            }
        }

        if (options.Behavior.ReplyInThread && string.IsNullOrWhiteSpace(options.Behavior.ReplyTemplate))
        {
            failures.Add("Behavior.ReplyTemplate must not be empty when Behavior.ReplyInThread is true.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}

public sealed class GitHubOptionsValidator : IValidateOptions<GitHubOptions>
{
    public ValidateOptionsResult Validate(string? name, GitHubOptions options)
    {
        var failures = new List<string>();
        var auth = options.Auth;

        switch (auth.Mode)
        {
            case GitHubAuthMode.Pat:
                if (string.IsNullOrWhiteSpace(auth.Token))
                {
                    failures.Add("GitHub.Auth.Token is required when GitHub.Auth.Mode is Pat.");
                }
                break;

            case GitHubAuthMode.App:
                if (auth.AppId <= 0)
                {
                    failures.Add("GitHub.Auth.AppId is required when GitHub.Auth.Mode is App.");
                }
                if (auth.InstallationId < 0)
                {
                    failures.Add("GitHub.Auth.InstallationId must be positive when set. Leave it unset to let the bridge find it.");
                }
                if (string.IsNullOrWhiteSpace(auth.PrivateKeyPath) && string.IsNullOrWhiteSpace(auth.PrivateKeyPem))
                {
                    failures.Add("GitHub.Auth.PrivateKeyPath or GitHub.Auth.PrivateKeyPem is required when GitHub.Auth.Mode is App.");
                }
                else if (!string.IsNullOrWhiteSpace(auth.PrivateKeyPath) && !File.Exists(auth.PrivateKeyPath))
                {
                    failures.Add($"GitHub.Auth.PrivateKeyPath '{auth.PrivateKeyPath}' does not exist.");
                }
                break;

            default:
                failures.Add($"GitHub.Auth.Mode '{auth.Mode}' is not supported.");
                break;
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}

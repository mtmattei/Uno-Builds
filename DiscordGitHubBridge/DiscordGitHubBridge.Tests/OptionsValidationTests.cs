using DiscordGitHubBridge.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Tests;

public class OptionsValidationTests
{
    private static ServiceProvider Build(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddBridgeOptions(configuration);
        return services.BuildServiceProvider();
    }

    private static Dictionary<string, string?> ValidAppConfig() => new()
    {
        ["Discord:Token"] = "token",
        ["GitHub:Owner"] = "owner",
        ["GitHub:Repository"] = "repo",
        ["GitHub:Auth:Mode"] = "App",
        ["GitHub:Auth:AppId"] = "123",
        ["GitHub:Auth:InstallationId"] = "456",
        ["GitHub:Auth:PrivateKeyPem"] = "-----BEGIN RSA PRIVATE KEY-----",
        ["Forums:0:ChannelId"] = "1547279690664771605",
        ["Forums:0:RequiredTags:0"] = "Track on GitHub",
        ["Forums:0:DefaultLabels:0"] = "from-discord",
    };

    [Fact]
    public void Valid_app_configuration_binds_and_validates()
    {
        using var provider = Build(ValidAppConfig());

        var bridge = provider.GetRequiredService<IOptions<BridgeOptions>>().Value;
        var github = provider.GetRequiredService<IOptions<GitHubOptions>>().Value;
        var discord = provider.GetRequiredService<IOptions<DiscordOptions>>().Value;

        Assert.Single(bridge.Forums);
        Assert.Equal(1547279690664771605UL, bridge.Forums[0].ChannelId);
        Assert.Equal(["Track on GitHub"], bridge.Forums[0].RequiredTags);
        Assert.Equal(GitHubAuthMode.App, github.Auth.Mode);
        Assert.Equal("token", discord.Token);
        Assert.True(bridge.Behavior.ReplyInThread);
    }

    [Fact]
    public void Missing_discord_token_fails()
    {
        var config = ValidAppConfig();
        config.Remove("Discord:Token");
        using var provider = Build(config);

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DiscordOptions>>().Value);
        Assert.Contains("Token", ex.Message);
    }

    [Fact]
    public void Empty_forums_fails()
    {
        var config = ValidAppConfig();
        foreach (var key in config.Keys.Where(k => k.StartsWith("Forums", StringComparison.Ordinal)).ToList())
        {
            config.Remove(key);
        }
        using var provider = Build(config);

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<BridgeOptions>>().Value);
        Assert.Contains("at least one forum", ex.Message);
    }

    [Fact]
    public void Duplicate_channel_ids_fail()
    {
        var config = ValidAppConfig();
        config["Forums:1:ChannelId"] = "1547279690664771605";
        using var provider = Build(config);

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<BridgeOptions>>().Value);
        Assert.Contains("more than once", ex.Message);
    }

    [Fact]
    public void App_mode_without_key_fails()
    {
        var config = ValidAppConfig();
        config.Remove("GitHub:Auth:PrivateKeyPem");
        using var provider = Build(config);

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<GitHubOptions>>().Value);
        Assert.Contains("PrivateKeyPath", ex.Message);
    }

    [Fact]
    public void Pat_mode_requires_token_only()
    {
        var config = ValidAppConfig();
        config["GitHub:Auth:Mode"] = "Pat";
        config.Remove("GitHub:Auth:AppId");
        config.Remove("GitHub:Auth:InstallationId");
        config.Remove("GitHub:Auth:PrivateKeyPem");
        using var provider = Build(config);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<GitHubOptions>>().Value);

        config["GitHub:Auth:Token"] = "ghp_x";
        using var okProvider = Build(config);
        Assert.Equal(GitHubAuthMode.Pat, okProvider.GetRequiredService<IOptions<GitHubOptions>>().Value.Auth.Mode);
    }
}

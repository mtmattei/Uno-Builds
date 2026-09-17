using Microsoft.Extensions.Options;

namespace DiscordGitHubBridge.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBridgeOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DiscordOptions>()
            .Bind(configuration.GetSection(DiscordOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GitHubOptions>()
            .Bind(configuration.GetSection(GitHubOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<GitHubOptions>, GitHubOptionsValidator>();

        services.AddOptions<BridgeOptions>()
            .Bind(configuration)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<BridgeOptions>, BridgeOptionsValidator>();

        return services;
    }
}

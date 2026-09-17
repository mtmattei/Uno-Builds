using DiscordGitHubBridge.Configuration;
using DiscordGitHubBridge.Data;
using DiscordGitHubBridge.GitHub;
using DiscordGitHubBridge.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBridgeOptions(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContextFactory<BridgeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Bridge") ?? "Data Source=bridge.db"));
builder.Services.AddSingleton<IMappingRepository, MappingRepository>();
builder.Services.AddHostedService<DatabaseStartupService>();

builder.Services.AddSingleton<IInstallationTokenExchanger, OctokitInstallationTokenExchanger>();
builder.Services.AddSingleton<IGitHubCredentialProvider>(sp =>
{
    var options = sp.GetRequiredService<IOptions<GitHubOptions>>();
    return options.Value.Auth.Mode == GitHubAuthMode.Pat
        ? new PatCredentialProvider(options)
        : ActivatorUtilities.CreateInstance<GitHubAppCredentialProvider>(sp);
});
builder.Services.AddSingleton<IGitHubIssueClient, OctokitIssueClient>();

// Discord services are registered in a later step.

var host = builder.Build();
host.Run();

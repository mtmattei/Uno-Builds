using DiscordGitHubBridge.Bridge;
using DiscordGitHubBridge.Configuration;
using Discord.WebSocket;
using DiscordGitHubBridge.Data;
using DiscordGitHubBridge.Discord;
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

builder.Services.AddSingleton<ThreadRuleEvaluator>();
builder.Services.AddSingleton<IssueFactory>();
builder.Services.AddSingleton<BridgeProcessor>();

builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = DiscordGatewayService.RequiredIntents,
    LogLevel = Discord.LogSeverity.Info,
    MessageCacheSize = 0,
});
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<ForumThreadReader>();
builder.Services.AddSingleton<ForumThreadHandler>();
builder.Services.AddSingleton<IDiscordReplier, DiscordThreadReplier>();
builder.Services.AddHostedService<DiscordGatewayService>();

var host = builder.Build();
host.Run();

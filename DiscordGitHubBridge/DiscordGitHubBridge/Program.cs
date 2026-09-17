using DiscordGitHubBridge.Configuration;
using DiscordGitHubBridge.Data;
using DiscordGitHubBridge.Mapping;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBridgeOptions(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContextFactory<BridgeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Bridge") ?? "Data Source=bridge.db"));
builder.Services.AddSingleton<IMappingRepository, MappingRepository>();
builder.Services.AddHostedService<DatabaseStartupService>();

// GitHub and Discord services are registered in later steps.

var host = builder.Build();
host.Run();

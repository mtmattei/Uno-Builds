using DiscordGitHubBridge.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBridgeOptions(builder.Configuration);

// Persistence, GitHub, and Discord services are registered in later steps.

var host = builder.Build();
host.Run();

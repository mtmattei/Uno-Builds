var builder = Host.CreateApplicationBuilder(args);

// Options, persistence, GitHub, and Discord services are registered in later steps.

var host = builder.Build();
host.Run();

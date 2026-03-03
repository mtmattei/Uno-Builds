using System.Text.Json.Serialization.Metadata;
using VTrack.DataContracts.Serialization;
using VTrack.Server.Apis;
using VTrack.Server.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure the JsonOptions to use the generated contexts
    builder.Services.Configure<JsonOptions>(options =>
        options.JsonSerializerOptions.TypeInfoResolver = JsonTypeInfoResolver.Combine(
            WeatherForecastContext.Default,
            VTrackContext.Default
        ));

    // Configure the RouteOptions to use lowercase URLs
    builder.Services.Configure<RouteOptions>(options =>
        options.LowercaseUrls = true);

    // Register services
    builder.Services.AddHttpClient<IRoboflowService, RoboflowService>();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        // Include XML comments for all included assemblies
        Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml")
            .Where(x => x.Contains("VTrack")
                && File.Exists(Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Path.GetFileNameWithoutExtension(x)}.dll")))
            .ToList()
            .ForEach(path => c.IncludeXmlComments(path));
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Serve uploaded files
    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    Directory.CreateDirectory(uploadsDir);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
        RequestPath = "/uploads"
    });

    // Uncomment when hosting WebAssembly app
    // app.UseUnoFrameworkFiles();
    // app.MapFallbackToFile("index.html");

    // Map API endpoints
    app.MapWeatherApi();
    app.MapVideoApi();
    app.MapTrackingApi();

    app.UseStaticFiles();

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine("Application terminated unexpectedly");
    Console.Error.WriteLine(ex);
#if DEBUG
    if (System.Diagnostics.Debugger.IsAttached)
    {
        System.Diagnostics.Debugger.Break();
    }
#endif
}

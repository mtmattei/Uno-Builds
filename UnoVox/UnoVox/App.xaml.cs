using Microsoft.Extensions.Configuration;
using Uno.Resizetizer;
using UnoVox.Services;
using UnoVox.Configuration;

namespace UnoVox;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    public static Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);

                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                        .Section<RoboflowConfig>()
                        .Section<HandTrackingConfig>()
                        .Section<WebcamConfig>()
                        .Section<VoxelEditorConfig>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                .UseHttp((context, services) => {
#if DEBUG
                // DelegatingHandler will be automatically injected
                services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif

})
                .ConfigureServices((context, services) =>
                {
                    // Register configuration options
                    services.Configure<HandTrackingConfig>(context.Configuration.GetSection("HandTracking"));
                    services.Configure<WebcamConfig>(context.Configuration.GetSection("Webcam"));
                    services.Configure<VoxelEditorConfig>(context.Configuration.GetSection("VoxelEditor"));

                    // Register infrastructure services
                    services.AddSingleton<DispatcherContext>();
                    services.AddSingleton<ThreadSafeFrameBuffer>();

                    // Register application services
                    services.AddSingleton<IProjectFileService, ProjectFileService>();
                    services.AddSingleton<IWebcamService, WebcamService>();
                    services.AddTransient<IHandTracker, OnnxHandTracker>();

                    // Register hand tracking services
                    services.AddSingleton<GestureDetector>();
                    services.AddSingleton<GestureStateMachine>();
                    services.AddSingleton<HandToVoxelMapper>();
                    services.AddSingleton<LandmarkSmoother>();

                    // Register Roboflow gesture classifier
                    // API key can be configured in appsettings.json or via ROBOFLOW_API_KEY environment variable
                    services.Configure<RoboflowConfig>(config =>
                    {
                        var section = context.Configuration.GetSection("Roboflow");
                        section.Bind(config);

                        // Allow environment variable override for API key (best practice for demos)
                        var envApiKey = Environment.GetEnvironmentVariable("ROBOFLOW_API_KEY");
                        if (!string.IsNullOrWhiteSpace(envApiKey))
                        {
                            config.ApiKey = envApiKey;
                        }
                    });
                    services.AddSingleton<IRoboflowGestureClassifier, RoboflowGestureClassifier>();

                    // Register ViewModels
                    services.AddTransient<VoxelEditorViewModel>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

        #if DEBUG
        MainWindow.UseStudio();
#endif
                MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainViewModel>(),
            new ViewMap<VoxelEditorPage, VoxelEditorViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainViewModel>(), IsDefault:true),
                    new ("VoxelEditor", View: views.FindByViewModel<VoxelEditorViewModel>())
                ]
            )
        );
    }
}

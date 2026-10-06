using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace SmartCity;

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

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    /// <summary>The app window, exposed so render controls can pause their loops when it deactivates.</summary>
    public static Window? MainWindowInstance { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
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
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    // Smart City Platform services (mock data backing the MVUX feeds)
                    services.AddSingleton<IEnergyService, MockEnergyService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;
        MainWindowInstance = MainWindow;

        // Decision: extended/custom title bar for the borderless console look. Deferred to M4
        // polish — it needs SetTitleBar drag-region handling; enabling it now hides the top nav
        // on desktop. Standard title bar for M2/M3 so the chrome stays visible.
        // TODO(M4): MainWindow.ExtendsContentIntoTitleBar = true + SetTitleBar drag regions.
        MainWindow.ExtendsContentIntoTitleBar = false;

        #if DEBUG
        MainWindow.UseStudio();
#endif
                MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new ViewMap<DashboardPage, EnergyDashboardModel>(),
            new ViewMap<ComingSoonPage, ComingSoonModel>()
        );

        // Shell -> Main chrome -> four top-nav destinations. Only Predictive AI is live
        // (DashboardPage); the others share the ComingSoon stub. Predictive AI is the default.
        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new RouteMap("Main", View: views.FindByViewModel<MainModel>(), IsDefault: true,
                        Nested:
                        [
                            new RouteMap("PredictiveAI", View: views.FindByViewModel<EnergyDashboardModel>(), IsDefault: true),
                            new RouteMap("Overview", View: views.FindByViewModel<ComingSoonModel>()),
                            new RouteMap("Monitoring", View: views.FindByViewModel<ComingSoonModel>()),
                            new RouteMap("Management", View: views.FindByViewModel<ComingSoonModel>()),
                        ]),
                ]
            )
        );
    }
}

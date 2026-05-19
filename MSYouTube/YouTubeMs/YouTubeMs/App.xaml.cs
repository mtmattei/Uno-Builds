using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;
using YouTubeMs.Presentation.Home;
using YouTubeMs.Presentation.Stubs;
using YouTubeMs.Services.Catalog;
using YouTubeMs.Services.Motion;

namespace YouTubeMs;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
        Motion.Initialize();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .UseHttp((context, services) => {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ICatalogService, MockCatalogService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // Extend content into the title bar for a chromeless look
        try
        {
            MainWindow.ExtendsContentIntoTitleBar = true;
        }
        catch
        {
            // not all targets support title bar extension
        }

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new ViewMap<HomePage, HomeModel>(),
            new ViewMap<ExplorePage, ExploreModel>(),
            new ViewMap<SubscriptionsPage, SubscriptionsModel>(),
            new ViewMap<LibraryPage, LibraryModel>(),
            new ViewMap<HistoryPage, HistoryModel>(),
            new ViewMap<WatchLaterPage, WatchLaterModel>(),
            new ViewMap<LikedPage, LikedModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new RouteMap("Main", View: views.FindByViewModel<MainModel>(), IsDefault: true,
                        Nested:
                        [
                            new RouteMap("Home", View: views.FindByViewModel<HomeModel>(), IsDefault: true),
                            new RouteMap("Explore", View: views.FindByViewModel<ExploreModel>()),
                            new RouteMap("Subscriptions", View: views.FindByViewModel<SubscriptionsModel>()),
                            new RouteMap("Library", View: views.FindByViewModel<LibraryModel>()),
                            new RouteMap("History", View: views.FindByViewModel<HistoryModel>()),
                            new RouteMap("WatchLater", View: views.FindByViewModel<WatchLaterModel>()),
                            new RouteMap("Liked", View: views.FindByViewModel<LikedModel>()),
                        ]
                    ),
                ]
            )
        );
    }
}

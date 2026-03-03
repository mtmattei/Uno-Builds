using Uno.Resizetizer;
using SpaceXhistory.ViewModels;
using SpaceXhistory.Helpers;

namespace SpaceXhistory;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

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
                )
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient<HomePageViewModel>(client =>
                    {
                        client.BaseAddress = new Uri(Constants.BaseUrl);
                    });
                    services.AddHttpClient<PastLaunchesViewModel>(client =>
                    {
                        client.BaseAddress = new Uri(Constants.BaseUrl);
                    });
                    services.AddHttpClient<UpcomingLaunchesViewModel>(client =>
                    {
                        client.BaseAddress = new Uri(Constants.BaseUrl);
                    });
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Views.ShellPage>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap<Views.ShellPage>(),
            new ViewMap<Views.HomePage, HomePageViewModel>(),
            new ViewMap<Views.PastLaunchesPage, PastLaunchesViewModel>(),
            new ViewMap<Views.UpcomingLaunchesPage, UpcomingLaunchesViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByView<Views.ShellPage>(),
                Nested:
                [
                    new ("Home", View: views.FindByViewModel<HomePageViewModel>(), IsDefault: true),
                    new ("PastLaunches", View: views.FindByViewModel<PastLaunchesViewModel>()),
                    new ("UpcomingLaunches", View: views.FindByViewModel<UpcomingLaunchesViewModel>()),
                ]
            )
        );
    }
}

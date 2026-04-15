using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace PhosphorProtocol;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    internal IHost? Host { get; private set; }

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
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IVehicleService, MockVehicleService>();
                    services.AddSingleton<IClimateService, MockClimateService>();
                    services.AddSingleton<IMediaService, MockMediaService>();
                    services.AddSingleton<INavigationDataService, MockNavigationDataService>();
                    services.AddSingleton<IEnergyService, MockEnergyService>();
                    services.AddSingleton<IChargeService, MockChargeService>();
                    services.AddSingleton<IAutopilotService, MockAutopilotService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
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
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<BootPage>(),
            new ViewMap<DashboardShell, DashboardModel>(),
            new ViewMap<NavView, NavModel>(),
            new ViewMap<MediaView, MediaModel>(),
            new ViewMap<EnergyView, EnergyModel>(),
            new ViewMap<ChargeView, ChargeModel>(),
            new ViewMap<ControlsView, ControlsModel>(),
            new ViewMap<AutopilotView, AutopilotModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new("Boot", View: views.FindByView<BootPage>(), IsDefault: true),
                    new("Dashboard", View: views.FindByView<DashboardShell>(),
                        Nested:
                        [
                            new("Nav", View: views.FindByViewModel<NavModel>(), IsDefault: true),
                            new("Media", View: views.FindByViewModel<MediaModel>()),
                            new("Energy", View: views.FindByViewModel<EnergyModel>()),
                            new("Charge", View: views.FindByViewModel<ChargeModel>()),
                            new("Controls", View: views.FindByViewModel<ControlsModel>()),
                            new("Autopilot", View: views.FindByViewModel<AutopilotModel>()),
                        ]),
                ]
            )
        );
    }
}

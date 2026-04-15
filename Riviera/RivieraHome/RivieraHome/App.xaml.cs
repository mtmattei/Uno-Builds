using System.Diagnostics.CodeAnalysis;
using RivieraHome.Services;
using Uno.Resizetizer;

namespace RivieraHome;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Console.WriteLine("[RivieraHome] OnLaunched starting...");

            var builder = this.CreateBuilder(args)
                .UseToolkitNavigation()
                .Configure(host => host
#if DEBUG
                    .UseEnvironment(Environments.Development)
#endif
                    .UseLogging(configure: (context, logBuilder) =>
                    {
                        logBuilder
                            .SetMinimumLevel(LogLevel.Debug)
                            .CoreLogLevel(LogLevel.Warning)
                            .AddConsole();
                    }, enableUnoLogging: true)
                    .UseConfiguration(configure: configBuilder =>
                        configBuilder
                            .EmbeddedSource<App>()
                            .Section<AppConfig>()
                    )
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<ISmartHomeService, MockSmartHomeService>();
                    })
                    .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
                );

            Console.WriteLine("[RivieraHome] Builder configured, getting window...");
            MainWindow = builder.Window;

#if DEBUG
            MainWindow.UseStudio();
#endif
            MainWindow.SetWindowIcon();

            Console.WriteLine("[RivieraHome] Navigating to Shell...");
            Host = await builder.NavigateAsync<Shell>();
            Console.WriteLine("[RivieraHome] Navigation complete.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[RivieraHome] FATAL: {ex}");
            throw;
        }
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new ViewMap<ClimatePage, ClimateModel>(),
            new ViewMap<SecurityPage, SecurityModel>(),
            new ViewMap<EnergyPage, EnergyModel>(),
            new ViewMap<LightingPage, LightingModel>(),
            new ViewMap<DiagnosticsPage, DiagnosticsModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new("Main", View: views.FindByViewModel<MainModel>(), IsDefault: true,
                        Nested:
                        [
                            new("Climate", View: views.FindByViewModel<ClimateModel>(), IsDefault: true),
                            new("Security", View: views.FindByViewModel<SecurityModel>()),
                            new("Energy", View: views.FindByViewModel<EnergyModel>()),
                            new("Lighting", View: views.FindByViewModel<LightingModel>()),
                            new("Diagnostics", View: views.FindByViewModel<DiagnosticsModel>()),
                        ]),
                ]
            )
        );
    }
}

using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace Orbital;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public Window? AppWindow => MainWindow;
    public IHost? Host { get; private set; }

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
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IProjectContext, ProjectContextService>();
                    services.AddSingleton<IClockService, ClockService>();
                    services.AddSingleton<IEnvironmentService, RuntimeEnvironmentService>();
                    services.AddSingleton<IStudioService, RuntimeStudioService>();
                    services.AddSingleton<IAgentService, RuntimeAgentService>();
                    services.AddSingleton<IBuildService, RuntimeBuildService>();
                    services.AddSingleton<IDiagnosticsService, RuntimeDiagnosticsService>();
                    services.AddSingleton<IMcpService, RuntimeMcpService>();
                    services.AddSingleton<ISkillsService, RuntimeSkillsService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

#if HAS_UNO
        Uno.UI.Xaml.WindowHelper.SetBackground(
            MainWindow,
            new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x0A, 0x0A, 0x0B)));
#endif

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new ViewMap<HomePage, HomeModel>(),
            new ViewMap<ProjectPage, ProjectModel>(),
            new ViewMap<AgentsPage, AgentsModel>(),
            new ViewMap<StudioPage, StudioModel>(),
            new ViewMap<DiagnosticsPage, DiagnosticsModel>(),
            new ViewMap<SkillsPage, SkillsModel>(),
            new ViewMap<SettingsPage, SettingsModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new RouteMap("Main", View: views.FindByViewModel<MainModel>(),
                        IsDefault: true,
                        Nested:
                        [
                            new RouteMap("Home", View: views.FindByViewModel<HomeModel>(), IsDefault: true),
                            new RouteMap("Project", View: views.FindByViewModel<ProjectModel>()),
                            new RouteMap("Agents", View: views.FindByViewModel<AgentsModel>()),
                            new RouteMap("Studio", View: views.FindByViewModel<StudioModel>()),
                            new RouteMap("Skills", View: views.FindByViewModel<SkillsModel>()),
                            new RouteMap("Diagnostics", View: views.FindByViewModel<DiagnosticsModel>()),
                            new RouteMap("Settings", View: views.FindByViewModel<SettingsModel>()),
                        ])
                ])
        );
    }
}

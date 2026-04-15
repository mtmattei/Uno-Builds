using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace ClaudeDash;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public new static App Current => (App)Application.Current;
    public IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
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
                        .Section<Helpers.SupabaseConfig>()
                )
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .UseThemeSwitching()
                .UseNavigation(RegisterRoutes)
                .ConfigureServices((context, services) =>
                {
                    // Core services
                    services.AddSingleton<Services.IClaudeConfigService, Services.ClaudeConfigService>();
                    services.AddSingleton<Services.ISupabaseService, Services.SupabaseService>();
                    services.AddSingleton<Services.IBackgroundScannerService, Services.BackgroundScannerService>();
                    services.AddSingleton<Services.ISearchIndexService, Services.SearchIndexService>();

                    // Feature services
                    services.AddSingleton<Services.IWorktreeService, Services.WorktreeService>();
                    services.AddSingleton<Services.IProjectScannerService, Services.ProjectScannerService>();
                    services.AddSingleton<Services.IAgentLauncherService, Services.AgentLauncherService>();
                    services.AddSingleton<Services.ISessionParserService, Services.SessionParserService>();
                    services.AddSingleton<Services.IRemediationService, Services.RemediationService>();
                    services.AddSingleton<Services.IChatService, Services.ChatService>();

                    // Navigation (legacy, used by SearchOverlay)
                    services.AddSingleton<Services.INavigationService, Services.NavigationService>();

                    // SlideOver service
                    services.AddSingleton<Services.SlideOverService>();
                    services.AddSingleton<Services.ISlideOverService>(sp => sp.GetRequiredService<Services.SlideOverService>());
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = builder.Build();

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        // Enable Uno Navigation Extensions on root frame so ViewMap sets DataContext
        Uno.Extensions.Navigation.UI.Region.SetAttached(rootFrame, true);

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(Views.ShellPage), args.Arguments);
        }
        MainWindow.Activate();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap<Views.ShellPage, ViewModels.ShellModel>(),
            new ViewMap<Views.HomePage, ViewModels.HomeModel>(),
            new ViewMap<Views.ChatPage, ViewModels.ChatModel>(),
            new ViewMap<Views.SessionsPage, ViewModels.SessionsModel>(),
            new ViewMap<Views.ProjectsPage, ViewModels.ProjectsModel>(),
            new ViewMap<Views.UnoPlatformOverviewPage, ViewModels.UnoPlatformOverviewModel>(),
            new ViewMap<Views.RemediationPage, ViewModels.EnvAuditModel>(),
            new ViewMap<Views.McpSkillsPage, ViewModels.McpHealthModel>(),
            new ViewMap<Views.HooksMemoryPage, ViewModels.HooksModel>(),
            new ViewMap<Views.CostsPage, ViewModels.CostsModel>(),
            new ViewMap<Views.SettingsPage, ViewModels.SettingsModel>(),
            new ViewMap<Views.SessionReplayPage, ViewModels.SessionReplayModel>(),
            new ViewMap<Views.RalphLoopsPage, ViewModels.RalphLoopsModel>()
        );

        routes.Register(
            new RouteMap("Shell", View: views.FindByViewModel<ViewModels.ShellModel>(),
                Nested: new RouteMap[]
                {
                    new("home", View: views.FindByView<Views.HomePage>()),
                    new("chat", View: views.FindByView<Views.ChatPage>()),
                    new("sessions", View: views.FindByView<Views.SessionsPage>()),
                    new("projects", View: views.FindByView<Views.ProjectsPage>()),
                    new("uno-platform", View: views.FindByView<Views.UnoPlatformOverviewPage>()),
                    new("ralph-loops", View: views.FindByView<Views.RalphLoopsPage>()),
                    new("hygiene", View: views.FindByView<Views.RemediationPage>()),
                    new("mcp-skills", View: views.FindByView<Views.McpSkillsPage>()),
                    new("hooks-memory", View: views.FindByView<Views.HooksMemoryPage>()),
                    new("costs", View: views.FindByView<Views.CostsPage>()),
                    new("settings", View: views.FindByView<Views.SettingsPage>()),
                    new("session-replay", View: views.FindByView<Views.SessionReplayPage>()),
                })
        );
    }
}

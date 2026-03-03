using AgentNotifier.Services;
using AgentNotifier.ViewModels;
using Microsoft.UI.Windowing;
using Uno.Resizetizer;

namespace AgentNotifier;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;
    public IServiceProvider Services => Host!.Services;

    public Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }
    private SystemTrayService? _trayService;

    public App()
    {
        this.InitializeComponent();
    }

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
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IAgentStatusService, AgentStatusService>();
                    services.AddSingleton<IAudioService, AudioService>();
                    services.AddSingleton<SystemTrayService>();
                    services.AddSingleton<MainViewModel>();
                })
            );

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = builder.Build();

        ConfigureWindow();

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        MainWindow.Activate();

        // Initialize system tray
        InitializeSystemTray();
    }

    private void InitializeSystemTray()
    {
        if (MainWindow == null) return;

        _trayService = Services.GetService<SystemTrayService>();
        _trayService?.Initialize(MainWindow);

        // Handle window closing - minimize to tray instead of exiting
        MainWindow.AppWindow.Closing += OnWindowClosing;
    }

    private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // Cancel the close and minimize to tray instead
        args.Cancel = true;
        _trayService?.HideWindow();
    }

    private void ConfigureWindow()
    {
        if (MainWindow == null) return;

        MainWindow.Title = "Agent Notifier";

        try
        {
            // Extend content into title bar for custom title bar
            MainWindow.ExtendsContentIntoTitleBar = true;

            var appWindow = MainWindow.AppWindow;
            if (appWindow != null)
            {
                // Initial size - will be adjusted dynamically based on agent count
                appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 720, Height = 280 });

                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsAlwaysOnTop = true;
                    presenter.IsResizable = false;
                    presenter.IsMinimizable = true;
                    presenter.IsMaximizable = false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Window configuration failed: {ex.Message}");
        }
    }
}

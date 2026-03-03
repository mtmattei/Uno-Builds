using System.Diagnostics.CodeAnalysis;
using EnterpriseDashboard.Services;
using EnterpriseDashboard.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Uno.Resizetizer;

namespace EnterpriseDashboard;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configure LiveCharts2 dark theme
        LiveCharts.Configure(config =>
            config.AddSkiaSharp()
                  .AddDefaultMappers()
                  .AddDarkTheme()
        );

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
                    services.AddSingleton<IDashboardService, MockDashboardService>();
                    services.AddSingleton<IObservatoryService, MockObservatoryService>();
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

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(ShellPage), args.Arguments);
        }

        // Force dark theme for the analytics dashboard
        if (MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = ElementTheme.Dark;
        }

        MainWindow.Activate();
    }
}

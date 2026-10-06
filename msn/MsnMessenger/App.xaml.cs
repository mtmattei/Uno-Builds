using MsnMessenger.Services;
using MsnMessenger.Views;
using Uno.Resizetizer;

namespace MsnMessenger;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    public static IServiceProvider? Services { get; private set; }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

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
                    services.AddSingleton<IMsnDataService, MsnDataService>();
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        var appWindow = MainWindow.AppWindow;
        if (appWindow is not null)
        {
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1100, Height = 780 });

            if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
            }
        }

        Host = builder.Build();
        Services = Host.Services;

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content is null)
        {
            rootFrame.Navigate(typeof(OnboardingPage), args.Arguments);
        }

        MainWindow.Activate();
    }
}

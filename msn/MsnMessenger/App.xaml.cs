using MsnMessenger.Services;
using MsnMessenger.ViewModels;
using MsnMessenger.Views;
using Uno.Resizetizer;

namespace MsnMessenger;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

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
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<ProfileViewModel>();
                    services.AddTransient<ChatViewModel>();
                    services.AddTransient<SettingsViewModel>();
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // Set window size constraints (less restrictive)
        var appWindow = MainWindow.AppWindow;
        if (appWindow != null)
        {
            // Set initial size (mobile-ish but larger)
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 480, Height = 820 });

            // Set minimum size constraints
            var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
            }
        }

        Host = builder.Build();

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            // Start with onboarding flow
            rootFrame.Navigate(typeof(OnboardingPage), args.Arguments);
        }

        MainWindow.Activate();
    }
}

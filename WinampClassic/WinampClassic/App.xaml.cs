using Uno.Resizetizer;
using WinampClassic.ViewModels;
using Microsoft.UI.Windowing;

namespace WinampClassic;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }
    public static IHost? AppHost { get; private set; }

    /// <summary>
    /// Convenience accessor for the singleton PlayerViewModel via DI.
    /// </summary>
    public static PlayerViewModel? PlayerViewModel =>
        AppHost?.Services.GetService<PlayerViewModel>();

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
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<PlayerViewModel>();
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        AppHost = builder.Build();

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

        // Dispose MediaPlayer on window close
        if (MainWindow is not null)
        {
            MainWindow.Closed += (_, _) =>
            {
                PlayerViewModel?.Dispose();
            };
        }
    }

    private void ConfigureWindow()
    {
        if (MainWindow == null) return;

        var appWindow = MainWindow.AppWindow;
        if (appWindow != null)
        {
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 620, Height = 300 });
            appWindow.Title = "Winamp Classic";

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }
        }
    }
}

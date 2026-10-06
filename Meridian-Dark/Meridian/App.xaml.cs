using System.Diagnostics.CodeAnalysis;
using Meridian.Services;
using Meridian.Views;
using Uno.Resizetizer;

namespace Meridian;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    public static IServiceProvider Services { get; private set; } = null!;
    public new Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

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
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IMarketDataService, MockMarketDataService>();
                    var apiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY") ?? "";
                    services.AddSingleton(sp =>
                        new FinnhubService(apiKey, ["AAPL", "NVDA", "MSFT", "GOOGL", "META", "TSLA"]));
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = builder.Build();
        Services = Host.Services;

        // Set desktop window size for 1400px + padding layout
        MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1500, Height = 900 });

        ApplyDarkTitleBar(MainWindow);

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(DashboardPage), args.Arguments);
        }

        MainWindow.Activate();
    }

    /// <summary>
    /// Paints the system title bar in the ink palette. Without this the default
    /// light chrome sits directly above the dark canvas and reads as a seam.
    /// Title bar customization is best-effort per platform, so failures are
    /// non-fatal — the app just keeps the system default.
    /// </summary>
    private static void ApplyDarkTitleBar(Window window)
    {
        try
        {
            var ink = Windows.UI.Color.FromArgb(0xFF, 0x15, 0x13, 0x0F);
            var lift = Windows.UI.Color.FromArgb(0xFF, 0x24, 0x20, 0x19);
            var cream = Windows.UI.Color.FromArgb(0xFF, 0xF2, 0xED, 0xE3);
            var muted = Windows.UI.Color.FromArgb(0xFF, 0x9C, 0x91, 0x7C);

            var titleBar = window.AppWindow.TitleBar;
            titleBar.BackgroundColor = ink;
            titleBar.InactiveBackgroundColor = ink;
            titleBar.ForegroundColor = cream;
            titleBar.InactiveForegroundColor = muted;
            titleBar.ButtonBackgroundColor = ink;
            titleBar.ButtonInactiveBackgroundColor = ink;
            titleBar.ButtonForegroundColor = cream;
            titleBar.ButtonInactiveForegroundColor = muted;
            titleBar.ButtonHoverBackgroundColor = lift;
            titleBar.ButtonHoverForegroundColor = cream;
            titleBar.ButtonPressedBackgroundColor = lift;
            titleBar.ButtonPressedForegroundColor = cream;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Title bar theming unavailable: {ex.Message}");
        }

        // AppWindow.TitleBar colors are a no-op on Skia desktop, so on Windows
        // ask DWM to draw the frame in dark mode instead.
        TryEnableImmersiveDarkFrame(window);
    }

    private const int DwmwaUseImmersiveDarkMode = 20;

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int value, int size);

    private static void TryEnableImmersiveDarkFrame(Window window)
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            if (hwnd == IntPtr.Zero)
                return;

            var enabled = 1;
            DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dark window frame unavailable: {ex.Message}");
        }
    }
}

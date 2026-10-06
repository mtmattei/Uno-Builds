using System.Diagnostics.CodeAnalysis;
using FreewriteUno.InlineAi.Services;
using FreewriteUno.Services;
using FreewriteUno.ViewModels;
using Microsoft.UI.Xaml;
using Uno.Resizetizer;

namespace FreewriteUno;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Window? MainAppWindow { get; private set; }

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IEntryStore>(_ => new EntryStore());
                    services.AddSingleton<ISettingsStore, SettingsStore>();
                    services.AddSingleton<IPdfExporter, QuestPdfExporter>();
                    services.AddSingleton<MainViewModel>();

                    // Inline AI (P1 seams). FakeAiService backs the prototype; a real
                    // streaming backend swaps in behind IAiService at P6. EditorBridge
                    // mediates the model<->TextBox apply/selection seam (host wired at P5).
                    services.AddSingleton<IAiService, FakeAiService>();
                    services.AddSingleton<IClipboardService, ClipboardService>();
                    services.AddSingleton<IEditorBridge, EditorBridge>();
                })
            );
        MainWindow = builder.Window;
        MainAppWindow = MainWindow;

        #if DEBUG
        MainWindow.UseStudio();
#endif
                MainWindow.SetWindowIcon();

        Host = builder.Build();
        Services = Host.Services;

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (MainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        MainWindow.Closed += OnMainWindowClosed;

        // Ensure the current window is active
        MainWindow.Activate();
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        // Best-effort flush of any pending debounced save on shutdown.
        // Resolve via the service provider so we don't depend on MainPage being alive.
        try
        {
            var vm = Services.GetService<MainViewModel>();
            vm?.FlushPendingSaveAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Closing — nothing useful we can surface here.
        }
    }
}

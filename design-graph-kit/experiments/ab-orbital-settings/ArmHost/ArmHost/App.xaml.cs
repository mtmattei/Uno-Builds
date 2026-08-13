using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Uno.Resizetizer;

namespace ArmHost;

/// <summary>
/// Host shell for the four A/B implementation arms.
///
/// The arm to display is chosen by command line argument so each arm renders
/// alone, with no launcher chrome in the frame to confound a visual comparison:
///
///     ArmHost.exe A     -> AbExperiment.A.SettingsPage   (baseline: brief only)
///     ArmHost.exe B     -> AbExperiment.B.SettingsPage   (brief + design graph)
///     ArmHost.exe C     -> AbExperiment.C.SettingsPage   (brief + prose notes)
///     ArmHost.exe B2    -> AbExperiment.B2.SettingsPage  (brief + graph + Uno MCP)
///
/// With no argument the host exits non-zero rather than silently picking one,
/// so a mistyped capture run fails loudly instead of quietly producing a
/// screenshot of the wrong arm.
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }

    private static Type ResolveArmPage(string arm) => arm.ToUpperInvariant() switch
    {
        "A" => typeof(AbExperiment.A.SettingsPage),
        "B" => typeof(AbExperiment.B.SettingsPage),
        "C" => typeof(AbExperiment.C.SettingsPage),
        "B2" => typeof(AbExperiment.B2.SettingsPage),
        _ => throw new ArgumentException($"Unknown arm '{arm}'. Expected one of: A, B, C, B2."),
    };

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Environment args rather than args.Arguments: on desktop the latter is
        // empty for a plain `ArmHost.exe A` launch.
        var arm = Environment.GetCommandLineArgs()
            .Skip(1)
            .FirstOrDefault(a => !a.StartsWith('-'));

        if (string.IsNullOrWhiteSpace(arm))
        {
            Console.Error.WriteLine("No arm specified. Usage: ArmHost.exe <A|B|C|B2>");
            Environment.Exit(2);
            return;
        }

        var pageType = ResolveArmPage(arm);

        MainWindow = new Window { Title = $"ArmHost - {arm.ToUpperInvariant()}" };

        var rootFrame = new Frame();
        MainWindow.Content = rootFrame;
        rootFrame.NavigationFailed += OnNavigationFailed;
        rootFrame.Navigate(pageType);

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Called by Platforms/Desktop/Program.cs before the app starts.
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}

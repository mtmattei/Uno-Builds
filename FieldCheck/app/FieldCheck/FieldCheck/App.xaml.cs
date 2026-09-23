using FieldCheck.Services;
using FieldCheck.ViewModels;
using FieldCheck.Views;
using Uno.Resizetizer;

namespace FieldCheck;

public partial class App : Application
{
    /// <summary>Optional data mode handed over by a platform head before launch (Android intent extra).</summary>
    public static string? PlatformDataMode { get; set; }

    public App()
    {
        this.InitializeComponent();
    }

    public static IServiceProvider Services { get; private set; } = default!;

    protected Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow ??= new Window { Title = "FieldCheck" };
        Services ??= ConfigureServices(MainWindow);

        if (MainWindow.Content is not ShellPage)
        {
            MainWindow.Content = new ShellPage();
        }

        MainWindow.SetWindowIcon();
        MainWindow.Activate();
    }

    private static ServiceProvider ConfigureServices(Window window)
    {
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
            "FieldCheck");
        var mode = DataModeParser.FromEnvironment(PlatformDataMode);

        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IFieldCheckRepository>(sp => new DataModeRepository(
            new JsonFieldCheckRepository(dataDirectory, new EmbeddedSeedSource(), sp.GetRequiredService<TimeProvider>()),
            mode));
        services.AddSingleton<IFilePickerService>(_ => new FilePickerService(() => window));

        services.AddSingleton(sp => new ShellViewModel(sp.GetRequiredService<INavigator>));
        services.AddSingleton(sp => new FrameNavigator(sp.GetRequiredService<ShellViewModel>(), sp.GetRequiredService<AssetsViewModel>));
        services.AddSingleton<INavigator>(sp => sp.GetRequiredService<FrameNavigator>());

        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<AssetsViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddTransient<AssetDetailViewModel>();
        services.AddTransient<NewInspectionViewModel>();
        services.AddTransient<InspectionSuccessViewModel>();

        return services.BuildServiceProvider();
    }
}

using Pens.Services;
using Uno.Resizetizer;

namespace Pens;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var appBuilder = this.CreateBuilder(args)
            .Configure(host => host
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .ConfigureServices((context, services) =>
                {
                    // Register services (swap to SupabaseService for production)
                    services.AddSingleton<ISupabaseService, MockSupabaseService>();
                    services.AddSingleton<IPlayerIdentityService, PlayerIdentityService>();

                    // Register ViewModels
                    services.AddTransient<ScheduleViewModel>();
                    services.AddTransient<ChatViewModel>();
                    services.AddTransient<BeersViewModel>();
                    services.AddTransient<DutiesViewModel>();
                    services.AddTransient<RosterViewModel>();
                }));

        MainWindow = appBuilder.Window;
        Host = appBuilder.Build();

        MainWindow.SetWindowIcon();
        ShowAppContent();
        MainWindow.Activate();
    }

    private void ShowAppContent()
    {
        var identity = Host!.Services.GetRequiredService<IPlayerIdentityService>();

        if (identity.IsLoggedIn)
        {
            MainWindow!.Content = new Presentation.Shell(Host.Services);
        }
        else
        {
            ShowPlayerPicker();
        }
    }

    private void ShowPlayerPicker()
    {
        var supabase = Host!.Services.GetRequiredService<ISupabaseService>();
        var identity = Host.Services.GetRequiredService<IPlayerIdentityService>();

        var viewModel = new Presentation.PlayerPickerViewModel(supabase, identity, () =>
        {
            // After player is selected, show the main shell
            MainWindow!.Content = new Presentation.Shell(Host.Services);
        });

        MainWindow!.Content = new Presentation.PlayerPickerPage(viewModel);
    }
}

public class AppConfig
{
    public SupabaseConfig? Supabase { get; set; }
}

public class SupabaseConfig
{
    public string? Url { get; set; }
    public string? AnonKey { get; set; }
}

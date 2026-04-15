using System.Diagnostics.CodeAnalysis;
using ReservoomUno.DbContexts;
using ReservoomUno.Exceptions;
using ReservoomUno.Models;
using ReservoomUno.Services.ReservationConflictValidators;
using ReservoomUno.Services.ReservationCreators;
using ReservoomUno.Services.ReservationProviders;
using ReservoomUno.Stores;
using ReservoomUno.ViewModels;
using Uno.Resizetizer;

namespace ReservoomUno;

public partial class App : Application
{
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
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
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
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    // Database
                    var dbPath = Path.Combine(
                        Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                        "reservoom.db");
                    var connectionString = $"Data Source={dbPath}";
                    services.AddSingleton<IReservoomDbContextFactory>(
                        new ReservoomDbContextFactory(connectionString));

                    // Services
                    services.AddSingleton<IReservationProvider, DatabaseReservationProvider>();
                    services.AddSingleton<IReservationCreator, DatabaseReservationCreator>();
                    services.AddSingleton<IReservationConflictValidator, DatabaseReservationConflictValidator>();

                    // Domain
                    services.AddTransient<ReservationBook>();
                    services.AddSingleton<Hotel>(sp =>
                    {
                        var hotelName = context.Configuration.GetValue<string>("HotelName") ?? "Reservoom";
                        return new Hotel(hotelName, sp.GetRequiredService<ReservationBook>());
                    });
                    services.AddSingleton<HotelStore>();

                    // ViewModels
                    services.AddTransient<ReservationListingViewModel>();
                    services.AddTransient<MakeReservationViewModel>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();

        // Ensure database is created
        var dbContextFactory = Host!.Services.GetRequiredService<IReservoomDbContextFactory>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.EnsureCreated();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<ReservationListingPage, ReservationListingViewModel>(),
            new ViewMap<MakeReservationPage, MakeReservationViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new ("Listing", View: views.FindByViewModel<ReservationListingViewModel>(), IsDefault: true),
                    new ("MakeReservation", View: views.FindByViewModel<MakeReservationViewModel>()),
                ]
            )
        );
    }
}

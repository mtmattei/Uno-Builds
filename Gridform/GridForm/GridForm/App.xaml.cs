using System.Diagnostics.CodeAnalysis;
using GridForm.Presentation.Dashboard;
using GridForm.Presentation.Orders;
using GridForm.Presentation.Placeholder;
using GridForm.Presentation.Warehouse;
using GridForm.Services.Impl;
using Uno.Resizetizer;

namespace GridForm;

public partial class App : Application
{
	public App()
	{
		this.InitializeComponent();
		// Force dark theme — GRIDFORM is a dark-only app
		this.RequestedTheme = ApplicationTheme.Dark;
	}

	protected Window? MainWindow { get; private set; }
	public IHost? Host { get; private set; }

	public new static App Current => (App)Application.Current;

	[SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming.")]
	protected override async void OnLaunched(LaunchActivatedEventArgs args)
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
							context.HostingEnvironment.IsDevelopment()
								? LogLevel.Information
								: LogLevel.Warning)
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
					services.AddSingleton<IProcurementService, InMemoryProcurementService>();
					services.AddSingleton<IWarehouseService, InMemoryWarehouseService>();
					services.AddSingleton<INotificationService, InMemoryNotificationService>();
					services.AddSingleton<IActivityService, InMemoryActivityService>();
				})
				.UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
			);

		MainWindow = builder.Window;

#if DEBUG
		MainWindow.UseStudio();
#endif
		MainWindow.SetWindowIcon();

		Host = await builder.NavigateAsync<Shell>();
	}

	private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
	{
		views.Register(
			new ViewMap(ViewModel: typeof(ShellModel)),
			new ViewMap<MainPage, MainModel>(),
			new ViewMap<DashboardPage, DashboardModel>(),
			new ViewMap<WarehousePage, WarehouseModel>(),
			new ViewMap<OrdersPage, OrdersModel>(),
			new DataViewMap<OrderDetailPage, OrderDetailModel, PurchaseOrder>(),
			new ViewMap<ComingSoonPage>()
		);

		routes.Register(
			new RouteMap("", View: views.FindByViewModel<ShellModel>(),
				Nested:
				[
					new RouteMap("Main", View: views.FindByViewModel<MainModel>(), IsDefault: true,
						Nested:
						[
							new RouteMap("Dashboard", View: views.FindByViewModel<DashboardModel>(), IsDefault: true),
							new RouteMap("Warehouse", View: views.FindByViewModel<WarehouseModel>()),
							new RouteMap("Orders", View: views.FindByViewModel<OrdersModel>(),
								Nested:
								[
									new RouteMap("OrderDetail", View: views.FindByViewModel<OrderDetailModel>())
								]),
							new RouteMap("Inventory", View: views.FindByView<ComingSoonPage>()),
							new RouteMap("Vendors", View: views.FindByView<ComingSoonPage>()),
						])
				])
		);
	}
}

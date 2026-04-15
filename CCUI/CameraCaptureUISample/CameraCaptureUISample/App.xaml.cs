using CameraCaptureUISample.Models;
using CameraCaptureUISample.Services;
using Microsoft.Extensions.Hosting;
using Uno.Resizetizer;

namespace CameraCaptureUISample;

public partial class App : Application
{
	internal IHost? Host { get; private set; }

	public App()
	{
		this.InitializeComponent();
	}

	protected Window? MainWindow { get; private set; }

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		var builder = this.CreateBuilder(args)
			.Configure(host =>
			{
				host.ConfigureServices((context, services) =>
				{
					services.AddSingleton<ICameraService, CameraService>();
					services.AddTransient<CaptureModel>();
				});
			});

		MainWindow = builder.Window;

#if DEBUG
		MainWindow.UseStudio();
#endif

		Host = builder.Build();

		if (MainWindow.Content is not Frame rootFrame)
		{
			rootFrame = new Frame();
			MainWindow.Content = rootFrame;
			rootFrame.NavigationFailed += OnNavigationFailed;
		}

		if (rootFrame.Content == null)
		{
			rootFrame.Navigate(typeof(MainPage), args.Arguments);
		}

		MainWindow.SetWindowIcon();
		MainWindow.Activate();
	}

	void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
	{
		throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
	}

	public static void InitializeLogging()
	{
#if DEBUG
		var factory = LoggerFactory.Create(builder =>
		{
#if __WASM__
			builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
			builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
			builder.AddConsole();
#else
			builder.AddConsole();
#endif
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

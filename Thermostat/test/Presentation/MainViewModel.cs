namespace test.Presentation;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;

    [ObservableProperty]
    private string? name;

	[ObservableProperty]
	private ThermostatState thermostat = new()
	{
		CurrentTemperature = 21.8,
		TargetTemperature = 24.0,
		MaxTemperature = 30.0,
		MinTemperature = 16.0,
		Mode = "Heating",
		IsActive = true
	};

	[ObservableProperty]
	private ObservableCollection<MetricCard> metrics = new()
	{
		new MetricCard { Icon = "\uE9CA", IconColor = "#00D9FF", Value = "45", Unit = "%", Label = "Humidity" },
		new MetricCard { Icon = "\uE81E", IconColor = "#00FF7F", Value = "98", Unit = "AQI", Label = "Air Quality" },
		new MetricCard { Icon = "\uE945", IconColor = "#FFD700", Value = "1.2", Unit = "W", Label = "Power" }
	};

	[ObservableProperty]
	private ObservableCollection<double> energyData = new()
	{
		30, 45, 35, 50, 40, 55, 45, 60, 50, 45, 55, 65
	};

	[ObservableProperty]
	private double currentProgress = 0.0;

	partial void OnCurrentProgressChanged(double value)
	{
		// Update current temperature based on progress
		var range = Thermostat.MaxTemperature - Thermostat.MinTemperature;
		var newTemp = Thermostat.MinTemperature + (value * range);

		if (Math.Abs(Thermostat.CurrentTemperature - newTemp) > 0.1)
		{
			Thermostat = Thermostat with { CurrentTemperature = newTemp };
		}
	}

    public MainViewModel(
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";

        // Safe null handling for environment configuration
        var environment = appInfo?.Value?.Environment;
        if (!string.IsNullOrEmpty(environment))
        {
            Title += $" - {environment}";
        }

        GoToSecond = new AsyncRelayCommand(GoToSecondView);

		// Calculate progress for circular arc (0-1 range)
		UpdateProgress();
    }

    public string? Title { get; }

	public string GreetingText => GetGreeting();

	public string StatusText => $"{Thermostat.Mode} to {Thermostat.TargetTemperature:F0}°";

    public ICommand GoToSecond { get; }

    private async Task GoToSecondView()
    {
        // Only navigate if Name is not null or empty
        if (!string.IsNullOrWhiteSpace(Name))
        {
            await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name));
        }
    }

	private void UpdateProgress()
	{
		var range = Thermostat.MaxTemperature - Thermostat.MinTemperature;
		var value = Thermostat.CurrentTemperature - Thermostat.MinTemperature;
		CurrentProgress = value / range;
	}

	private string GetGreeting()
	{
		var hour = DateTime.Now.Hour;
		return hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
	}

}

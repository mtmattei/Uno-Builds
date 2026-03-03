using FluxTransit.Models;
using FluxTransit.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FluxTransit.Presentation;

public partial record DashboardModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _localizer;
    private readonly ITransitService _transitService;
    private readonly IAiRouteService _aiRouteService;

    public DashboardModel(
        IStringLocalizer localizer,
        INavigator navigator,
        ITransitService transitService,
        IAiRouteService aiRouteService)
    {
        _localizer = localizer;
        _navigator = navigator;
        _transitService = transitService;
        _aiRouteService = aiRouteService;
    }

    // User greeting based on time of day
    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Good morning,",
                < 17 => "Good afternoon,",
                _ => "Good evening,"
            };
        }
    }

    // Origin input state
    public IState<string> Origin => State<string>.Value(this, () => string.Empty);

    // Destination input state
    public IState<string> Destination => State<string>.Value(this, () => string.Empty);

    // Route suggestions state - updated when user searches
    public IListState<RouteSuggestion> RouteSuggestions => ListState<RouteSuggestion>.Empty(this);

    // Loading state for route search
    public IState<bool> IsSearching => State<bool>.Value(this, () => false);

    // Live routes feed - refreshes automatically
    public IListFeed<TransitRoute> LiveRoutes => ListFeed.Async<TransitRoute>(
        async ct => (await _transitService.GetLiveRoutesAsync(ct)).ToImmutableList());

    // Network status feed
    public IFeed<NetworkStatus> NetworkStatus => Feed.Async(
        async ct => await _transitService.GetNetworkStatusAsync(ct));

    // Crowd chart series for LiveCharts2
    public ISeries[] CrowdSeries { get; } = new ISeries[]
    {
        new ColumnSeries<double>
        {
            Values = new double[] { 25, 40, 65, 85, 95, 80, 70, 55, 45, 60, 75, 50 },
            Name = "Metro",
            Fill = new SolidColorPaint(new SKColor(129, 140, 248)), // Primary indigo
            MaxBarWidth = 12,
            Rx = 4,
            Ry = 4
        },
        new ColumnSeries<double>
        {
            Values = new double[] { 20, 35, 55, 70, 80, 65, 55, 45, 35, 50, 60, 40 },
            Name = "Bus",
            Fill = new SolidColorPaint(new SKColor(251, 191, 36)), // Warning amber
            MaxBarWidth = 12,
            Rx = 4,
            Ry = 4
        }
    };

    // X axis labels for crowd chart
    public Axis[] CrowdXAxes { get; } = new Axis[]
    {
        new Axis
        {
            Labels = new string[] { "6AM", "7AM", "8AM", "9AM", "10AM", "11AM", "12PM", "1PM", "2PM", "3PM", "4PM", "5PM" },
            LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184)), // Text muted
            TextSize = 10
        }
    };

    // Y axis configuration for crowd chart
    public Axis[] CrowdYAxes { get; } = new Axis[]
    {
        new Axis
        {
            LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184)),
            TextSize = 10,
            MinLimit = 0,
            MaxLimit = 100,
            Labeler = value => $"{value}%"
        }
    };

    // Navigate to profile
    public async Task GoToProfile()
    {
        await _navigator.NavigateViewModelAsync<ProfileModel>(this);
    }

    // Find routes command - calls AI service
    public async Task FindRoutes(CancellationToken ct)
    {
        var origin = await Origin;
        var destination = await Destination;

        // Validate inputs
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return;
        }

        // Set loading state
        await IsSearching.Set(true, ct);

        try
        {
            // Call AI service for route suggestions
            var suggestions = await _aiRouteService.GetRouteSuggestionsAsync(origin, destination, ct);
            await RouteSuggestions.UpdateAsync(list => suggestions.ToImmutableList(), ct);
        }
        finally
        {
            await IsSearching.Set(false, ct);
        }
    }

    // Clear route suggestions
    public async Task ClearSuggestions(CancellationToken ct)
    {
        await RouteSuggestions.UpdateAsync(list => ImmutableList<RouteSuggestion>.Empty, ct);
    }

    // Helper to get route color based on type
    public static string GetRouteColor(TransitRoute route) => route.Type switch
    {
        RouteType.Metro => "#818cf8", // Primary indigo
        RouteType.Bus => "#fbbf24",   // Warning amber
        RouteType.Train => "#34d399", // Success emerald
        _ => "#818cf8"
    };

    // Helper to get route type color
    public static string GetRouteTypeColor(RouteType type) => type switch
    {
        RouteType.Metro => "#818cf8", // Primary indigo
        RouteType.Bus => "#fbbf24",   // Warning amber
        RouteType.Train => "#34d399", // Success emerald
        _ => "#818cf8"
    };

    // Helper to get crowd level text
    public static string GetCrowdLevelText(CrowdLevel level) => level switch
    {
        CrowdLevel.Low => "Low crowding",
        CrowdLevel.Moderate => "Moderate crowding",
        CrowdLevel.High => "High crowding",
        CrowdLevel.VeryHigh => "Very high crowding",
        _ => "Unknown"
    };

    // Helper to get network health status text
    public static string GetNetworkHealthText(NetworkHealth health) => health switch
    {
        NetworkHealth.Normal => "NORMAL",
        NetworkHealth.MinorDelays => "MINOR DELAYS",
        NetworkHealth.MajorDelays => "MAJOR DELAYS",
        NetworkHealth.ServiceDisruption => "DISRUPTION",
        _ => "UNKNOWN"
    };
}

using FluxTransit.Models;

namespace FluxTransit.Services;

/// <summary>
/// Mock implementation of AI route planning service.
/// Returns realistic Montreal transit suggestions.
/// </summary>
public class MockAiRouteService : IAiRouteService
{
    private readonly Dictionary<string, string> _stationAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "home", "Berri-UQAM" },
        { "work", "McGill" },
        { "airport", "YUL-Aeroport-Montreal-Trudeau" },
        { "downtown", "Place-des-Arts" },
        { "university", "Universite-de-Montreal" }
    };

    public async Task<IReadOnlyList<RouteSuggestion>> GetRouteSuggestionsAsync(
        string origin,
        string destination,
        CancellationToken ct = default)
    {
        // Simulate AI processing time
        await Task.Delay(800, ct);

        // Resolve aliases
        var resolvedOrigin = ResolveAlias(origin);
        var resolvedDestination = ResolveAlias(destination);

        // Generate mock suggestions based on common Montreal routes
        var suggestions = GenerateSuggestions(resolvedOrigin, resolvedDestination);

        return suggestions;
    }

    private string ResolveAlias(string location)
    {
        return _stationAliases.TryGetValue(location.Trim(), out var resolved)
            ? resolved
            : location.Trim();
    }

    private IReadOnlyList<RouteSuggestion> GenerateSuggestions(string origin, string destination)
    {
        var now = DateTime.Now;
        var baseTime = now.AddMinutes(5);

        // Generate three route options
        return new List<RouteSuggestion>
        {
            CreateFastestRoute(origin, destination, baseTime),
            CreateFewestTransfersRoute(origin, destination, baseTime.AddMinutes(3)),
            CreateScenicRoute(origin, destination, baseTime.AddMinutes(7))
        };
    }

    private RouteSuggestion CreateFastestRoute(string origin, string destination, DateTime departureTime)
    {
        var legs = new List<RouteLeg>
        {
            new RouteLeg(
                RouteNumber: "Orange",
                RouteName: "Orange Line",
                Type: RouteType.Metro,
                DepartureStop: origin,
                ArrivalStop: "Lionel-Groulx",
                DurationMinutes: 8,
                DepartureTime: departureTime.ToString("HH:mm")),
            new RouteLeg(
                RouteNumber: "Green",
                RouteName: "Green Line",
                Type: RouteType.Metro,
                DepartureStop: "Lionel-Groulx",
                ArrivalStop: destination,
                DurationMinutes: 6,
                DepartureTime: departureTime.AddMinutes(10).ToString("HH:mm"))
        };

        return new RouteSuggestion(
            Id: Guid.NewGuid().ToString(),
            Title: "Fastest Route",
            Description: $"{origin} to {destination} via Lionel-Groulx",
            TotalMinutes: 18,
            Transfers: 1,
            Legs: legs,
            AiReasoning: "This route minimizes travel time by using the metro interchange at Lionel-Groulx. Current conditions show low crowding on both lines.");
    }

    private RouteSuggestion CreateFewestTransfersRoute(string origin, string destination, DateTime departureTime)
    {
        var legs = new List<RouteLeg>
        {
            new RouteLeg(
                RouteNumber: "24",
                RouteName: "Sherbrooke",
                Type: RouteType.Bus,
                DepartureStop: origin,
                ArrivalStop: destination,
                DurationMinutes: 25,
                DepartureTime: departureTime.ToString("HH:mm"))
        };

        return new RouteSuggestion(
            Id: Guid.NewGuid().ToString(),
            Title: "Direct Route",
            Description: $"Bus 24 Sherbrooke - No transfers",
            TotalMinutes: 25,
            Transfers: 0,
            Legs: legs,
            AiReasoning: "This route requires no transfers, ideal if you prefer a relaxed journey or have luggage. The bus runs every 8 minutes during peak hours.");
    }

    private RouteSuggestion CreateScenicRoute(string origin, string destination, DateTime departureTime)
    {
        var legs = new List<RouteLeg>
        {
            new RouteLeg(
                RouteNumber: "Blue",
                RouteName: "Blue Line",
                Type: RouteType.Metro,
                DepartureStop: origin,
                ArrivalStop: "Jean-Talon",
                DurationMinutes: 12,
                DepartureTime: departureTime.ToString("HH:mm")),
            new RouteLeg(
                RouteNumber: "Orange",
                RouteName: "Orange Line",
                Type: RouteType.Metro,
                DepartureStop: "Jean-Talon",
                ArrivalStop: "Bonaventure",
                DurationMinutes: 10,
                DepartureTime: departureTime.AddMinutes(14).ToString("HH:mm")),
            new RouteLeg(
                RouteNumber: "747",
                RouteName: "747 Express",
                Type: RouteType.Bus,
                DepartureStop: "Bonaventure",
                ArrivalStop: destination,
                DurationMinutes: 8,
                DepartureTime: departureTime.AddMinutes(26).ToString("HH:mm"))
        };

        return new RouteSuggestion(
            Id: Guid.NewGuid().ToString(),
            Title: "Scenic Route",
            Description: $"Via Jean-Talon and downtown core",
            TotalMinutes: 38,
            Transfers: 2,
            Legs: legs,
            AiReasoning: "This route takes you through Montreal's vibrant neighborhoods. Perfect if you have time to enjoy the journey or want to make stops along the way.");
    }
}

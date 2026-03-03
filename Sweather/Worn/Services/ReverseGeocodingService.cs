using System.Net.Http;
using System.Text.Json;

namespace Worn.Services;

public class ReverseGeocodingService : IReverseGeocodingService
{
    private static readonly HttpClient _httpClient = new();
    private readonly ILogger<ReverseGeocodingService> _logger;

    public ReverseGeocodingService(ILogger<ReverseGeocodingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetLocationNameAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=10";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "WornWeatherApp/1.0");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.SendAsync(request, cts.Token);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("address", out var address))
            {
                var city = address.TryGetProperty("city", out var c) ? c.GetString()
                    : address.TryGetProperty("town", out var t) ? t.GetString()
                    : address.TryGetProperty("village", out var v) ? v.GetString()
                    : null;

                var state = address.TryGetProperty("state", out var s) ? s.GetString() : null;
                var country = address.TryGetProperty("country", out var co) ? co.GetString() : null;

                if (city is not null && state is not null)
                    return $"{city}, {state}";
                if (city is not null && country is not null)
                    return $"{city}, {country}";
                if (city is not null)
                    return city;
            }

            return "Your Location";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reverse geocoding failed for ({Lat}, {Lng})", latitude, longitude);
            return "Your Location";
        }
    }

    public async Task<(double Latitude, double Longitude)?> SearchCityAsync(string cityName, CancellationToken ct = default)
    {
        try
        {
            var encoded = Uri.EscapeDataString(cityName);
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={encoded}&limit=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "WornWeatherApp/1.0");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.SendAsync(request, cts.Token);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement;

            if (results.GetArrayLength() > 0)
            {
                var first = results[0];
                var lat = double.Parse(first.GetProperty("lat").GetString()!,
                    System.Globalization.CultureInfo.InvariantCulture);
                var lng = double.Parse(first.GetProperty("lon").GetString()!,
                    System.Globalization.CultureInfo.InvariantCulture);
                return (lat, lng);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "City search failed for '{City}'", cityName);
            return null;
        }
    }

    public async Task<IList<CitySuggestion>> SearchCitySuggestionsAsync(string query, CancellationToken ct = default)
    {
        var results = new List<CitySuggestion>();
        try
        {
            var encoded = Uri.EscapeDataString(query);
            // Open-Meteo geocoding API - fast, free, no key required
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={encoded}&count=5&language=en&format=json";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var response = await _httpClient.GetAsync(url, cts.Token);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("results", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var name = item.GetProperty("name").GetString() ?? "";
                    var lat = item.GetProperty("latitude").GetDouble();
                    var lng = item.GetProperty("longitude").GetDouble();

                    var admin1 = item.TryGetProperty("admin1", out var a1) ? a1.GetString() : null;
                    var country = item.TryGetProperty("country", out var co) ? co.GetString() : null;

                    var parts = new List<string> { name };
                    if (admin1 is not null) parts.Add(admin1);
                    if (country is not null) parts.Add(country);

                    results.Add(new CitySuggestion(string.Join(", ", parts), lat, lng));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during debounce cancellation - don't log
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "City suggestions search failed for '{Query}'", query);
        }

        return results;
    }
}

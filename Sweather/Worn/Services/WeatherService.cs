using System.Net.Http;
using System.Text.Json;
using Worn.Models;

namespace Worn.Services;

public class WeatherService : IWeatherService
{
    private static readonly HttpClient _httpClient = new();
    private OpenMeteoResponse? _cached;
    private DateTime _cachedAt;
    private (double Lat, double Lng) _cachedCoords;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public async Task<OpenMeteoResponse> FetchWeatherAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        // Return cached if valid
        if (_cached is not null
            && DateTime.UtcNow - _cachedAt < CacheTtl
            && Math.Abs(_cachedCoords.Lat - latitude) < 0.01
            && Math.Abs(_cachedCoords.Lng - longitude) < 0.01)
        {
            return _cached;
        }

        var url = "https://api.open-meteo.com/v1/forecast"
            + $"?latitude={latitude}&longitude={longitude}"
            + "&temperature_unit=fahrenheit"
            + "&wind_speed_unit=mph"
            + "&forecast_days=7"
            + "&timezone=auto"
            + "&current=apparent_temperature,temperature_2m,precipitation,rain,snowfall,weather_code,wind_speed_10m,wind_gusts_10m,relative_humidity_2m,cloud_cover,uv_index,is_day,visibility"
            + "&hourly=apparent_temperature,precipitation_probability,precipitation,rain,snowfall,weather_code,wind_speed_10m,uv_index,cloud_cover,relative_humidity_2m,visibility"
            + "&daily=weather_code,apparent_temperature_max,apparent_temperature_min,precipitation_sum,precipitation_probability_max,wind_speed_10m_max,uv_index_max,sunrise,sunset";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        var response = await _httpClient.GetAsync(url, cts.Token);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cts.Token);
        var result = JsonSerializer.Deserialize<OpenMeteoResponse>(json)
            ?? throw new InvalidOperationException("Failed to parse weather data.");

        _cached = result;
        _cachedAt = DateTime.UtcNow;
        _cachedCoords = (latitude, longitude);

        return result;
    }
}

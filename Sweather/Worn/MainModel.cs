using Uno.Extensions.Reactive;
using Worn.Models;
using Worn.Services;

namespace Worn;

public partial record MainModel(
    IGeoLocationService GeoService,
    IWeatherService WeatherService,
    IReverseGeocodingService GeocodingService,
    IWeatherMappingEngine Engine)
{
    public IState<string> CityOverride => State.Value(this, () => "");

    public IFeed<WornResult> Result => CityOverride.SelectAsync(async (city, ct) =>
    {
        var (lat, lng) = await ResolveLocationAsync(city, ct);
        var locName = await GeocodingService.GetLocationNameAsync(lat, lng, ct);
        var api = await WeatherService.FetchWeatherAsync(lat, lng, ct);
        var current = WeatherNormalizer.NormalizeCurrent(api);
        var hourly = WeatherNormalizer.NormalizeHourly(api);
        var daily = WeatherNormalizer.NormalizeDaily(api);
        return Engine.Process(current, hourly, daily, locName);
    });

    public IFeed<string> LocationName => Result.Select(r => r.LocationName);

    private async Task<(double Lat, double Lng)> ResolveLocationAsync(string city, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(city))
        {
            var coords = await GeocodingService.SearchCityAsync(city, ct);
            if (coords is not null)
                return coords.Value;
        }
        return await GeoService.GetLocationAsync(ct);
    }
}

using Worn.Models;

namespace Worn.Services;

public interface IWeatherService
{
    Task<OpenMeteoResponse> FetchWeatherAsync(double latitude, double longitude, CancellationToken ct = default);
}

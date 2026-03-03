namespace FluxTransit.Services.Caching;
using WeatherForecast = FluxTransit.Client.Models.WeatherForecast;
public interface IWeatherCache
{
    ValueTask<IImmutableList<WeatherForecast>> GetForecast(CancellationToken token);
}

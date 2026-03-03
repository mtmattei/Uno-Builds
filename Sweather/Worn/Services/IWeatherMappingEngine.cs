using Worn.Models;

namespace Worn.Services;

public interface IWeatherMappingEngine
{
    WornResult Process(CurrentWeather current, IList<HourlyWeather> hourly, IList<DailyWeather> daily, string locationName);
}

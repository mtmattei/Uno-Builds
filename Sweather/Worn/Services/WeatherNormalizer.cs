using Worn.Models;

namespace Worn.Services;

public static class WeatherNormalizer
{
    public static CurrentWeather NormalizeCurrent(OpenMeteoResponse api)
    {
        var c = api.Current ?? new OpenMeteoCurrent();
        var firstHourPrecipProb = api.Hourly?.PrecipitationProbability?.FirstOrDefault() ?? 0;

        return new CurrentWeather(
            ApparentTemp: c.ApparentTemperature,
            Temp: c.Temperature2m,
            Humidity: c.RelativeHumidity2m,
            Precipitation: c.Precipitation,
            Rain: c.Rain,
            Snowfall: c.Snowfall,
            WeatherCode: c.WeatherCode,
            WindSpeed: c.WindSpeed10m,
            WindGusts: c.WindGusts10m,
            CloudCover: c.CloudCover,
            UvIndex: c.UvIndex,
            IsDay: c.IsDay == 1,
            Visibility: c.Visibility,
            IsRaining: c.Rain > 0 || c.WeatherCode is >= 61 and <= 67,
            IsSnowing: c.Snowfall > 0 || c.WeatherCode is >= 71 and <= 77,
            PrecipProbability: firstHourPrecipProb
        );
    }

    public static List<HourlyWeather> NormalizeHourly(OpenMeteoResponse api)
    {
        var h = api.Hourly;
        if (h?.Time is null) return [];

        var count = h.Time.Count;
        var result = new List<HourlyWeather>(count);

        for (var i = 0; i < count; i++)
        {
            var rain = h.Rain?.ElementAtOrDefault(i) ?? 0;
            var snow = h.Snowfall?.ElementAtOrDefault(i) ?? 0;
            var wc = h.WeatherCode?.ElementAtOrDefault(i) ?? 0;

            result.Add(new HourlyWeather(
                Time: h.Time[i],
                ApparentTemp: h.ApparentTemperature?.ElementAtOrDefault(i) ?? 0,
                PrecipProbability: h.PrecipitationProbability?.ElementAtOrDefault(i) ?? 0,
                Precipitation: h.Precipitation?.ElementAtOrDefault(i) ?? 0,
                WeatherCode: wc,
                WindSpeed: h.WindSpeed10m?.ElementAtOrDefault(i) ?? 0,
                UvIndex: h.UvIndex?.ElementAtOrDefault(i) ?? 0,
                CloudCover: h.CloudCover?.ElementAtOrDefault(i) ?? 0,
                IsRaining: rain > 0 || wc is >= 61 and <= 67,
                IsSnowing: snow > 0 || wc is >= 71 and <= 77,
                Humidity: h.RelativeHumidity2m?.ElementAtOrDefault(i) ?? 50,
                Visibility: h.Visibility?.ElementAtOrDefault(i) ?? 10000
            ));
        }

        return result;
    }

    public static List<DailyWeather> NormalizeDaily(OpenMeteoResponse api)
    {
        var d = api.Daily;
        if (d?.Time is null) return [];

        var count = d.Time.Count;
        var result = new List<DailyWeather>(count);

        for (var i = 0; i < count; i++)
        {
            var maxTemp = d.ApparentTemperatureMax?.ElementAtOrDefault(i) ?? 0;
            var minTemp = d.ApparentTemperatureMin?.ElementAtOrDefault(i) ?? 0;
            var wc = d.WeatherCode?.ElementAtOrDefault(i) ?? 0;

            result.Add(new DailyWeather(
                Date: d.Time[i],
                WeatherCode: wc,
                ApparentTempMax: maxTemp,
                ApparentTempMin: minTemp,
                ApparentTempMid: (maxTemp + minTemp) / 2.0,
                PrecipSum: d.PrecipitationSum?.ElementAtOrDefault(i) ?? 0,
                PrecipProbabilityMax: d.PrecipitationProbabilityMax?.ElementAtOrDefault(i) ?? 0,
                WindSpeedMax: d.WindSpeed10mMax?.ElementAtOrDefault(i) ?? 0,
                UvIndexMax: d.UvIndexMax?.ElementAtOrDefault(i) ?? 0,
                Sunrise: d.Sunrise?.ElementAtOrDefault(i) ?? "",
                Sunset: d.Sunset?.ElementAtOrDefault(i) ?? "",
                IsRainy: wc is >= 61 and <= 67 || (d.PrecipitationSum?.ElementAtOrDefault(i) ?? 0) > 1,
                IsSnowy: wc is >= 71 and <= 77
            ));
        }

        return result;
    }
}

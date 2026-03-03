using System.Text.Json.Serialization;

namespace Worn.Models;

public record CurrentWeather(
    double ApparentTemp,
    double Temp,
    double Humidity,
    double Precipitation,
    double Rain,
    double Snowfall,
    int WeatherCode,
    double WindSpeed,
    double WindGusts,
    double CloudCover,
    double UvIndex,
    bool IsDay,
    double Visibility,
    bool IsRaining,
    bool IsSnowing,
    double PrecipProbability
);

public record HourlyWeather(
    string Time,
    double ApparentTemp,
    double PrecipProbability,
    double Precipitation,
    int WeatherCode,
    double WindSpeed,
    double UvIndex,
    double CloudCover,
    bool IsRaining,
    bool IsSnowing,
    double Humidity,
    double Visibility
);

public record DailyWeather(
    string Date,
    int WeatherCode,
    double ApparentTempMax,
    double ApparentTempMin,
    double ApparentTempMid,
    double PrecipSum,
    double PrecipProbabilityMax,
    double WindSpeedMax,
    double UvIndexMax,
    string Sunrise,
    string Sunset,
    bool IsRainy,
    bool IsSnowy
);

// Open-Meteo API response shape
public class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }

    [JsonPropertyName("hourly")]
    public OpenMeteoHourly? Hourly { get; set; }

    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; set; }
}

public class OpenMeteoCurrent
{
    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("rain")]
    public double Rain { get; set; }

    [JsonPropertyName("snowfall")]
    public double Snowfall { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }

    [JsonPropertyName("wind_gusts_10m")]
    public double WindGusts10m { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public double RelativeHumidity2m { get; set; }

    [JsonPropertyName("cloud_cover")]
    public double CloudCover { get; set; }

    [JsonPropertyName("uv_index")]
    public double UvIndex { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("visibility")]
    public double Visibility { get; set; }
}

public class OpenMeteoHourly
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public List<double>? ApparentTemperature { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public List<double>? PrecipitationProbability { get; set; }

    [JsonPropertyName("precipitation")]
    public List<double>? Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public List<double>? WindSpeed10m { get; set; }

    [JsonPropertyName("uv_index")]
    public List<double>? UvIndex { get; set; }

    [JsonPropertyName("cloud_cover")]
    public List<double>? CloudCover { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public List<double>? RelativeHumidity2m { get; set; }

    [JsonPropertyName("visibility")]
    public List<double>? Visibility { get; set; }

    [JsonPropertyName("rain")]
    public List<double>? Rain { get; set; }

    [JsonPropertyName("snowfall")]
    public List<double>? Snowfall { get; set; }
}

public class OpenMeteoDaily
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("apparent_temperature_max")]
    public List<double>? ApparentTemperatureMax { get; set; }

    [JsonPropertyName("apparent_temperature_min")]
    public List<double>? ApparentTemperatureMin { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double>? PrecipitationSum { get; set; }

    [JsonPropertyName("precipitation_probability_max")]
    public List<double>? PrecipitationProbabilityMax { get; set; }

    [JsonPropertyName("wind_speed_10m_max")]
    public List<double>? WindSpeed10mMax { get; set; }

    [JsonPropertyName("uv_index_max")]
    public List<double>? UvIndexMax { get; set; }

    [JsonPropertyName("sunrise")]
    public List<string>? Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public List<string>? Sunset { get; set; }
}

namespace Worn.Services;

public interface IReverseGeocodingService
{
    Task<string> GetLocationNameAsync(double latitude, double longitude, CancellationToken ct = default);
    Task<(double Latitude, double Longitude)?> SearchCityAsync(string cityName, CancellationToken ct = default);
    Task<IList<CitySuggestion>> SearchCitySuggestionsAsync(string query, CancellationToken ct = default);
}

public record CitySuggestion(string DisplayName, double Latitude, double Longitude);

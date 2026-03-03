namespace Worn.Services;

public interface IGeoLocationService
{
    Task<(double Latitude, double Longitude)> GetLocationAsync(CancellationToken ct = default);
}

using Windows.Devices.Geolocation;

namespace Worn.Services;

public class GeoLocationService : IGeoLocationService
{
    // Fallback: Montreal, QC (used when location services are unavailable)
    private const double FallbackLat = 45.5019;
    private const double FallbackLng = -73.5674;

    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(ILogger<GeoLocationService> logger)
    {
        _logger = logger;
    }

    public async Task<(double Latitude, double Longitude)> GetLocationAsync(CancellationToken ct = default)
    {
        try
        {
            var geolocator = new Geolocator
            {
                DesiredAccuracyInMeters = 1000
            };

            var access = await Geolocator.RequestAccessAsync();
            if (access != GeolocationAccessStatus.Allowed)
            {
                _logger.LogWarning("Geolocation access denied (status: {Status}), using fallback", access);
                return (FallbackLat, FallbackLng);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            var position = await geolocator.GetGeopositionAsync()
                .AsTask(cts.Token);

            return (position.Coordinate.Point.Position.Latitude,
                    position.Coordinate.Point.Position.Longitude);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geolocation failed, using fallback");
            return (FallbackLat, FallbackLng);
        }
    }
}

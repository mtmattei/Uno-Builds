using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Microsoft.UI.Xaml;

namespace FriendSonar.Services;

public class LocationService : IDisposable
{
    private readonly Geolocator _geolocator;
    private DispatcherTimer? _updateTimer;
    private bool _isTracking;

    public event EventHandler<LocationUpdatedEventArgs>? LocationUpdated;
    public event EventHandler<string>? Error;

    public double? CurrentLatitude { get; private set; }
    public double? CurrentLongitude { get; private set; }
    public DateTime? LastUpdate { get; private set; }

    public LocationService()
    {
        _geolocator = new Geolocator
        {
            DesiredAccuracy = PositionAccuracy.High,
            DesiredAccuracyInMeters = 10
        };
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            var status = await Geolocator.RequestAccessAsync();
            return status == GeolocationAccessStatus.Allowed;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Permission request failed: {ex.Message}");
            return false;
        }
    }

    public async Task<(double lat, double lon)?> GetCurrentLocationAsync()
    {
        try
        {
            var position = await _geolocator.GetGeopositionAsync(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(30));

            CurrentLatitude = position.Coordinate.Point.Position.Latitude;
            CurrentLongitude = position.Coordinate.Point.Position.Longitude;
            LastUpdate = DateTime.UtcNow;

            return (CurrentLatitude.Value, CurrentLongitude.Value);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to get location: {ex.Message}");
            return null;
        }
    }

    public void StartTracking(int intervalSeconds = 30)
    {
        if (_isTracking) return;

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(intervalSeconds)
        };
        _updateTimer.Tick += async (s, e) => await UpdateLocationAsync();
        _updateTimer.Start();
        _isTracking = true;

        // Get initial location immediately
        _ = UpdateLocationAsync();
    }

    public void StopTracking()
    {
        if (_updateTimer != null)
        {
            _updateTimer.Stop();
            _updateTimer = null;
        }
        _isTracking = false;
    }

    private async Task UpdateLocationAsync()
    {
        var location = await GetCurrentLocationAsync();
        if (location.HasValue)
        {
            LocationUpdated?.Invoke(this, new LocationUpdatedEventArgs(
                location.Value.lat,
                location.Value.lon,
                DateTime.UtcNow));
        }
    }

    public void Dispose()
    {
        StopTracking();
    }
}

public class LocationUpdatedEventArgs : EventArgs
{
    public double Latitude { get; }
    public double Longitude { get; }
    public DateTime Timestamp { get; }

    public LocationUpdatedEventArgs(double latitude, double longitude, DateTime timestamp)
    {
        Latitude = latitude;
        Longitude = longitude;
        Timestamp = timestamp;
    }
}

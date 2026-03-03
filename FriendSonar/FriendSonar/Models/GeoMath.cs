using System;

namespace FriendSonar.Models;

public static class GeoMath
{
    private const double EarthRadiusMiles = 3959.0;

    /// <summary>
    /// Calculate distance between two GPS coordinates using Haversine formula.
    /// </summary>
    public static double DistanceMiles(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMiles * c;
    }

    /// <summary>
    /// Calculate bearing from point 1 to point 2 (0-360 degrees, 0=North).
    /// </summary>
    public static int BearingDegrees(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRad(lon2 - lon1);
        var lat1Rad = ToRad(lat1);
        var lat2Rad = ToRad(lat2);

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearing = Math.Atan2(y, x) * 180.0 / Math.PI;

        return (int)((bearing + 360) % 360);
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;
}

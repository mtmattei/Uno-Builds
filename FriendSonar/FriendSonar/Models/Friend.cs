using System;
using Windows.UI;

namespace FriendSonar.Models;

public class Friend
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = "👤";

    // GPS coordinates
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdated { get; set; }

    // Computed from user's location - set by UpdateFromUserLocation()
    public double DistanceMilesValue { get; set; }
    public int Angle { get; set; }

    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }

    public string DistanceMiles => $"{DistanceMilesValue:F1} MI";
    public string BearingDegrees => $"{Angle}°";
    public string DistanceAndBearing => $"{DistanceMiles}  ·  {BearingDegrees}";
    public string TooltipText => $"{Name} - {DistanceAndBearing}";

    // Status based on last update time
    public FriendStatus Status
    {
        get
        {
            var elapsed = DateTime.UtcNow - LastUpdated;
            if (elapsed.TotalMinutes < 2) return FriendStatus.Active;
            if (elapsed.TotalMinutes < 5) return FriendStatus.Idle;
            return FriendStatus.Away;
        }
    }

    public string StatusText => Status switch
    {
        FriendStatus.Active => "ACTIVE",
        FriendStatus.Idle => "IDLE",
        FriendStatus.Away => "AWAY",
        _ => "UNKNOWN"
    };

    public string LastSeenText
    {
        get
        {
            var elapsed = DateTime.UtcNow - LastUpdated;
            if (elapsed.TotalSeconds < 60) return "Just now";
            if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}m ago";
            return $"{(int)elapsed.TotalHours}h ago";
        }
    }

    public string EtaText => LastSeenText;

    public Color StatusColor => Status switch
    {
        FriendStatus.Active => Color.FromArgb(0xFF, 0x00, 0xFF, 0x41),
        FriendStatus.Idle => Color.FromArgb(0xFF, 0xFF, 0xAA, 0x00),
        FriendStatus.Away => Color.FromArgb(0xFF, 0xFF, 0x41, 0x41),
        _ => Color.FromArgb(0xFF, 0x00, 0xFF, 0x41)
    };

    /// <summary>
    /// Update distance and bearing relative to user's current location.
    /// </summary>
    public void UpdateFromUserLocation(double userLat, double userLon)
    {
        DistanceMilesValue = GeoMath.DistanceMiles(userLat, userLon, Latitude, Longitude);
        Angle = GeoMath.BearingDegrees(userLat, userLon, Latitude, Longitude);
    }

    /// <summary>
    /// Check if this friend should be visible (updated within 5 minutes).
    /// </summary>
    public bool IsVisible => (DateTime.UtcNow - LastUpdated).TotalMinutes <= 5;
}

public enum FriendStatus
{
    Active,
    Idle,
    Away
}

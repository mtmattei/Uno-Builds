namespace MsnMessenger.Models;

public enum ActivityType
{
    Spotify,
    AppleMusic,
    Gaming,
    Video,
    Custom
}

public class LiveActivity
{
    public ActivityType Type { get; set; }

    // Common fields
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string? AccentColor { get; set; }
    public DateTime? StartedAt { get; set; }

    // Music-specific
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? AlbumArt { get; set; }
    public int Progress { get; set; } // 0-100
    public string? Duration { get; set; } // "M:SS" format

    // Gaming-specific
    public string? Platform { get; set; }
    public string? Status { get; set; }
    public string? PartySize { get; set; }
    public bool Joinable { get; set; }

    // Video-specific
    public string? Service { get; set; }
    public string? Thumbnail { get; set; }
    public bool IsLive { get; set; }
    public int? ViewerCount { get; set; }

    // Action
    public string? ActionLabel { get; set; }
    public string? ActionUrl { get; set; }

    // Helper properties
    public string ServiceLabel => Type switch
    {
        ActivityType.Spotify => "Listening on Spotify",
        ActivityType.AppleMusic => "Listening on Apple Music",
        ActivityType.Gaming => $"Playing on {Platform ?? "PC"}",
        ActivityType.Video => $"Watching on {Service ?? "Video"}",
        _ => "Activity"
    };

    public string DisplayDuration
    {
        get
        {
            if (StartedAt.HasValue)
            {
                var elapsed = DateTime.Now - StartedAt.Value;
                if (elapsed.TotalHours >= 1)
                    return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
                return $"{(int)elapsed.TotalMinutes} min";
            }
            return Duration ?? "";
        }
    }
}

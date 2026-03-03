namespace MsnMessenger.Models;

public class NowPlaying
{
    public string TrackName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string AlbumName { get; set; } = string.Empty;
    public string? AlbumArtUrl { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Progress { get; set; }
    public bool IsPlaying { get; set; }

    public string DisplayText => $"{ArtistName} - {TrackName}";

    public double ProgressPercent => Duration.TotalSeconds > 0
        ? (Progress.TotalSeconds / Duration.TotalSeconds) * 100
        : 0;
}

public static class MockSpotifyData
{
    private static readonly Random _random = new();

    private static readonly NowPlaying[] _tracks =
    [
        new NowPlaying
        {
            TrackName = "Around the World",
            ArtistName = "Daft Punk",
            AlbumName = "Homework",
            AlbumArtUrl = "https://picsum.photos/seed/daftpunk/64/64",
            Duration = TimeSpan.FromMinutes(7).Add(TimeSpan.FromSeconds(9)),
            Progress = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(34)),
            IsPlaying = true
        },
        new NowPlaying
        {
            TrackName = "Blinding Lights",
            ArtistName = "The Weeknd",
            AlbumName = "After Hours",
            AlbumArtUrl = "https://picsum.photos/seed/weeknd/64/64",
            Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(20)),
            Progress = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(45)),
            IsPlaying = true
        },
        new NowPlaying
        {
            TrackName = "Levitating",
            ArtistName = "Dua Lipa",
            AlbumName = "Future Nostalgia",
            AlbumArtUrl = "https://picsum.photos/seed/dualipa/64/64",
            Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(23)),
            Progress = TimeSpan.FromMinutes(0).Add(TimeSpan.FromSeconds(58)),
            IsPlaying = true
        },
        new NowPlaying
        {
            TrackName = "Bad Guy",
            ArtistName = "Billie Eilish",
            AlbumName = "WHEN WE ALL FALL ASLEEP",
            AlbumArtUrl = "https://picsum.photos/seed/billie/64/64",
            Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(14)),
            Progress = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(10)),
            IsPlaying = true
        },
        new NowPlaying
        {
            TrackName = "Starboy",
            ArtistName = "The Weeknd ft. Daft Punk",
            AlbumName = "Starboy",
            AlbumArtUrl = "https://picsum.photos/seed/starboy/64/64",
            Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(50)),
            Progress = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(22)),
            IsPlaying = true
        },
        new NowPlaying
        {
            TrackName = "Get Lucky",
            ArtistName = "Daft Punk ft. Pharrell",
            AlbumName = "Random Access Memories",
            AlbumArtUrl = "https://picsum.photos/seed/getlucky/64/64",
            Duration = TimeSpan.FromMinutes(6).Add(TimeSpan.FromSeconds(9)),
            Progress = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(15)),
            IsPlaying = true
        }
    ];

    public static NowPlaying GetRandomTrack()
    {
        return _tracks[_random.Next(_tracks.Length)];
    }

    public static NowPlaying GetCurrentTrack()
    {
        // Return a "current" track - first one for consistency
        return _tracks[0];
    }
}

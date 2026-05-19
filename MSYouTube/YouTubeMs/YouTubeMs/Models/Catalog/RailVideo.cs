using System.Globalization;

namespace YouTubeMs.Models.Catalog;

public partial record RailVideo(
    string Id,
    string Title,
    Channel Channel,
    TimeSpan Duration,
    string ThumbnailUrl,
    string YouTubeId)
{
    public string DurationLabel => Duration.TotalHours >= 1
        ? Duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
        : Duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);

    public string YouTubeEmbedUrl => $"https://www.youtube.com/embed/{YouTubeId}?autoplay=1&rel=0";
    public string YouTubeWatchUrl => $"https://www.youtube.com/watch?v={YouTubeId}";
}

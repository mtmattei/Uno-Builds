using System.Globalization;

namespace YouTubeMs.Models.Catalog;

public partial record FeaturedVideo(
    string Id,
    string BadgeText,
    string Title,
    string Description,
    Channel Channel,
    TimeSpan Duration,
    string BackgroundImageUrl,
    string YouTubeId)
{
    public string DurationLabel => Duration.TotalHours >= 1
        ? Duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
        : Duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);

    public string YouTubeEmbedUrl => $"https://www.youtube.com/embed/{YouTubeId}?autoplay=1&rel=0";
    public string YouTubeWatchUrl => $"https://www.youtube.com/watch?v={YouTubeId}";
}

using System.Globalization;

namespace YouTubeMs.Models.Catalog;

public partial record Video(
    string Id,
    string Title,
    Channel Channel,
    TimeSpan Duration,
    long ViewCount,
    DateTimeOffset PublishedAt,
    string ThumbnailUrl,
    string YouTubeId)
{
    public string YouTubeEmbedUrl => $"https://www.youtube.com/embed/{YouTubeId}?autoplay=1&rel=0";
    public string YouTubeWatchUrl => $"https://www.youtube.com/watch?v={YouTubeId}";


    public string DurationLabel => Duration.TotalHours >= 1
        ? Duration.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
        : Duration.ToString(@"m\:ss", CultureInfo.InvariantCulture);

    public string MetaLabel => $"{FormatViews(ViewCount)} views · {FormatRelative(PublishedAt)}";

    private static string FormatViews(long count) => count switch
    {
        >= 1_000_000_000 => $"{count / 1_000_000_000d:0.#}B",
        >= 1_000_000 => $"{count / 1_000_000d:0.#}M",
        >= 1_000 => $"{count / 1_000d:0.#}K",
        _ => count.ToString(CultureInfo.InvariantCulture),
    };

    private static string FormatRelative(DateTimeOffset published)
    {
        var span = DateTimeOffset.UtcNow - published;
        return span switch
        {
            { TotalDays: >= 365 } => $"{(int)(span.TotalDays / 365)} year{Plural(span.TotalDays / 365)} ago",
            { TotalDays: >= 30 } => $"{(int)(span.TotalDays / 30)} month{Plural(span.TotalDays / 30)} ago",
            { TotalDays: >= 7 } => $"{(int)(span.TotalDays / 7)} week{Plural(span.TotalDays / 7)} ago",
            { TotalDays: >= 1 } => $"{(int)span.TotalDays} day{Plural(span.TotalDays)} ago",
            { TotalHours: >= 1 } => $"{(int)span.TotalHours} hour{Plural(span.TotalHours)} ago",
            _ => "just now",
        };
    }

    private static string Plural(double v) => Math.Floor(v) == 1 ? string.Empty : "s";
}

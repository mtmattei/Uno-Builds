using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Windows.UI;
using YouTubeMs.Models.Catalog;

namespace YouTubeMs.Services.Catalog;

public sealed class MockCatalogService : ICatalogService
{
    private readonly Lazy<CatalogSnapshot> _snapshot;

    public MockCatalogService()
    {
        _snapshot = new Lazy<CatalogSnapshot>(LoadSnapshot, isThreadSafe: true);
    }

    public ValueTask<FeaturedVideo> GetFeaturedAsync(CancellationToken ct = default)
        => new(_snapshot.Value.Featured);

    public ValueTask<IImmutableList<Video>> GetForYouAsync(CancellationToken ct = default)
        => new(_snapshot.Value.ForYou);

    public ValueTask<IImmutableList<Video>> GetTrendingAsync(CancellationToken ct = default)
        => new(_snapshot.Value.Trending);

    public ValueTask<IImmutableList<RailVideo>> GetRailAsync(CancellationToken ct = default)
        => new(_snapshot.Value.Rail);

    public ValueTask<IImmutableList<SubscriptionItem>> GetSubscriptionsAsync(CancellationToken ct = default)
        => new(_snapshot.Value.Subscriptions);

    public ValueTask<IImmutableList<Video>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new(ImmutableArray<Video>.Empty as IImmutableList<Video>);

        var snap = _snapshot.Value;
        var pool = snap.ForYou.Concat(snap.Trending);

        var results = pool
            .Where(v =>
                v.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || v.Channel.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(v => v.Id)
            .ToImmutableArray();

        return new(results as IImmutableList<Video>);
    }

    private static CatalogSnapshot LoadSnapshot()
    {
        var asm = typeof(MockCatalogService).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("catalog.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Embedded resource catalog.json not found.");

        using var stream = asm.GetManifestResourceStream(resourceName)!;
        var dto = JsonSerializer.Deserialize<MockCatalogDto>(stream, JsonOpts)
            ?? throw new InvalidOperationException("Failed to deserialize catalog.json.");

        var channels = dto.Channels.ToDictionary(
            c => c.Slug,
            c => new Channel(
                c.Name,
                c.Slug,
                c.Initials,
                c.Verified,
                ParseHex(c.AccentTop),
                ParseHex(c.AccentBottom),
                AvatarUrl(c.Slug)),
            StringComparer.OrdinalIgnoreCase);

        var featured = new FeaturedVideo(
            dto.Featured.Id,
            dto.Featured.BadgeText,
            dto.Featured.Title,
            dto.Featured.Description,
            channels[dto.Featured.ChannelSlug],
            TimeSpan.FromSeconds(dto.Featured.DurationSeconds),
            HeroUrl(dto.Featured.Id),
            dto.Featured.YoutubeId);

        var forYou = dto.ForYou.Select(v => MapVideo(v, channels)).ToImmutableArray();
        var trending = dto.Trending.Select(v => MapVideo(v, channels)).ToImmutableArray();
        var rail = dto.Rail
            .Select(r => new RailVideo(
                r.Id,
                r.Title,
                channels[r.ChannelSlug],
                TimeSpan.FromSeconds(180 + r.Id.GetHashCode() % 540),
                ThumbUrl(r.Id, 240, 135),
                r.YoutubeId))
            .ToImmutableArray();

        var subs = dto.Subscriptions
            .Select(s => new SubscriptionItem(channels[s.ChannelSlug], s.HasNew))
            .ToImmutableArray();

        return new CatalogSnapshot(featured, forYou, trending, rail, subs);
    }

    private static Video MapVideo(MockVideoDto v, IDictionary<string, Channel> channels) => new(
        v.Id,
        v.Title,
        channels[v.ChannelSlug],
        TimeSpan.FromSeconds(v.DurationSeconds),
        v.ViewCount,
        DateTimeOffset.UtcNow.AddDays(-v.DaysAgo),
        ThumbUrl(v.Id, 600, 340),
        v.YoutubeId);

    private static string ThumbUrl(string seed, int w, int h)
        => $"https://picsum.photos/seed/{Uri.EscapeDataString(seed)}/{w}/{h}";

    private static string HeroUrl(string seed)
        => $"https://picsum.photos/seed/hero-{Uri.EscapeDataString(seed)}/1600/640";

    private static string AvatarUrl(string slug)
        => $"https://picsum.photos/seed/avatar-{Uri.EscapeDataString(slug)}/96/96";

    private static Color ParseHex(string hex)
    {
        var s = hex.TrimStart('#');
        var r = byte.Parse(s.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = byte.Parse(s.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = byte.Parse(s.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb(0xFF, r, g, b);
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed record CatalogSnapshot(
        FeaturedVideo Featured,
        ImmutableArray<Video> ForYou,
        ImmutableArray<Video> Trending,
        ImmutableArray<RailVideo> Rail,
        ImmutableArray<SubscriptionItem> Subscriptions);
}

internal sealed partial record MockCatalogDto(
    MockChannelDto[] Channels,
    MockFeaturedDto Featured,
    MockVideoDto[] ForYou,
    MockVideoDto[] Trending,
    MockRailDto[] Rail,
    MockSubscriptionDto[] Subscriptions);

internal sealed record MockChannelDto(
    string Name,
    string Slug,
    string Initials,
    bool Verified,
    string AccentTop,
    string AccentBottom);

internal sealed partial record MockFeaturedDto(
    string Id,
    string YoutubeId,
    string BadgeText,
    string Title,
    string Description,
    string ChannelSlug,
    int DurationSeconds);

internal sealed partial record MockVideoDto(
    string Id,
    string YoutubeId,
    string Title,
    string ChannelSlug,
    int DurationSeconds,
    long ViewCount,
    int DaysAgo,
    int? Rank = null);

internal sealed partial record MockRailDto(string Id, string YoutubeId, string Title, string ChannelSlug);

internal sealed record MockSubscriptionDto(string ChannelSlug, bool HasNew);

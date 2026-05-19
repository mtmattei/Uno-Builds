using YouTubeMs.Models.Catalog;

namespace YouTubeMs.Services.Catalog;

public interface ICatalogService
{
    ValueTask<FeaturedVideo> GetFeaturedAsync(CancellationToken ct = default);
    ValueTask<IImmutableList<Video>> GetForYouAsync(CancellationToken ct = default);
    ValueTask<IImmutableList<Video>> GetTrendingAsync(CancellationToken ct = default);
    ValueTask<IImmutableList<RailVideo>> GetRailAsync(CancellationToken ct = default);
    ValueTask<IImmutableList<SubscriptionItem>> GetSubscriptionsAsync(CancellationToken ct = default);
    ValueTask<IImmutableList<Video>> SearchAsync(string query, CancellationToken ct = default);
}

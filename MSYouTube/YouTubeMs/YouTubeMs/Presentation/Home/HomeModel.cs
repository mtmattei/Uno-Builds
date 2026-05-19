using YouTubeMs.Services.Catalog;

namespace YouTubeMs.Presentation.Home;

public partial record HomeModel(ICatalogService Catalog, ILogger<HomeModel> Logger)
{
    public IFeed<FeaturedVideo> Featured =>
        Feed.Async(Catalog.GetFeaturedAsync);

    public IListFeed<Video> ForYou =>
        ListFeed.Async(Catalog.GetForYouAsync);

    public IListFeed<Video> Trending =>
        ListFeed.Async(Catalog.GetTrendingAsync);

    public IListFeed<RailVideo> Recommendations =>
        ListFeed.Async(Catalog.GetRailAsync);

    public async ValueTask Play(Video video)
    {
        Logger.LogInformation("Play requested for {VideoId} '{Title}'", video.Id, video.Title);
        await ValueTask.CompletedTask;
    }

    public async ValueTask PlayFeatured(FeaturedVideo featured)
    {
        Logger.LogInformation("Featured play requested for {VideoId}", featured.Id);
        await ValueTask.CompletedTask;
    }
}

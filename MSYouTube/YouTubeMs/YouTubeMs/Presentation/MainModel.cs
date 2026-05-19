using YouTubeMs.Services.Catalog;

namespace YouTubeMs.Presentation;

public partial record MainModel(ICatalogService Catalog, IStringLocalizer Localizer)
{
    public string Title => Localizer["ApplicationName"];

    public IListFeed<SubscriptionItem> Subscriptions =>
        ListFeed.Async(Catalog.GetSubscriptionsAsync);

    public IState<string> Query => State<string>.Value(this, () => string.Empty);

    public IFeed<bool> IsSearching =>
        Query.Select(q => !string.IsNullOrWhiteSpace(q));

    public IListFeed<Video> SearchResults =>
        Query
            .SelectAsync(async (q, ct) =>
            {
                if (string.IsNullOrWhiteSpace(q)) return ImmutableArray<Video>.Empty as IImmutableList<Video>;
                return await Catalog.SearchAsync(q, ct);
            })
            .AsListFeed();

    public IState<int> UnreadCount => State<int>.Value(this, () => 3);

    public IState<bool> NotifPanelOpen => State<bool>.Value(this, () => false);

    public async ValueTask Search(string query)
    {
        await Query.SetAsync(query ?? string.Empty);
    }

    public async ValueTask ClearSearch()
    {
        await Query.SetAsync(string.Empty);
    }

    public async ValueTask ToggleNotifications()
    {
        var open = await NotifPanelOpen;
        await NotifPanelOpen.SetAsync(!open);
    }
}

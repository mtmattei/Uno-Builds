namespace YouTubeMs.Presentation.Stubs;

public abstract record ComingSoonModelBase(string Title, string Subtitle);

public partial record ExploreModel() : ComingSoonModelBase(
    "Explore",
    "Discover what's trending across categories. Coming next sprint.");

public partial record SubscriptionsModel() : ComingSoonModelBase(
    "Subscriptions",
    "Latest videos from channels you follow. Coming next sprint.");

public partial record LibraryModel() : ComingSoonModelBase(
    "Library",
    "Your playlists and saved videos in one place. Coming next sprint.");

public partial record HistoryModel() : ComingSoonModelBase(
    "History",
    "What you've watched, ready to revisit. Coming next sprint.");

public partial record WatchLaterModel() : ComingSoonModelBase(
    "Watch later",
    "Save videos for later viewing. Coming next sprint.");

public partial record LikedModel() : ComingSoonModelBase(
    "Liked videos",
    "Everything you've given a thumbs up. Coming next sprint.");

using YouTubeMs.Models.Catalog;

namespace YouTubeMs.Services.VideoPlayer;

public static class VideoPlayer
{
    public sealed record VideoOpenRequest(string Title, string ChannelName, string YouTubeId, string EmbedUrl, string WatchUrl, string PosterUrl);

    public static event Action<VideoOpenRequest>? OpenRequested;
    public static event Action? CloseRequested;

    public static void Open(Video v) =>
        OpenRequested?.Invoke(new VideoOpenRequest(v.Title, v.Channel.Name, v.YouTubeId, v.YouTubeEmbedUrl, v.YouTubeWatchUrl, v.ThumbnailUrl));

    public static void Open(RailVideo v) =>
        OpenRequested?.Invoke(new VideoOpenRequest(v.Title, v.Channel.Name, v.YouTubeId, v.YouTubeEmbedUrl, v.YouTubeWatchUrl, v.ThumbnailUrl));

    public static void Open(FeaturedVideo v) =>
        OpenRequested?.Invoke(new VideoOpenRequest(v.Title, v.Channel.Name, v.YouTubeId, v.YouTubeEmbedUrl, v.YouTubeWatchUrl, v.BackgroundImageUrl));

    public static void Close() => CloseRequested?.Invoke();
}

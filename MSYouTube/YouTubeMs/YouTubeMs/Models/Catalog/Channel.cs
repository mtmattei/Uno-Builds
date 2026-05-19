using Windows.UI;

namespace YouTubeMs.Models.Catalog;

public record Channel(
    string Name,
    string Slug,
    string Initials,
    bool IsVerified,
    Color AccentTop,
    Color AccentBottom,
    string AvatarUrl);

namespace HockeyBarn.Models;

public partial record Notification
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public bool IsRead { get; init; }
    public string RelatedGameId { get; init; } = string.Empty;
}

public enum NotificationType
{
    Alert,
    GameInvite,
    Confirmation,
    Message
}

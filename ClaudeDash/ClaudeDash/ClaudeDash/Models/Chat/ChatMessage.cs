namespace ClaudeDash.Models.Chat;

public partial record ChatMessage(
    string Id = "",
    ChatRole Role = ChatRole.User,
    string Text = "",
    DateTime Timestamp = default,
    bool IsStreaming = false,
    ImmutableList<ContentCard>? Cards = null)
{
    public string Id { get; init; } = string.IsNullOrEmpty(Id) ? Guid.NewGuid().ToString("N")[..8] : Id;
    public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.Now : Timestamp;
    public ImmutableList<ContentCard> Cards { get; init; } = Cards ?? ImmutableList<ContentCard>.Empty;
}

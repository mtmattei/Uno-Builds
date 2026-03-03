namespace MsnMessenger.Models;

public enum MessageType
{
    Text,
    Wink,
    Nudge
}

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public MessageType Type { get; set; } = MessageType.Text;
    public string? WinkEmoji { get; set; }

    public bool IsSentByMe => SenderId == "me";
    public string TimeDisplay => Timestamp.ToString("h:mm tt");
}

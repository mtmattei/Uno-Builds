namespace MsnMessenger.Models;

public class Chat
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Contact Contact { get; set; } = new();
    public DateTime LastMessageTime { get; set; } = DateTime.Now;
    public int UnreadCount { get; set; }

    public string TimeDisplay
    {
        get
        {
            var diff = DateTime.Now - LastMessageTime;
            if (diff.TotalMinutes < 1) return "now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h";
            return LastMessageTime.ToString("MMM d");
        }
    }
}

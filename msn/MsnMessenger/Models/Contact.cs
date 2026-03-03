namespace MsnMessenger.Models;

public class Contact
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DisplayName { get; set; } = string.Empty;
    public PresenceStatus Status { get; set; } = PresenceStatus.Offline;
    public string PersonalMessage { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? FrameColor { get; set; }
    public int UnreadCount { get; set; } = 0;
    public NowPlaying? NowPlaying { get; set; }

    public string Initials
    {
        get
        {
            if (string.IsNullOrEmpty(DisplayName))
                return "?";

            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts
                .Take(2)
                .Select(s => s.FirstOrDefault(c => char.IsLetter(c) || char.IsDigit(c)))
                .Where(c => c != default)
                .Select(c => char.ToUpper(c));

            var result = string.Concat(initials);
            return string.IsNullOrEmpty(result) ? "?" : result;
        }
    }
}

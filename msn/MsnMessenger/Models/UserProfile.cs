namespace MsnMessenger.Models;

public class UserProfile
{
    public string DisplayName { get; set; } = "~*You*~";
    public PresenceStatus Status { get; set; } = PresenceStatus.Online;
    public string PersonalMessage { get; set; } = string.Empty;
    public string Email { get; set; } = "user@msn.com";
    public int BuddyCount { get; set; }
    public int MessageCount { get; set; }
    public int NudgeCount { get; set; }
}

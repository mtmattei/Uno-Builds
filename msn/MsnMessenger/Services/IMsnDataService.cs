using MsnMessenger.Models;
using System.Collections.ObjectModel;

namespace MsnMessenger.Services;

public interface IMsnDataService
{
    UserProfile CurrentUser { get; }
    ObservableCollection<ContactGroup> Groups { get; }
    ObservableCollection<Chat> RecentChats { get; }

    void UpdateUserProfile(UserProfile profile);
    void UpdateUserStatus(PresenceStatus status);
    void UpdateDisplayName(string name);
    void UpdatePersonalMessage(string message);
    List<Message> GetMessagesForContact(string contactId);
    void SendMessage(string contactId, string text);
    void SendWink(string contactId, string emoji);
    void SendNudge(string contactId);
    LiveActivity? GetActivityForContact(string contactId);
}

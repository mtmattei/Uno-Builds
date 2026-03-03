using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MsnMessenger.Models;

public class ContactGroup : INotifyPropertyChanged
{
    private bool _isExpanded = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public ObservableCollection<Contact> Contacts { get; set; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContactsVisibility)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ArrowText)));
            }
        }
    }

    public Visibility ContactsVisibility => IsExpanded ? Visibility.Visible : Visibility.Collapsed;
    public string ArrowText => IsExpanded ? "▼" : "▶";

    public int OnlineCount => Contacts.Count(c => c.Status != PresenceStatus.Offline);
    public int TotalCount => Contacts.Count;
    public string CountDisplay => $"{OnlineCount}/{TotalCount}";
}

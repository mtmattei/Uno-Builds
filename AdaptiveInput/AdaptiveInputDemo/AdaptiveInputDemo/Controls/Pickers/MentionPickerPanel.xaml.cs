using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AdaptiveInputDemo.Controls;

public sealed partial class MentionPickerPanel : UserControl
{
    private readonly List<ContactItem> _allContacts;

    public event EventHandler<string>? MentionSelected;

    public MentionPickerPanel()
    {
        InitializeComponent();

        // Sample contacts - in a real app, this would come from a data source
        _allContacts = new List<ContactItem>
        {
            new("Alice Johnson", "alice", "Engineering", ContactStatus.Online),
            new("Bob Smith", "bob", "Design", ContactStatus.Away),
            new("Carol Williams", "carol", "Product", ContactStatus.Online),
            new("David Brown", "david", "Marketing", ContactStatus.Offline),
            new("Emma Davis", "emma", "Engineering", ContactStatus.Online),
            new("Frank Miller", "frank", "Sales", ContactStatus.Away),
            new("Grace Lee", "grace", "Design", ContactStatus.Online),
            new("Henry Wilson", "henry", "Engineering", ContactStatus.Offline),
            new("Ivy Chen", "ivy", "Product", ContactStatus.Online),
            new("Jack Taylor", "jack", "Marketing", ContactStatus.Away)
        };

        ContactList.ItemsSource = _allContacts;
    }

    public void UpdateFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            ContactList.ItemsSource = _allContacts;
            UpdateEmptyState(false);
            return;
        }

        var filtered = _allContacts
            .Where(c => c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.Handle.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ContactList.ItemsSource = filtered;
        UpdateEmptyState(filtered.Count == 0);
    }

    private void UpdateEmptyState(bool isEmpty)
    {
        EmptyState.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        ContactList.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnContactClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ContactItem contact)
        {
            MentionSelected?.Invoke(this, contact.Handle);
        }
    }
}

public enum ContactStatus
{
    Online,
    Away,
    Offline
}

public class ContactItem
{
    // Cached brushes for performance - shared across all instances
    private static readonly SolidColorBrush OnlineBrush = new(Color.FromArgb(255, 34, 197, 94));
    private static readonly SolidColorBrush AwayBrush = new(Color.FromArgb(255, 251, 191, 36));
    private static readonly SolidColorBrush OfflineBrush = new(Color.FromArgb(255, 148, 163, 184));

    public string Name { get; }
    public string Handle { get; }
    public string Role { get; }
    public ContactStatus Status { get; }
    public string Initials { get; }

    public SolidColorBrush StatusColor => Status switch
    {
        ContactStatus.Online => OnlineBrush,
        ContactStatus.Away => AwayBrush,
        _ => OfflineBrush
    };

    public ContactItem(string name, string handle, string role, ContactStatus status)
    {
        Name = name;
        Handle = handle;
        Role = role;
        Status = status;
        // Compute initials once in constructor instead of every property access
        Initials = string.Concat(name.Split(' ').Take(2).Select(n => n.Length > 0 ? n[0].ToString() : ""));
    }
}

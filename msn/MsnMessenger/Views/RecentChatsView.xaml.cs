using MsnMessenger.Models;
using MsnMessenger.Services;

namespace MsnMessenger.Views;

public sealed partial class RecentChatsView : UserControl
{
    private IMsnDataService? _dataService;

    public event Action<Chat>? OnChatSelected;

    public IMsnDataService? DataService
    {
        get => _dataService;
        set
        {
            _dataService = value;
            if (_dataService != null)
            {
                BindData();
            }
        }
    }

    public RecentChatsView()
    {
        this.InitializeComponent();
    }

    private void BindData()
    {
        if (_dataService == null) return;

        ChatsList.ItemsSource = _dataService.RecentChats;

        if (_dataService.RecentChats.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
        }
        else
        {
            EmptyState.Visibility = Visibility.Collapsed;
        }
    }

    private void OnChatClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Chat chat)
        {
            OnChatSelected?.Invoke(chat);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsnMessenger.Models;
using MsnMessenger.Services;
using System.Collections.ObjectModel;

namespace MsnMessenger.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMsnDataService _dataService;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private Contact? _selectedContact;

    [ObservableProperty]
    private bool _isInChat;

    public UserProfile CurrentUser => _dataService.CurrentUser;
    public ObservableCollection<ContactGroup> Groups => _dataService.Groups;
    public ObservableCollection<Chat> RecentChats => _dataService.RecentChats;

    public int TotalOnlineCount => Groups.Sum(g => g.OnlineCount);
    public int TotalContactCount => Groups.Sum(g => g.TotalCount);

    public MainViewModel(IMsnDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private void SelectContact(Contact contact)
    {
        SelectedContact = contact;
        IsInChat = true;
    }

    [RelayCommand]
    private void BackFromChat()
    {
        IsInChat = false;
        SelectedContact = null;
    }

    [RelayCommand]
    private void SelectChat(Chat chat)
    {
        SelectedContact = chat.Contact;
        chat.UnreadCount = 0;
        IsInChat = true;
    }
}

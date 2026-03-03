using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsnMessenger.Models;
using MsnMessenger.Services;
using System.Collections.ObjectModel;

namespace MsnMessenger.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly IMsnDataService _dataService;

    [ObservableProperty]
    private Contact? _contact;

    [ObservableProperty]
    private string _messageInput = string.Empty;

    [ObservableProperty]
    private bool _showWinkPanel;

    [ObservableProperty]
    private bool _isNudgeAnimating;

    [ObservableProperty]
    private ObservableCollection<Message> _messages = new();

    public string[] WinkEmojis { get; } = new[]
    {
        "😘", "🎸", "💃", "🎉",
        "❤️‍🔥", "😎", "🌈", "⚡",
        "🦋", "🎵", "☕", "🔥"
    };

    public bool CanSend => !string.IsNullOrWhiteSpace(MessageInput);
    public bool ShowNudgeButton => string.IsNullOrWhiteSpace(MessageInput);

    public ChatViewModel(IMsnDataService dataService)
    {
        _dataService = dataService;
    }

    public void LoadContact(Contact contact)
    {
        Contact = contact;
        var existingMessages = _dataService.GetMessagesForContact(contact.Id);
        Messages = new ObservableCollection<Message>(existingMessages);
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (Contact == null || string.IsNullOrWhiteSpace(MessageInput))
            return;

        _dataService.SendMessage(Contact.Id, MessageInput);

        Messages.Add(new Message
        {
            Text = MessageInput,
            SenderId = "me",
            Type = MessageType.Text
        });

        MessageInput = string.Empty;
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(ShowNudgeButton));
    }

    [RelayCommand]
    private void SendWink(string emoji)
    {
        if (Contact == null) return;

        _dataService.SendWink(Contact.Id, emoji);

        Messages.Add(new Message
        {
            Text = emoji,
            SenderId = "me",
            Type = MessageType.Wink,
            WinkEmoji = emoji
        });

        ShowWinkPanel = false;
    }

    [RelayCommand]
    private async Task SendNudge()
    {
        if (Contact == null) return;

        _dataService.SendNudge(Contact.Id);

        Messages.Add(new Message
        {
            Text = "👊 Nudge!",
            SenderId = "me",
            Type = MessageType.Nudge
        });

        IsNudgeAnimating = true;
        await Task.Delay(500);
        IsNudgeAnimating = false;
    }

    [RelayCommand]
    private void ToggleWinkPanel()
    {
        ShowWinkPanel = !ShowWinkPanel;
    }

    partial void OnMessageInputChanged(string value)
    {
        OnPropertyChanged(nameof(CanSend));
        OnPropertyChanged(nameof(ShowNudgeButton));
    }
}

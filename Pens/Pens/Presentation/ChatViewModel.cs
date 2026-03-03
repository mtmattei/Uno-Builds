using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Pens.Models;
using Pens.Services;

namespace Pens.Presentation;

public partial class ChatViewModel : ObservableObject, IDisposable
{
    private readonly ISupabaseService _supabase;
    private readonly IPlayerIdentityService _identity;
    private readonly ILogger<ChatViewModel> _logger;
    private readonly DispatcherQueue _dispatcher;
    private readonly CancellationTokenSource _cts = new();

    public ChatViewModel(ISupabaseService supabase, IPlayerIdentityService identity, ILogger<ChatViewModel> logger)
    {
        _supabase = supabase;
        _identity = identity;
        _logger = logger;
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _ = LoadMessagesAsync();
    }

    [ObservableProperty]
    private string _messageText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    private async Task LoadMessagesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var dbMessages = await _supabase.GetChatMessagesAsync(50);

            foreach (var msg in dbMessages)
            {
                Messages.Add(ToViewModel(msg));
            }

            // Subscribe to new messages using polling with cancellation support
            await _supabase.SubscribeToChatMessagesAsync(OnNewMessage, _cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading messages");
            ErrorMessage = "Failed to load messages";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnNewMessage(DbChatMessage msg)
    {
        _dispatcher.TryEnqueue(() =>
        {
            // Avoid duplicates
            if (!Messages.Any(m => m.Message == msg.Message && m.Sender == msg.PlayerName))
            {
                Messages.Add(ToViewModel(msg));
            }
        });
    }

    private static ChatMessage ToViewModel(DbChatMessage msg)
    {
        var initials = GetInitials(msg.PlayerName);
        var time = msg.CreatedAt.ToLocalTime().ToString("h:mm tt");
        return new ChatMessage(initials, msg.PlayerName, time, msg.Message);
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0][..2].ToUpper();
        return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        var messageToSend = MessageText.Trim();
        if (messageToSend.Length > 500)
        {
            ErrorMessage = "Message too long (max 500 characters)";
            return;
        }

        MessageText = string.Empty;
        ErrorMessage = null;

        try
        {
            var playerName = _identity.CurrentPlayerName ?? "Unknown";
            await _supabase.SendChatMessageAsync(_identity.CurrentPlayerId, playerName, messageToSend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            MessageText = messageToSend;
            ErrorMessage = "Failed to send message";
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _supabase.UnsubscribeFromChat();
    }
}

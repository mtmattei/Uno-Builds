using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Controls;
using MsnMessenger.Helpers;
using MsnMessenger.Models;
using MsnMessenger.Services;
using System.Collections.ObjectModel;
using Windows.UI;
using LiveActivityCard = MsnMessenger.Controls.LiveActivityCard;

namespace MsnMessenger.Views;

public sealed partial class ChatView : UserControl
{
    private IMsnDataService? _dataService;
    private Contact? _contact;
    private ObservableCollection<Message> _messages = new();

    // UI element references
    private AvatarControl? _contactAvatar;
    private TextBlock? _contactNameText;
    private TextBlock? _contactStatusText;
    private ScrollViewer? _messagesScroller;
    private ItemsControl? _messagesList;
    private Border? _winkPanel;
    private TextBox? _messageInput;
    private Button? _sendButton;
    private FontIcon? _sendButtonIcon;
    private Grid? _chatContainer;
    private LiveActivityCard? _activityCard;

    public event Action? OnBackRequested;

    public IMsnDataService? DataService
    {
        get => _dataService;
        set => _dataService = value;
    }

    public ChatView()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find elements after loading
        _contactAvatar = FindName("ContactAvatar") as AvatarControl;
        _contactNameText = FindName("ContactNameText") as TextBlock;
        _contactStatusText = FindName("ContactStatusText") as TextBlock;
        _messagesScroller = FindName("MessagesScroller") as ScrollViewer;
        _messagesList = FindName("MessagesList") as ItemsControl;
        _winkPanel = FindName("WinkPanel") as Border;
        _messageInput = FindName("MessageInput") as TextBox;
        _sendButton = FindName("SendButton") as Button;
        _sendButtonIcon = FindName("SendButtonIcon") as FontIcon;
        _chatContainer = FindName("ChatContainer") as Grid;
        _activityCard = FindName("ActivityCard") as LiveActivityCard;
    }

    public void LoadContact(Contact contact)
    {
        _contact = contact;

        if (_contactAvatar != null)
        {
            _contactAvatar.Initials = contact.Initials;
            _contactAvatar.Status = contact.Status;
            _contactAvatar.FrameColor = contact.FrameColor;
        }

        if (_contactNameText != null)
            _contactNameText.Text = contact.DisplayName;

        if (_contactStatusText != null)
            _contactStatusText.Text = !string.IsNullOrEmpty(contact.PersonalMessage)
                ? contact.PersonalMessage
                : contact.Status.ToString();

        if (_dataService != null && _messagesList != null)
        {
            var existingMessages = _dataService.GetMessagesForContact(contact.Id);
            _messages = new ObservableCollection<Message>(existingMessages);
            _messagesList.ItemsSource = _messages;
        }

        // Load activity data for the contact
        LoadContactActivity(contact);

        UpdateSendButton();
    }

    private void LoadContactActivity(Contact contact)
    {
        if (_activityCard == null || _dataService == null) return;

        var activity = _dataService.GetActivityForContact(contact.Id);
        _activityCard.SetActivity(activity);
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        OnBackRequested?.Invoke();
    }

    private void OnMessageTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSendButton();
    }

    private void UpdateSendButton()
    {
        if (_messageInput == null || _sendButtonIcon == null || _sendButton == null) return;

        var hasText = !string.IsNullOrWhiteSpace(_messageInput.Text);

        // Use Segoe Fluent Icons: E724 is Send, E7F0 is Hand/Nudge
        _sendButtonIcon.Glyph = hasText ? "\uE724" : "\uE7F0";

        if (hasText)
        {
            _sendButton.Background = (Brush)Application.Current.Resources["ChatSendButtonGradientBrush"];
            _sendButtonIcon.Foreground = new SolidColorBrush(Colors.White);
        }
        else
        {
            // Glass background when no text (nudge mode)
            _sendButton.Background = (Brush)Application.Current.Resources["GlassBackgroundBrush"];
            _sendButtonIcon.Foreground = new SolidColorBrush(Colors.White);
        }
    }

    private void OnMessageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_messageInput != null && e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(_messageInput.Text))
        {
            SendMessage();
            e.Handled = true;
        }
    }

    private void OnSendOrNudgeClick(object sender, RoutedEventArgs e)
    {
        if (_messageInput != null && !string.IsNullOrWhiteSpace(_messageInput.Text))
        {
            SendMessage();
        }
        else
        {
            SendNudge();
        }
    }

    private async void SendMessage()
    {
        if (_contact == null || _dataService == null || _messageInput == null || string.IsNullOrWhiteSpace(_messageInput.Text))
            return;

        // Animate send button press
        if (_sendButton != null)
        {
            await MicroAnimations.AnimatePress(_sendButton);
        }

        var text = _messageInput.Text;
        _dataService.SendMessage(_contact.Id, text);

        var newMessage = new Message
        {
            Text = text,
            SenderId = "me",
            Type = MessageType.Text
        };
        _messages.Add(newMessage);

        _messageInput.Text = "";

        // Animate the new message entrance after a brief delay
        await Task.Delay(50);
        AnimateLastMessage();
        ScrollToBottom();
    }

    private void AnimateLastMessage()
    {
        if (_messagesList == null) return;

        // Get the last item container and animate it
        var container = _messagesList.ContainerFromIndex(_messages.Count - 1) as UIElement;
        if (container != null)
        {
            MicroAnimations.AnimatePopIn(container);
        }
    }

    private async void SendNudge()
    {
        if (_contact == null || _dataService == null) return;

        // Animate send button press
        if (_sendButton != null)
        {
            await MicroAnimations.AnimatePress(_sendButton);
        }

        _dataService.SendNudge(_contact.Id);

        _messages.Add(new Message
        {
            Text = "👊 Nudge!",
            SenderId = "me",
            Type = MessageType.Nudge
        });

        // Animate the new message
        await Task.Delay(50);
        AnimateLastMessage();

        // Then shake the whole chat
        await MicroAnimations.AnimateShake(_chatContainer!);
        ScrollToBottom();
    }

    private void OnNudgeClick(object sender, RoutedEventArgs e)
    {
        SendNudge();
    }

    private void OnToggleWinkPanel(object sender, RoutedEventArgs e)
    {
        if (_winkPanel != null)
        {
            _winkPanel.Visibility = _winkPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private void OnCloseWinkPanel(object sender, RoutedEventArgs e)
    {
        if (_winkPanel != null)
            _winkPanel.Visibility = Visibility.Collapsed;
    }

    private async void OnWinkClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string emoji && _contact != null && _dataService != null)
        {
            // Animate the wink button
            await MicroAnimations.AnimatePress(btn);

            _dataService.SendWink(_contact.Id, emoji);

            _messages.Add(new Message
            {
                Text = emoji,
                SenderId = "me",
                Type = MessageType.Wink,
                WinkEmoji = emoji
            });

            if (_winkPanel != null)
                _winkPanel.Visibility = Visibility.Collapsed;

            // Animate the new message
            await Task.Delay(50);
            AnimateLastMessage();
            ScrollToBottom();
        }
    }

    private async void OnEmojiClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string emoji && _messageInput != null)
        {
            // Animate the emoji button
            await MicroAnimations.AnimatePress(btn);

            // Append emoji to current message text
            _messageInput.Text += emoji;
            _messageInput.Focus(FocusState.Programmatic);
            _messageInput.SelectionStart = _messageInput.Text.Length;
        }
    }

    private void ScrollToBottom()
    {
        _messagesScroller?.ChangeView(null, _messagesScroller.ScrollableHeight, null);
    }
}

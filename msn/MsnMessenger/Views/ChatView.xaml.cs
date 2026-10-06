using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MsnMessenger.Helpers;
using MsnMessenger.Models;
using MsnMessenger.Services;
using System.Collections.ObjectModel;
using Windows.UI;

namespace MsnMessenger.Views;

public sealed partial class ChatView : UserControl
{
    private IMsnDataService? _dataService;
    private Contact? _contact;
    private ObservableCollection<Message> _messages = new();

    public event Action? OnBackRequested;

    public IMsnDataService? DataService
    {
        get => _dataService;
        set => _dataService = value;
    }

    public ChatView()
    {
        this.InitializeComponent();
    }

    public void LoadContact(Contact contact)
    {
        _contact = contact;

        ContactAvatar.Initials = contact.Initials;
        ContactAvatar.Status = contact.Status;
        ContactAvatar.FrameColor = contact.FrameColor;

        ContactNameText.Text = contact.DisplayName;
        ContactStatusText.Text = !string.IsNullOrEmpty(contact.PersonalMessage)
            ? contact.PersonalMessage
            : contact.Status.ToString();

        if (_dataService is not null)
        {
            var existingMessages = _dataService.GetMessagesForContact(contact.Id);
            _messages = new ObservableCollection<Message>(existingMessages);
            MessagesList.ItemsSource = _messages;
        }

        LoadContactActivity(contact);
        UpdateSendButton();
    }

    private void LoadContactActivity(Contact contact)
    {
        if (_dataService is null) return;

        var activity = _dataService.GetActivityForContact(contact.Id);
        ActivityCard.SetActivity(activity);
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
        var hasText = !string.IsNullOrWhiteSpace(MessageInput.Text);

        // E724 = Send glyph, E7F0 = Hand/Nudge glyph.
        SendButtonIcon.Glyph = hasText ? "" : "";

        SendButton.Background = hasText
            ? (Brush)Application.Current.Resources["ChatSendButtonGradientBrush"]
            : (Brush)Application.Current.Resources["GlassBackgroundBrush"];

        SendButtonIcon.Foreground = new SolidColorBrush(Colors.White);
    }

    private void OnMessageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(MessageInput.Text))
        {
            SendMessage();
            e.Handled = true;
        }
    }

    private void OnSendOrNudgeClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(MessageInput.Text))
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
        if (_contact is null || _dataService is null || string.IsNullOrWhiteSpace(MessageInput.Text))
            return;

        await MicroAnimations.AnimatePress(SendButton);

        var text = MessageInput.Text;
        _dataService.SendMessage(_contact.Id, text);

        _messages.Add(new Message
        {
            Text = text,
            SenderId = "me",
            Type = MessageType.Text,
        });

        MessageInput.Text = "";

        await Task.Delay(50);
        AnimateLastMessage();
        ScrollToBottom();
    }

    private void AnimateLastMessage()
    {
        var container = MessagesList.ContainerFromIndex(_messages.Count - 1) as UIElement;
        if (container is not null)
        {
            MicroAnimations.AnimatePopIn(container);
        }
    }

    private async void SendNudge()
    {
        if (_contact is null || _dataService is null) return;

        await MicroAnimations.AnimatePress(SendButton);

        _dataService.SendNudge(_contact.Id);

        _messages.Add(new Message
        {
            Text = "👊 Nudge!",
            SenderId = "me",
            Type = MessageType.Nudge,
        });

        await Task.Delay(50);
        AnimateLastMessage();

        await MicroAnimations.AnimateShake(ChatContainer);
        ScrollToBottom();
    }

    private void OnNudgeClick(object sender, RoutedEventArgs e)
    {
        SendNudge();
    }

    private void OnToggleWinkPanel(object sender, RoutedEventArgs e)
    {
        WinkPanel.Visibility = WinkPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void OnCloseWinkPanel(object sender, RoutedEventArgs e)
    {
        WinkPanel.Visibility = Visibility.Collapsed;
    }

    private async void OnWinkClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string emoji && _contact is not null && _dataService is not null)
        {
            await MicroAnimations.AnimatePress(btn);

            _dataService.SendWink(_contact.Id, emoji);

            _messages.Add(new Message
            {
                Text = emoji,
                SenderId = "me",
                Type = MessageType.Wink,
                WinkEmoji = emoji,
            });

            WinkPanel.Visibility = Visibility.Collapsed;

            await Task.Delay(50);
            AnimateLastMessage();
            ScrollToBottom();
        }
    }

    private async void OnEmojiClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string emoji)
        {
            await MicroAnimations.AnimatePress(btn);

            MessageInput.Text += emoji;
            MessageInput.Focus(FocusState.Programmatic);
            MessageInput.SelectionStart = MessageInput.Text.Length;
        }
    }

    private void ScrollToBottom()
    {
        MessagesScroller.ChangeView(null, MessagesScroller.ScrollableHeight, null);
    }
}

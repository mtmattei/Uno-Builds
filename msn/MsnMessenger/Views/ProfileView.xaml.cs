using MsnMessenger.Models;
using MsnMessenger.Services;

namespace MsnMessenger.Views;

public sealed partial class ProfileView : UserControl
{
    private IMsnDataService? _dataService;
    private bool _isEditingName;
    private bool _isEditingMessage;

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

    public ProfileView()
    {
        this.InitializeComponent();
    }

    private void BindData()
    {
        if (_dataService == null) return;

        var user = _dataService.CurrentUser;

        ProfileAvatar.Initials = GetInitials(user.DisplayName);
        ProfileAvatar.Status = user.Status;

        DisplayNameText.Text = user.DisplayName;
        PersonalMessageText.Text = string.IsNullOrEmpty(user.PersonalMessage)
            ? "What's on your mind?"
            : user.PersonalMessage;

        BuddyCountText.Text = user.BuddyCount.ToString();
        MessageCountText.Text = FormatCount(user.MessageCount);
        NudgeCountText.Text = user.NudgeCount.ToString();
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        return string.Concat(parts.Take(2).Select(p =>
        {
            var letter = p.FirstOrDefault(c => char.IsLetter(c) || char.IsDigit(c));
            return letter == default ? '?' : char.ToUpper(letter);
        }));
    }

    private static string FormatCount(int count)
    {
        if (count >= 1000)
            return $"{count / 1000.0:0.#}k";
        return count.ToString();
    }

    private void OnEditNameClick(object sender, RoutedEventArgs e)
    {
        _isEditingName = true;
        NameInput.Text = _dataService?.CurrentUser.DisplayName ?? "";
        DisplayNameButton.Visibility = Visibility.Collapsed;
        EditNamePanel.Visibility = Visibility.Visible;
        NameInput.Focus(FocusState.Programmatic);
    }

    private void OnSaveNameClick(object sender, RoutedEventArgs e)
    {
        if (_dataService != null && !string.IsNullOrWhiteSpace(NameInput.Text))
        {
            _dataService.UpdateDisplayName(NameInput.Text);
            DisplayNameText.Text = NameInput.Text;
            ProfileAvatar.Initials = GetInitials(NameInput.Text);
        }
        _isEditingName = false;
        DisplayNameButton.Visibility = Visibility.Visible;
        EditNamePanel.Visibility = Visibility.Collapsed;
    }

    private void OnCancelNameClick(object sender, RoutedEventArgs e)
    {
        _isEditingName = false;
        DisplayNameButton.Visibility = Visibility.Visible;
        EditNamePanel.Visibility = Visibility.Collapsed;
    }

    private void OnEditMessageClick(object sender, RoutedEventArgs e)
    {
        _isEditingMessage = true;
        MessageInput.Text = _dataService?.CurrentUser.PersonalMessage ?? "";
        MessageButton.Visibility = Visibility.Collapsed;
        EditMessagePanel.Visibility = Visibility.Visible;
        MessageInput.Focus(FocusState.Programmatic);
    }

    private void OnSaveMessageClick(object sender, RoutedEventArgs e)
    {
        if (_dataService != null)
        {
            _dataService.UpdatePersonalMessage(MessageInput.Text);
            PersonalMessageText.Text = string.IsNullOrEmpty(MessageInput.Text)
                ? "What's on your mind?"
                : MessageInput.Text;
        }
        _isEditingMessage = false;
        MessageButton.Visibility = Visibility.Visible;
        EditMessagePanel.Visibility = Visibility.Collapsed;
    }

    private void OnCancelMessageClick(object sender, RoutedEventArgs e)
    {
        _isEditingMessage = false;
        MessageButton.Visibility = Visibility.Visible;
        EditMessagePanel.Visibility = Visibility.Collapsed;
    }

    private void OnQuickMessageClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string message && _dataService != null)
        {
            _dataService.UpdatePersonalMessage(message);
            PersonalMessageText.Text = message;
        }
    }

    private void OnStatusClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string statusStr && _dataService != null)
        {
            if (Enum.TryParse<PresenceStatus>(statusStr, out var status))
            {
                _dataService.UpdateUserStatus(status);
                ProfileAvatar.Status = status;
            }
        }
    }
}

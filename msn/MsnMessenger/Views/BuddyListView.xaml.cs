using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using MsnMessenger.Converters;
using MsnMessenger.Helpers;
using MsnMessenger.Models;
using MsnMessenger.Services;
using Windows.System;

namespace MsnMessenger.Views;

public sealed partial class BuddyListView : UserControl
{
    private IMsnDataService? _dataService;
    private bool _isEditingName;
    private bool _isEditingMessage;
    private NowPlaying? _currentTrack;
    private DispatcherTimer? _progressTimer;
    private Storyboard? _breathingStoryboard;
    private Storyboard? _pulseStoryboard;
    private bool _animationsStarted;

    public event Action<Contact>? OnContactSelected;

    public IMsnDataService? DataService
    {
        get => _dataService;
        set
        {
            _dataService = value;
            if (_dataService is not null)
            {
                BindData();
            }
        }
    }

    public BuddyListView()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await AnimateHeaderEntrance();
        InitializeNowPlaying();
        StartAmbientAnimations();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_progressTimer is not null)
        {
            _progressTimer.Stop();
            _progressTimer.Tick -= OnProgressTimerTick;
            _progressTimer = null;
        }

        _breathingStoryboard?.Stop();
        _breathingStoryboard = null;
        _pulseStoryboard?.Stop();
        _pulseStoryboard = null;
        _animationsStarted = false;
    }

    private async Task AnimateHeaderEntrance()
    {
        await Task.Delay(100);
        MicroAnimations.AnimateEntrance(UserAvatar, 0);
        await Task.Delay(50);
    }

    private void StartAmbientAnimations()
    {
        if (_animationsStarted) return;
        _animationsStarted = true;

        if (_dataService?.CurrentUser.Status == PresenceStatus.Online)
        {
            _pulseStoryboard = MicroAnimations.AnimatePulse(ProfileStatusDot, 1.15);
        }

        if (NowPlayingCard is not null)
        {
            StartNowPlayingBreathing();
        }
    }

    private void StartNowPlayingBreathing()
    {
        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 0.8, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(2000), Value = 1.0, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(4000), Value = 0.8, EasingFunction = ease });
        Storyboard.SetTarget(anim, NowPlayingCard);
        Storyboard.SetTargetProperty(anim, "Opacity");

        _breathingStoryboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        _breathingStoryboard.Children.Add(anim);
        _breathingStoryboard.Begin();
    }

    private void InitializeNowPlaying()
    {
        _currentTrack = MockSpotifyData.GetCurrentTrack();
        UpdateNowPlayingUI();

        _progressTimer?.Stop();
        _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _progressTimer.Tick += OnProgressTimerTick;
        _progressTimer.Start();
    }

    private void OnProgressTimerTick(object? sender, object e)
    {
        if (_currentTrack is null || !_currentTrack.IsPlaying) return;

        _currentTrack.Progress = _currentTrack.Progress.Add(TimeSpan.FromSeconds(1));

        if (_currentTrack.Progress >= _currentTrack.Duration)
        {
            _currentTrack = MockSpotifyData.GetRandomTrack();
            UpdateNowPlayingUI();
        }
        else
        {
            UpdateProgressBar();
        }
    }

    private void UpdateNowPlayingUI()
    {
        if (_currentTrack is null) return;

        TrackNameText.Text = _currentTrack.TrackName;
        ArtistNameText.Text = _currentTrack.ArtistName;

        if (!string.IsNullOrEmpty(_currentTrack.AlbumArtUrl))
        {
            AlbumArtImage.Source = new BitmapImage(new Uri(_currentTrack.AlbumArtUrl));
        }

        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (_currentTrack is null || ProgressBar is null) return;

        const double maxWidth = 120.0;
        var progressWidth = (_currentTrack.ProgressPercent / 100.0) * maxWidth;
        ProgressBar.Width = Math.Max(4, progressWidth);
    }

    private void BindData()
    {
        if (_dataService is null) return;

        var user = _dataService.CurrentUser;
        DisplayNameText.Text = user.DisplayName;
        StatusText.Text = user.Status.ToString();
        ProfileStatusDot.Fill = StatusBrushes.ForStatus(user.Status);

        PersonalMessageText.Text = string.IsNullOrEmpty(user.PersonalMessage)
            ? "What's on your mind?"
            : user.PersonalMessage;

        var totalOnline = _dataService.Groups.Sum(g => g.OnlineCount);
        var totalContacts = _dataService.Groups.Sum(g => g.TotalCount);
        OnlineCountText.Text = $"{totalOnline}/{totalContacts} online";

        GroupsList.ItemsSource = _dataService.Groups;
    }

    private async void OnGroupHeaderClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ContactGroup group)
        {
            await MicroAnimations.AnimatePress(btn);
            group.IsExpanded = !group.IsExpanded;
        }
    }

    private async void OnContactClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Contact contact)
        {
            await MicroAnimations.AnimatePress(btn);
            OnContactSelected?.Invoke(contact);
        }
    }

    private void OnDisplayNameClick(object sender, RoutedEventArgs e)
    {
        if (_isEditingName) return;

        _isEditingName = true;
        DisplayNameEditBox.Text = DisplayNameText.Text;
        DisplayNameButton.Visibility = Visibility.Collapsed;
        DisplayNameEditBox.Visibility = Visibility.Visible;
        DisplayNameEditBox.Focus(FocusState.Programmatic);
        DisplayNameEditBox.SelectAll();
    }

    private void OnDisplayNameKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            SaveDisplayName();
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.Escape)
        {
            CancelDisplayNameEdit();
            e.Handled = true;
        }
    }

    private void OnDisplayNameLostFocus(object sender, RoutedEventArgs e)
    {
        if (_isEditingName)
        {
            SaveDisplayName();
        }
    }

    private void SaveDisplayName()
    {
        if (!_isEditingName) return;

        var newName = DisplayNameEditBox.Text?.Trim();
        if (!string.IsNullOrEmpty(newName) && _dataService is not null)
        {
            _dataService.CurrentUser.DisplayName = newName;
            DisplayNameText.Text = newName;
        }

        CancelDisplayNameEdit();
    }

    private void CancelDisplayNameEdit()
    {
        _isEditingName = false;
        DisplayNameEditBox.Visibility = Visibility.Collapsed;
        DisplayNameButton.Visibility = Visibility.Visible;
    }

    private void OnPersonalMessageClick(object sender, RoutedEventArgs e)
    {
        if (_isEditingMessage) return;

        _isEditingMessage = true;
        var currentMessage = PersonalMessageText.Text;
        PersonalMessageEditBox.Text = currentMessage == "What's on your mind?" ? "" : currentMessage;
        PersonalMessageButton.Visibility = Visibility.Collapsed;
        PersonalMessageEditBox.Visibility = Visibility.Visible;
        PersonalMessageEditBox.Focus(FocusState.Programmatic);
        PersonalMessageEditBox.SelectAll();
    }

    private void OnPersonalMessageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            SavePersonalMessage();
            e.Handled = true;
        }
        else if (e.Key == VirtualKey.Escape)
        {
            CancelPersonalMessageEdit();
            e.Handled = true;
        }
    }

    private void OnPersonalMessageLostFocus(object sender, RoutedEventArgs e)
    {
        if (_isEditingMessage)
        {
            SavePersonalMessage();
        }
    }

    private void SavePersonalMessage()
    {
        if (!_isEditingMessage) return;

        var newMessage = PersonalMessageEditBox.Text?.Trim();
        if (_dataService is not null)
        {
            _dataService.CurrentUser.PersonalMessage = newMessage ?? "";
            PersonalMessageText.Text = string.IsNullOrEmpty(newMessage) ? "What's on your mind?" : newMessage;
        }

        CancelPersonalMessageEdit();
    }

    private void CancelPersonalMessageEdit()
    {
        _isEditingMessage = false;
        PersonalMessageEditBox.Visibility = Visibility.Collapsed;
        PersonalMessageButton.Visibility = Visibility.Visible;
    }

    private void OnStatusSelected(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string statusString && _dataService is not null
            && Enum.TryParse<PresenceStatus>(statusString, out var newStatus))
        {
            _dataService.CurrentUser.Status = newStatus;
            StatusText.Text = newStatus.ToString();
            ProfileStatusDot.Fill = StatusBrushes.ForStatus(newStatus);
        }
    }

    private async void OnAddGroupClick(object sender, RoutedEventArgs e)
    {
        if (_dataService is null) return;

        var dialog = new ContentDialog
        {
            Title = "Add New Group",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };

        var inputPanel = new StackPanel { Spacing = 12 };
        var nameBox = new TextBox
        {
            PlaceholderText = "Group name (e.g., Work, Gaming)",
            Header = "Group Name",
        };
        var emojiBox = new TextBox
        {
            PlaceholderText = "Emoji (e.g., 💼, 🎮)",
            Header = "Emoji",
            MaxLength = 4,
            Text = "👥",
        };

        inputPanel.Children.Add(nameBox);
        inputPanel.Children.Add(emojiBox);
        dialog.Content = inputPanel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
            var newGroup = new ContactGroup
            {
                Name = nameBox.Text.Trim(),
                Emoji = string.IsNullOrWhiteSpace(emojiBox.Text) ? "👥" : emojiBox.Text.Trim(),
                IsExpanded = true,
                Contacts = new System.Collections.ObjectModel.ObservableCollection<Contact>(),
            };

            _dataService.Groups.Add(newGroup);

            GroupsList.ItemsSource = null;
            GroupsList.ItemsSource = _dataService.Groups;
        }
    }
}

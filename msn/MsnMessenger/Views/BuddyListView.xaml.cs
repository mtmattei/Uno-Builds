using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using MsnMessenger.Helpers;
using MsnMessenger.Models;
using MsnMessenger.Services;
using Windows.System;

namespace MsnMessenger.Views;

public sealed partial class BuddyListView : UserControl
{
    private IMsnDataService? _dataService;
    private bool _isEditingName = false;
    private bool _isEditingMessage = false;
    private NowPlaying? _currentTrack;
    private DispatcherTimer? _progressTimer;

    public event Action<Contact>? OnContactSelected;

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

    public BuddyListView()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Animate header entrance
        await AnimateHeaderEntrance();

        // Initialize Spotify Now Playing
        InitializeNowPlaying();

        // Start status dot pulse animation
        StartStatusDotPulse();

        // Start Now Playing card breathing animation
        StartNowPlayingBreathing();
    }

    private async Task AnimateHeaderEntrance()
    {
        // Animate profile section entrance
        await Task.Delay(100);
        MicroAnimations.AnimateEntrance(UserAvatar, 0);
        await Task.Delay(50);
    }

    private void StartStatusDotPulse()
    {
        // Subtle pulse on the online status dot
        if (_dataService?.CurrentUser.Status == PresenceStatus.Online)
        {
            MicroAnimations.AnimatePulse(ProfileStatusDot, 1.15);
        }
    }

    private void StartNowPlayingBreathing()
    {
        // Subtle breathing effect on the now playing card border
        if (NowPlayingCard != null)
        {
            AnimateNowPlayingGlow();
        }
    }

    private void AnimateNowPlayingGlow()
    {
        var opacityAnim = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(2000)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(opacityAnim);
        Storyboard.SetTarget(opacityAnim, NowPlayingCard);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");

        storyboard.Completed += (s, e) =>
        {
            var reverseAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.8,
                Duration = new Duration(TimeSpan.FromMilliseconds(2000)),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var reverseStoryboard = new Storyboard();
            reverseStoryboard.Children.Add(reverseAnim);
            Storyboard.SetTarget(reverseAnim, NowPlayingCard);
            Storyboard.SetTargetProperty(reverseAnim, "Opacity");

            reverseStoryboard.Completed += (s2, e2) => AnimateNowPlayingGlow();
            reverseStoryboard.Begin();
        };

        storyboard.Begin();
    }

    private void InitializeNowPlaying()
    {
        // Load mock Spotify data
        _currentTrack = MockSpotifyData.GetCurrentTrack();
        UpdateNowPlayingUI();

        // Start progress timer to simulate playback
        _progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _progressTimer.Tick += OnProgressTimerTick;
        _progressTimer.Start();
    }

    private void OnProgressTimerTick(object? sender, object e)
    {
        if (_currentTrack == null || !_currentTrack.IsPlaying) return;

        // Increment progress
        _currentTrack.Progress = _currentTrack.Progress.Add(TimeSpan.FromSeconds(1));

        // If track finished, get a new random track
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
        if (_currentTrack == null) return;

        TrackNameText.Text = _currentTrack.TrackName;
        ArtistNameText.Text = _currentTrack.ArtistName;

        // Load album art
        if (!string.IsNullOrEmpty(_currentTrack.AlbumArtUrl))
        {
            AlbumArtImage.Source = new BitmapImage(new Uri(_currentTrack.AlbumArtUrl));
        }

        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (_currentTrack == null || ProgressBar == null) return;

        // Calculate progress width (max width approximately 120px based on card width)
        var maxWidth = 120.0;
        var progressWidth = (_currentTrack.ProgressPercent / 100.0) * maxWidth;
        ProgressBar.Width = Math.Max(4, progressWidth); // Minimum 4px width
    }

    private void BindData()
    {
        if (_dataService == null) return;

        var user = _dataService.CurrentUser;
        DisplayNameText.Text = user.DisplayName;
        StatusText.Text = user.Status.ToString();
        ProfileStatusDot.Fill = GetStatusBrush(user.Status);

        PersonalMessageText.Text = string.IsNullOrEmpty(user.PersonalMessage)
            ? "What's on your mind?"
            : user.PersonalMessage;

        var totalOnline = _dataService.Groups.Sum(g => g.OnlineCount);
        var totalContacts = _dataService.Groups.Sum(g => g.TotalCount);
        OnlineCountText.Text = $"{totalOnline}/{totalContacts} online";

        GroupsList.ItemsSource = _dataService.Groups;
    }

    private static SolidColorBrush GetStatusBrush(PresenceStatus status)
    {
        // Neo-Y2K Design Spec Colors
        return status switch
        {
            PresenceStatus.Online => new SolidColorBrush(ColorHelper.FromArgb(255, 0, 200, 150)),   // #00c896 Teal
            PresenceStatus.Away => new SolidColorBrush(ColorHelper.FromArgb(255, 247, 183, 49)),    // #f7b731 Gold
            PresenceStatus.Busy => new SolidColorBrush(ColorHelper.FromArgb(255, 235, 59, 90)),     // #eb3b5a Red
            PresenceStatus.Offline => new SolidColorBrush(ColorHelper.FromArgb(255, 74, 74, 74)),   // #4a4a4a Gray
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    private async void OnGroupHeaderClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ContactGroup group)
        {
            // Micro-interaction: scale down then up
            await AnimateButtonPress(btn);
            group.IsExpanded = !group.IsExpanded;
        }
    }

    private async void OnContactClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Contact contact)
        {
            // Micro-interaction: scale press effect
            await AnimateButtonPress(btn);
            OnContactSelected?.Invoke(contact);
        }
    }

    private async Task AnimateButtonPress(UIElement element)
    {
        var transform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        element.RenderTransform = transform;
        element.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

        // Scale down
        var scaleDown = new DoubleAnimation
        {
            To = 0.95,
            Duration = new Duration(TimeSpan.FromMilliseconds(80)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard1 = new Storyboard();
        Storyboard.SetTarget(scaleDown, transform);
        Storyboard.SetTargetProperty(scaleDown, "ScaleX");
        storyboard1.Children.Add(scaleDown);

        var scaleDownY = new DoubleAnimation
        {
            To = 0.95,
            Duration = new Duration(TimeSpan.FromMilliseconds(80)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleDownY, transform);
        Storyboard.SetTargetProperty(scaleDownY, "ScaleY");
        storyboard1.Children.Add(scaleDownY);

        storyboard1.Begin();
        await Task.Delay(80);

        // Scale back up
        var scaleUp = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(120)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard2 = new Storyboard();
        Storyboard.SetTarget(scaleUp, transform);
        Storyboard.SetTargetProperty(scaleUp, "ScaleX");
        storyboard2.Children.Add(scaleUp);

        var scaleUpY = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(120)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleUpY, transform);
        Storyboard.SetTargetProperty(scaleUpY, "ScaleY");
        storyboard2.Children.Add(scaleUpY);

        storyboard2.Begin();
    }

    // Display Name Editing
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
        if (!string.IsNullOrEmpty(newName) && _dataService != null)
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

    // Personal Message Editing
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
        if (_dataService != null)
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

    // Status Selection
    private void OnStatusSelected(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string statusString && _dataService != null)
        {
            if (Enum.TryParse<PresenceStatus>(statusString, out var newStatus))
            {
                _dataService.CurrentUser.Status = newStatus;
                StatusText.Text = newStatus.ToString();
                ProfileStatusDot.Fill = GetStatusBrush(newStatus);
            }
        }
    }

    // Add Group functionality
    private async void OnAddGroupClick(object sender, RoutedEventArgs e)
    {
        if (_dataService == null) return;

        // Create a simple input dialog using a ContentDialog
        var dialog = new ContentDialog
        {
            Title = "Add New Group",
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var inputPanel = new StackPanel { Spacing = 12 };
        var nameBox = new TextBox
        {
            PlaceholderText = "Group name (e.g., Work, Gaming)",
            Header = "Group Name"
        };
        var emojiBox = new TextBox
        {
            PlaceholderText = "Emoji (e.g., 💼, 🎮)",
            Header = "Emoji",
            MaxLength = 4,
            Text = "👥"
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
                Contacts = new System.Collections.ObjectModel.ObservableCollection<Contact>()
            };

            _dataService.Groups.Add(newGroup);

            // Refresh the list
            GroupsList.ItemsSource = null;
            GroupsList.ItemsSource = _dataService.Groups;
        }
    }
}

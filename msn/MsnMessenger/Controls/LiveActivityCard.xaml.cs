using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Helpers;
using MsnMessenger.Models;

namespace MsnMessenger.Controls;

public sealed partial class LiveActivityCard : UserControl
{
    private LiveActivity? _activity;
    private bool _isExpanded;

    public event Action? OnActionRequested;

    public LiveActivityCard()
    {
        this.InitializeComponent();
    }

    public void SetActivity(LiveActivity? activity)
    {
        _activity = activity;

        if (activity == null)
        {
            this.Visibility = Visibility.Collapsed;
            return;
        }

        this.Visibility = Visibility.Visible;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_activity == null) return;

        // Set icon and colors based on activity type
        var (icon, gradient) = GetActivityVisuals(_activity.Type);
        ActivityIcon.Text = icon;
        IconBadge.Background = gradient;
        ActionButton.Background = gradient;

        // Set service label
        ServiceLabel.Text = _activity.ServiceLabel;

        // Set title
        TitleText.Text = _activity.Title;
        ExpandedTitle.Text = _activity.Title;

        // Set action button
        ActionButton.Content = _activity.ActionLabel ?? GetDefaultActionLabel(_activity.Type);

        // Configure based on activity type
        switch (_activity.Type)
        {
            case ActivityType.Spotify:
            case ActivityType.AppleMusic:
                ConfigureMusicActivity();
                break;
            case ActivityType.Gaming:
                ConfigureGamingActivity();
                break;
            case ActivityType.Video:
                ConfigureVideoActivity();
                break;
        }
    }

    private void ConfigureMusicActivity()
    {
        if (_activity == null) return;

        // Show music-specific UI
        ProgressContainer.Visibility = Visibility.Visible;
        GamingStatusPanel.Visibility = Visibility.Collapsed;

        // Set artist and album
        ArtistText.Text = _activity.Artist ?? "";
        AlbumText.Text = _activity.Album ?? "";
        AlbumText.Visibility = string.IsNullOrEmpty(_activity.Album) ? Visibility.Collapsed : Visibility.Visible;

        // Set progress
        if (_activity.Progress > 0 && !string.IsNullOrEmpty(_activity.Duration))
        {
            UpdateProgressBar(_activity.Progress);
            TotalTime.Text = _activity.Duration;

            // Calculate current time from progress
            if (TryParseDuration(_activity.Duration, out var totalSeconds))
            {
                var currentSeconds = (int)(totalSeconds * _activity.Progress / 100.0);
                CurrentTime.Text = FormatTime(currentSeconds);
            }
        }
        else
        {
            ProgressContainer.Visibility = Visibility.Collapsed;
        }

        // Set album art placeholder icon
        UpdatePlaceholderIcon("🎵");
    }

    private void ConfigureGamingActivity()
    {
        if (_activity == null) return;

        // Show gaming-specific UI
        ProgressContainer.Visibility = Visibility.Collapsed;
        GamingStatusPanel.Visibility = Visibility.Visible;

        // Set gaming info
        ArtistText.Text = _activity.Platform ?? "PC";
        AlbumText.Visibility = Visibility.Collapsed;

        GameStatusText.Text = _activity.Status ?? "Playing";
        PartySizeText.Text = _activity.PartySize ?? "";
        PlayTimeText.Text = _activity.DisplayDuration;

        // Update placeholder
        UpdatePlaceholderIcon("🎮");
    }

    private void ConfigureVideoActivity()
    {
        if (_activity == null) return;

        // Show video-specific UI
        ProgressContainer.Visibility = Visibility.Collapsed;
        GamingStatusPanel.Visibility = Visibility.Collapsed;

        // Set video info
        ArtistText.Text = _activity.Subtitle ?? _activity.Service ?? "";
        AlbumText.Visibility = Visibility.Collapsed;

        if (_activity.IsLive && _activity.ViewerCount.HasValue)
        {
            AlbumText.Text = $"{_activity.ViewerCount:N0} watching";
            AlbumText.Visibility = Visibility.Visible;
        }

        // Update placeholder
        UpdatePlaceholderIcon("📺");
    }

    private void UpdatePlaceholderIcon(string icon)
    {
        var placeholder = ArtPlaceholder.Child as TextBlock;
        if (placeholder != null)
        {
            placeholder.Text = icon;
        }
    }

    private void UpdateProgressBar(int progress)
    {
        // Get the parent width and calculate fill width
        var containerWidth = ProgressContainer.ActualWidth;
        if (containerWidth > 0)
        {
            ProgressFill.Width = containerWidth * progress / 100.0;
        }
    }

    private (string icon, Brush gradient) GetActivityVisuals(ActivityType type)
    {
        return type switch
        {
            ActivityType.Spotify => ("🎵", (Brush)Application.Current.Resources["SpotifyGradientBrush"]),
            ActivityType.AppleMusic => ("🎵", (Brush)Application.Current.Resources["NotificationGradientBrush"]),
            ActivityType.Gaming => ("🎮", (Brush)Application.Current.Resources["GamingGradientBrush"]),
            ActivityType.Video => ("📺", (Brush)Application.Current.Resources["PurpleAccentBrush"]),
            _ => ("⚡", (Brush)Application.Current.Resources["CtaGradientBrush"])
        };
    }

    private string GetDefaultActionLabel(ActivityType type)
    {
        return type switch
        {
            ActivityType.Spotify or ActivityType.AppleMusic => "Listen Along",
            ActivityType.Gaming => "Ask to Join",
            ActivityType.Video => "Watch Together",
            _ => "View Activity"
        };
    }

    private void OnCardTapped(object sender, PointerRoutedEventArgs e)
    {
        ToggleExpanded();
    }

    private void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
        {
            Expand();
        }
        else
        {
            Collapse();
        }
    }

    private void Expand()
    {
        // Show expanded content
        ExpandedContent.Visibility = Visibility.Visible;

        // Animate chevron rotation
        AnimateChevron(180);

        // Animate content fade in
        AnimateContentIn();

        // Update progress bar width after layout
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_activity != null)
            {
                UpdateProgressBar(_activity.Progress);
            }
        });
    }

    private void Collapse()
    {
        // Animate chevron rotation
        AnimateChevron(0);

        // Animate content fade out then hide
        AnimateContentOut();
    }

    private void AnimateChevron(double targetAngle)
    {
        var animation = new DoubleAnimation
        {
            To = targetAngle,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, ChevronRotation);
        Storyboard.SetTargetProperty(animation, "Angle");
        storyboard.Begin();
    }

    private void AnimateContentIn()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnimation);
        Storyboard.SetTarget(fadeAnimation, ExpandedContent);
        Storyboard.SetTargetProperty(fadeAnimation, "Opacity");
        storyboard.Begin();
    }

    private void AnimateContentOut()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnimation);
        Storyboard.SetTarget(fadeAnimation, ExpandedContent);
        Storyboard.SetTargetProperty(fadeAnimation, "Opacity");

        storyboard.Completed += (s, e) =>
        {
            ExpandedContent.Visibility = Visibility.Collapsed;
        };

        storyboard.Begin();
    }

    private async void OnActionClick(object sender, RoutedEventArgs e)
    {
        // Animate button press
        await MicroAnimations.AnimatePress(ActionButton);

        // Fire event
        OnActionRequested?.Invoke();

        // If there's an action URL, try to launch it
        if (!string.IsNullOrEmpty(_activity?.ActionUrl))
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(_activity.ActionUrl));
            }
            catch
            {
                // Handle launch failure silently
            }
        }
    }

    private bool TryParseDuration(string duration, out int totalSeconds)
    {
        totalSeconds = 0;
        var parts = duration.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var minutes) &&
            int.TryParse(parts[1], out var seconds))
        {
            totalSeconds = minutes * 60 + seconds;
            return true;
        }
        return false;
    }

    private string FormatTime(int totalSeconds)
    {
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }
}

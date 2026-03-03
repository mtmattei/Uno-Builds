using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using FieldOpsPro.Models;
using FieldOpsPro.Models.Enums;
using TaskStatus = FieldOpsPro.Models.Enums.TaskStatus;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class TaskCard : UserControl
{
    private DispatcherTimer? _slaFlashTimer;
    private bool _slaFlashState;

    // Cached brushes to avoid allocation on every timer tick / hover
    private static readonly SolidColorBrush SlaDarkGreyBg = new(Windows.UI.Color.FromArgb(255, 64, 64, 64));
    private static readonly SolidColorBrush SlaMedDarkBg = new(Windows.UI.Color.FromArgb(255, 48, 48, 48));
    private static readonly SolidColorBrush SlaDarkBg = new(Windows.UI.Color.FromArgb(255, 32, 32, 32));
    private static readonly SolidColorBrush SlaDarkestBg = new(Windows.UI.Color.FromArgb(255, 26, 26, 26));
    private static readonly SolidColorBrush SlaWhiteFg = new(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    private static readonly SolidColorBrush SlaBlackFg = new(Windows.UI.Color.FromArgb(255, 0, 0, 0));
    private static readonly SolidColorBrush SlaLightGreyFg = new(Windows.UI.Color.FromArgb(255, 192, 192, 192));
    private static readonly SolidColorBrush SlaMutedFg = new(Windows.UI.Color.FromArgb(255, 128, 128, 128));
    private static readonly SolidColorBrush HoverBorderBrush = new(ParseColor("#404040"));
    private static readonly SolidColorBrush HoverIconBrush = new(ParseColor("#FFFFFF"));
    private LinearGradientBrush? _hoverGradient;

    public TaskCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopSlaTimer();
    }

    public static readonly DependencyProperty TaskItemProperty =
        DependencyProperty.Register(nameof(TaskItem), typeof(TaskItem), typeof(TaskCard),
            new PropertyMetadata(null, OnTaskItemChanged));

    public TaskItem? TaskItem
    {
        get => (TaskItem?)GetValue(TaskItemProperty);
        set => SetValue(TaskItemProperty, value);
    }

    private static void OnTaskItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskCard card)
        {
            card.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (TaskItem == null || TitleText == null) return;

        TitleText.Text = TaskItem.Title;
        LocationText.Text = TaskItem.Location?.Name ?? TaskItem.Location?.Address ?? "Unknown";
        DurationText.Text = TaskItem.EstimatedDuration ?? "";

        // Priority indicator color
        if (PriorityIndicator != null)
            PriorityIndicator.Background = GetPriorityBrush(TaskItem.Priority);

        // Assignee avatar
        if (AssigneeAvatar != null && !string.IsNullOrEmpty(TaskItem.AssigneeInitials))
        {
            AssigneeAvatar.Visibility = Visibility.Visible;
            AssigneeAvatar.Initials = TaskItem.AssigneeInitials;
            AssigneeAvatar.AvatarColor = TaskItem.AssigneeAvatarColor ?? AvatarColor.Blue;
        }
        else if (AssigneeAvatar != null)
        {
            AssigneeAvatar.Visibility = Visibility.Collapsed;
        }

        // Status badge
        TaskStatusBadge.Text = GetStatusText(TaskItem.Status);
        TaskStatusBadge.BadgeType = GetBadgeType(TaskItem.Status);

        // SLA Timer
        UpdateSlaTimer();

        // Photo thumbnails
        UpdatePhotoThumbnails();
    }

    private void UpdateSlaTimer()
    {
        if (SlaTimerBorder == null || TaskItem?.SlaDeadline == null)
        {
            if (SlaTimerBorder != null)
                SlaTimerBorder.Visibility = Visibility.Collapsed;
            StopSlaTimer();
            return;
        }

        var deadline = TaskItem.SlaDeadline.Value;
        var remaining = deadline - DateTime.Now;

        if (remaining.TotalMinutes <= 0)
        {
            // SLA breached
            SlaTimerBorder.Visibility = Visibility.Visible;
            SlaTimerBorder.Background = SlaDarkGreyBg;
            SlaTimerText.Text = "OVERDUE";
            SlaTimerText.Foreground = SlaWhiteFg;
            SlaIcon.Foreground = SlaWhiteFg;
            StartSlaFlashTimer(true);
        }
        else if (remaining.TotalMinutes <= 30)
        {
            // Critical - under 30 minutes
            SlaTimerBorder.Visibility = Visibility.Visible;
            SlaTimerBorder.Background = SlaMedDarkBg;
            SlaTimerText.Text = FormatTimeRemaining(remaining);
            SlaTimerText.Foreground = SlaWhiteFg;
            SlaIcon.Foreground = SlaWhiteFg;
            StartSlaFlashTimer(true);
        }
        else if (remaining.TotalHours <= 2)
        {
            // Warning - under 2 hours
            SlaTimerBorder.Visibility = Visibility.Visible;
            SlaTimerBorder.Background = SlaDarkBg;
            SlaTimerText.Text = FormatTimeRemaining(remaining);
            SlaTimerText.Foreground = SlaLightGreyFg;
            SlaIcon.Foreground = SlaLightGreyFg;
            StopSlaTimer();
        }
        else
        {
            // Normal - show timer but no urgency
            SlaTimerBorder.Visibility = Visibility.Visible;
            SlaTimerBorder.Background = SlaDarkestBg;
            SlaTimerText.Text = FormatTimeRemaining(remaining);
            SlaTimerText.Foreground = SlaMutedFg;
            SlaIcon.Foreground = SlaMutedFg;
            StopSlaTimer();
        }
    }

    private string FormatTimeRemaining(TimeSpan remaining)
    {
        if (remaining.TotalHours >= 24)
        {
            return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
        }
        else if (remaining.TotalHours >= 1)
        {
            return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
        }
        else
        {
            return $"{remaining.Minutes}m";
        }
    }

    private void StartSlaFlashTimer(bool urgent)
    {
        if (!urgent)
        {
            StopSlaTimer();
            return;
        }

        if (_slaFlashTimer == null)
        {
            _slaFlashTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _slaFlashTimer.Tick += OnSlaFlashTick;
        }
        _slaFlashTimer.Start();
    }

    private void StopSlaTimer()
    {
        _slaFlashTimer?.Stop();
    }

    private void OnSlaFlashTick(object? sender, object e)
    {
        if (SlaTimerBorder == null) return;

        _slaFlashState = !_slaFlashState;

        // Flash between white and grey backgrounds
        if (_slaFlashState)
        {
            SlaTimerBorder.Background = SlaWhiteFg;
            SlaTimerText.Foreground = SlaBlackFg;
            SlaIcon.Foreground = SlaBlackFg;
        }
        else
        {
            SlaTimerBorder.Background = SlaDarkGreyBg;
            SlaTimerText.Foreground = SlaWhiteFg;
            SlaIcon.Foreground = SlaWhiteFg;
        }
    }

    private void UpdatePhotoThumbnails()
    {
        if (PhotosPanel == null) return;

        var photos = TaskItem?.PhotoUrls;
        if (photos == null || photos.Length == 0)
        {
            PhotosPanel.Visibility = Visibility.Collapsed;
            return;
        }

        PhotosPanel.Visibility = Visibility.Visible;

        // Show up to 3 photos
        Photo1.Visibility = photos.Length >= 1 ? Visibility.Visible : Visibility.Collapsed;
        Photo2.Visibility = photos.Length >= 2 ? Visibility.Visible : Visibility.Collapsed;
        Photo3.Visibility = photos.Length >= 3 ? Visibility.Visible : Visibility.Collapsed;

        try
        {
            if (photos.Length >= 1 && PhotoImage1 != null && Uri.TryCreate(photos[0], UriKind.Absolute, out var uri1))
                PhotoImage1.Source = new BitmapImage(uri1);
            if (photos.Length >= 2 && PhotoImage2 != null && Uri.TryCreate(photos[1], UriKind.Absolute, out var uri2))
                PhotoImage2.Source = new BitmapImage(uri2);
            if (photos.Length >= 3 && PhotoImage3 != null && Uri.TryCreate(photos[2], UriKind.Absolute, out var uri3))
                PhotoImage3.Source = new BitmapImage(uri3);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading photo thumbnails: {ex.Message}");
        }

        // Show "+N" indicator if more than 3 photos
        if (photos.Length > 3)
        {
            MorePhotosIndicator.Visibility = Visibility.Visible;
            MorePhotosText.Text = $"+{photos.Length - 3}";
        }
        else
        {
            MorePhotosIndicator.Visibility = Visibility.Collapsed;
        }
    }

    private static SolidColorBrush GetPriorityBrush(TaskPriority priority)
    {
        // Monochromatic priority colors
        var color = priority switch
        {
            TaskPriority.High => "#FFFFFF",    // White for high
            TaskPriority.Medium => "#808080",  // Grey for medium
            TaskPriority.Low => "#404040",     // Dark grey for low
            _ => "#252525"
        };

        return new SolidColorBrush(ParseColor(color));
    }

    private static string GetStatusText(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Pending => "Pending",
            TaskStatus.InProgress => "In Progress",
            TaskStatus.Completed => "Completed",
            TaskStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    private static BadgeType GetBadgeType(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Pending => BadgeType.Pending,
            TaskStatus.InProgress => BadgeType.InProgress,
            TaskStatus.Completed => BadgeType.Completed,
            TaskStatus.Cancelled => BadgeType.Danger,
            _ => BadgeType.Default
        };
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Animate lift effect
        var liftAnimation = new DoubleAnimation
        {
            To = -4,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(liftAnimation, CardTransform);
        Storyboard.SetTargetProperty(liftAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(liftAnimation);
        storyboard.Begin();

        // Glow border effect
        CardBorder.BorderBrush = HoverBorderBrush;

        // Additional hover: scale up and brighten background/icons (match QuickAction feel)
        AnimateScale(1.03);

        // Brighter gradient background on hover
        CardBorder.Background = _hoverGradient ??= new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = ParseColor("#1F1F1F"), Offset = 0 },
                new GradientStop { Color = ParseColor("#151515"), Offset = 1 }
            }
        };

        // Brighten the small icons if available
        if (LocationIcon != null) LocationIcon.Foreground = HoverIconBrush;
        if (DurationIcon != null) DurationIcon.Foreground = HoverIconBrush;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Animate back down
        var dropAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(dropAnimation, CardTransform);
        Storyboard.SetTargetProperty(dropAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(dropAnimation);
        storyboard.Begin();

        // Remove glow
        CardBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        // Scale back to normal
        AnimateScale(1.0);

        // Restore background to original resource if available
        if (Application.Current != null && Application.Current.Resources.ContainsKey("BgTertiaryBrush"))
        {
            var brush = Application.Current.Resources["BgTertiaryBrush"] as Brush;
            if (brush != null)
                CardBorder.Background = brush;
        }

        // Restore icons
        if (LocationIcon != null && Application.Current.Resources.TryGetValue("TextMutedBrush", out var locBrush) && locBrush is Brush locBrushTyped)
            LocationIcon.Foreground = locBrushTyped;
        if (DurationIcon != null && Application.Current.Resources.TryGetValue("TextMutedBrush", out var durBrush) && durBrush is Brush durBrushTyped)
            DurationIcon.Foreground = durBrushTyped;
    }

    private void AnimateScale(double targetScale)
    {
        var scaleXAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleXAnimation, CardTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, "ScaleX");

        var scaleYAnimation = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnimation, CardTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, "ScaleY");

        var sb = new Storyboard();
        sb.Children.Add(scaleXAnimation);
        sb.Children.Add(scaleYAnimation);
        sb.Begin();
    }
}

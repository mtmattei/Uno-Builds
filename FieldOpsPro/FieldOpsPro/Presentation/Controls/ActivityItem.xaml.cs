using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using FieldOpsPro.Models;
using FieldOpsPro.Models.Enums;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class ActivityItem : UserControl
{
    public ActivityItem()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty ActivityProperty =
        DependencyProperty.Register(nameof(Activity), typeof(Activity), typeof(ActivityItem),
            new PropertyMetadata(null, OnActivityChanged));

    public static readonly DependencyProperty IsLastItemProperty =
        DependencyProperty.Register(nameof(IsLastItem), typeof(bool), typeof(ActivityItem),
            new PropertyMetadata(false, OnActivityChanged));

    public Activity? Activity
    {
        get => (Activity?)GetValue(ActivityProperty);
        set => SetValue(ActivityProperty, value);
    }

    public bool IsLastItem
    {
        get => (bool)GetValue(IsLastItemProperty);
        set => SetValue(IsLastItemProperty, value);
    }

    private static void OnActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ActivityItem item)
        {
            item.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (Activity == null || ActorRun == null) return;

        ActorRun.Text = Activity.ActorName;
        ActionRun.Text = " " + Activity.Message;

        // Format timestamp
        TimestampText.Text = FormatTimestamp(Activity.Timestamp);

        // Set icon and color based on activity type
        var (glyph, color) = GetActivityIconAndColor(Activity.Type);
        ActivityIcon.Glyph = glyph;
        ActivityIcon.Foreground = new SolidColorBrush(ParseColor(color));

        // Hide timeline for last item
        TimelineLine.Visibility = IsLastItem ? Visibility.Collapsed : Visibility.Visible;
    }

    private static string FormatTimestamp(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;

        if (diff.TotalMinutes < 1)
            return "Just now";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        return timestamp.ToString("MMM d, h:mm tt");
    }

    private static (string glyph, string color) GetActivityIconAndColor(ActivityType type)
    {
        return type switch
        {
            ActivityType.TaskCompleted => ("\uE73E", "#22C55E"), // Checkmark
            ActivityType.Arrival => ("\uE707", "#4ECDC4"),       // MapPin
            ActivityType.Assignment => ("\uE77B", "#FF6B35"),    // Contact
            ActivityType.Report => ("\uE7C3", "#F59E0B"),        // Document
            ActivityType.StatusChange => ("\uE72C", "#9CA3AF"), // Sync
            _ => ("\uE946", "#6B7280")                           // Info
        };
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }
}

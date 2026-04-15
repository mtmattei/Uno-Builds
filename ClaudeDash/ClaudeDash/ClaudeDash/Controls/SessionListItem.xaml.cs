namespace ClaudeDash.Controls;

public sealed partial class SessionListItem : UserControl
{
    public static readonly DependencyProperty SessionProperty =
        DependencyProperty.Register(nameof(Session), typeof(SessionItem), typeof(SessionListItem),
            new PropertyMetadata(null, OnSessionChanged));

    public SessionItem? Session
    {
        get => (SessionItem?)GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    public SessionListItem()
    {
        this.InitializeComponent();
    }

    private static void OnSessionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SessionListItem item || e.NewValue is not SessionItem session) return;

        item.DescriptionText.Text = session.Description;
        item.RepoBadgeText.Text = session.RepoName;

        // Near-black status colors
        var statusColor = session.Status switch
        {
            "active" => ColorHelper.FromArgb(255, 74, 222, 128),    // green
            "completed" => ColorHelper.FromArgb(255, 210, 210, 212), // highlight
            "error" => ColorHelper.FromArgb(255, 239, 68, 68),      // red
            _ => ColorHelper.FromArgb(255, 67, 67, 72)              // dim
        };
        item.StatusDot.Fill = new SolidColorBrush(statusColor);

        var diff = DateTimeOffset.Now - session.Timestamp;
        item.TimeText.Text = diff.TotalMinutes < 1 ? "just now"
            : diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes}m"
            : diff.TotalHours < 24 ? $"{(int)diff.TotalHours}h"
            : $"{(int)diff.TotalDays}d";
    }
}

using ClaudeDash.Services;
using Microsoft.UI.Xaml.Input;

namespace ClaudeDash.Controls;

public sealed partial class SessionCard : UserControl
{
    private static readonly Color ActiveGreen = ColorHelper.FromArgb(255, 74, 222, 128);
    private static readonly Color RecentYellow = ColorHelper.FromArgb(255, 251, 191, 36);
    private static readonly Color InactiveGray = ColorHelper.FromArgb(255, 100, 100, 106);

    private string? _sessionId;
    private Color _accentColor = ActiveGreen;

    public SessionCard()
    {
        this.InitializeComponent();
    }

    public void Bind(ClaudeSessionInfo session, Color accentColor)
    {
        _sessionId = session.SessionId;
        _accentColor = accentColor;

        // Accent bar
        AccentBar.Background = new SolidColorBrush(accentColor);

        // Title: first user message truncated, or short ID fallback
        var title = !string.IsNullOrWhiteSpace(session.FirstUserMessage)
            ? Truncate(session.FirstUserMessage, 60)
            : session.ShortId;
        TitleText.Text = title.ToLowerInvariant();

        // Status dot based on recency
        var diff = DateTime.UtcNow - session.LastActivity;
        Color dotColor;
        if (diff.TotalMinutes < 5)
            dotColor = ActiveGreen;
        else if (diff.TotalHours < 1)
            dotColor = RecentYellow;
        else
            dotColor = InactiveGray;
        StatusDot.Fill = new SolidColorBrush(dotColor);

        // Time ago
        TimeAgoText.Text = FormatTimeAgo(session.LastActivity);

        // Model badge
        var model = !string.IsNullOrWhiteSpace(session.Model)
            ? NormalizeModel(session.Model)
            : "unknown";
        ModelText.Text = model;

        // Repo badge
        if (!string.IsNullOrWhiteSpace(session.RepoName))
        {
            RepoText.Text = session.RepoName.ToLowerInvariant();
            RepoBadge.Visibility = Visibility.Visible;
        }
        else
        {
            RepoBadge.Visibility = Visibility.Collapsed;
        }

        // Description
        DescriptionText.Text = !string.IsNullOrWhiteSpace(session.FirstUserMessage)
            ? session.FirstUserMessage.ToLowerInvariant()
            : "no description";

        // Metrics
        var parts = new List<string>();
        if (session.TokenCount > 0)
            parts.Add($"{FormatTokens(session.TokenCount)} tokens");
        if (!string.IsNullOrWhiteSpace(session.Duration))
            parts.Add(session.Duration);
        if (session.MessageCount > 0)
            parts.Add($"{session.MessageCount} msgs");
        MetricsText.Text = parts.Count > 0
            ? string.Join(" | ", parts)
            : "no metrics";
    }

    private static string FormatTimeAgo(DateTime lastActivity)
    {
        var diff = DateTime.UtcNow - lastActivity;
        if (diff.TotalMinutes < 1) return "now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        return $"{(int)diff.TotalDays}d ago";
    }

    private static string NormalizeModel(string model)
    {
        // Shorten model names: "claude-sonnet-4-6-20250514" -> "sonnet-4"
        if (model.Contains("opus", StringComparison.OrdinalIgnoreCase)) return "opus-4";
        if (model.Contains("sonnet", StringComparison.OrdinalIgnoreCase)) return "sonnet-4";
        if (model.Contains("haiku", StringComparison.OrdinalIgnoreCase)) return "haiku-4";
        return model.Length > 12 ? model[..12] : model;
    }

    private static string FormatTokens(int tokens)
    {
        if (tokens >= 1_000_000) return $"{tokens / 1_000_000.0:F1}M";
        if (tokens >= 1_000) return $"{tokens / 1_000.0:F1}k";
        return tokens.ToString();
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // Replace newlines with spaces for single-line display
        text = text.Replace('\n', ' ').Replace('\r', ' ');
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        CardBorder.BorderBrush = new SolidColorBrush(_accentColor);
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        CardBorder.BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"];
    }

    private void Card_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_sessionId)) return;
        var nav = App.Current.Host!.Services.GetRequiredService<INavigationService>();
        nav.NavigateTo("session-replay", _sessionId);
    }
}

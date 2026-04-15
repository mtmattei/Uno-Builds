using ClaudeDash.Models;
using ClaudeDash.Services;
using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class HomePage : Page
{
    private readonly ISlideOverService _slideOver;

    public HomePage()
    {
        this.InitializeComponent();

        var host = App.Current.Host!.Services;
        _slideOver = host.GetRequiredService<ISlideOverService>();

        TermInput.KeyDown += (_, e) =>
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                TermSend_Click(null, null!);
                e.Handled = true;
            }
        };

        // Create BindableHomeModel on a background thread to avoid blocking UI
        _ = Task.Run(() =>
        {
            var model = ActivatorUtilities.CreateInstance<BindableHomeModel>(host);
            DispatcherQueue.TryEnqueue(() => DataContext = model);
        });
    }

    private void TermSend_Click(object? sender, RoutedEventArgs e)
    {
        var input = TermInput.Text?.Trim();
        if (string.IsNullOrEmpty(input)) return;

        if (DataContext is BindableHomeModel vm)
        {
            vm.SendTerminalMessage.Execute(input);
        }
        TermInput.Text = "";
    }

    private void ProjectCard_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not ProjectInfo project) return;
        _slideOver.Show("PROJECT DETAILS", BuildProjectDetailPanel(project));
    }

    private UIElement BuildProjectDetailPanel(ProjectInfo project)
    {
        var mono = (string)Application.Current.Resources["MonoFontFamily"];
        var monoFamily = new FontFamily(mono);

        var cardBg = (Brush)Application.Current.Resources["CardBackgroundBrush"];
        var surfaceBg = (Brush)Application.Current.Resources["SurfaceBackgroundBrush"];
        var borderSubtle = (Brush)Application.Current.Resources["BorderSubtleBrush"];
        var textPrimary = (Brush)Application.Current.Resources["TextPrimaryBrush"];
        var textMuted = (Brush)Application.Current.Resources["TextMutedBrush"];
        var textTertiary = (Brush)Application.Current.Resources["TextTertiaryBrush"];
        var accentGreen = (Brush)Application.Current.Resources["StatusGreenBrush"];

        // Health status
        var isHealthy = project.PathExists && project.IsGitRepo;
        var healthLabel = isHealthy ? "healthy" : "issues";
        var healthColor = isHealthy ? accentGreen : (Brush)Application.Current.Resources["StatusAmberBrush"];

        // Relative time for LastActivity
        var lastActiveText = FormatRelativeTime(project.LastActivity);

        // --- Build the panel ---
        var root = new StackPanel { Spacing = 16 };

        // Project name + health badge row
        var headerRow = new Grid();
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nameBlock = new TextBlock
        {
            Text = project.Name,
            FontFamily = monoFamily,
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = textPrimary,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameBlock, 0);
        headerRow.Children.Add(nameBlock);

        // Health badge
        var badge = new Border
        {
            Background = surfaceBg,
            BorderBrush = borderSubtle,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(10, 4, 10, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        var badgeContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        badgeContent.Children.Add(new Ellipse
        {
            Width = 6, Height = 6,
            Fill = healthColor,
            VerticalAlignment = VerticalAlignment.Center
        });
        badgeContent.Children.Add(new TextBlock
        {
            Text = healthLabel,
            FontFamily = monoFamily,
            FontSize = 11,
            Foreground = healthColor,
            VerticalAlignment = VerticalAlignment.Center
        });
        badge.Child = badgeContent;
        Grid.SetColumn(badge, 1);
        headerRow.Children.Add(badge);
        root.Children.Add(headerRow);

        // Subtitle (path)
        root.Children.Add(new TextBlock
        {
            Text = project.Path,
            FontFamily = monoFamily,
            FontSize = 11,
            Foreground = textTertiary,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        // 2x2 detail card grid
        var detailGrid = new Grid { ColumnSpacing = 8, RowSpacing = 8 };
        detailGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        detailGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        detailGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        detailGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        detailGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddDetailCard(detailGrid, 0, 0, "FRAMEWORK", project.IsGitRepo ? "Uno Platform" : "Unknown", monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);
        AddDetailCard(detailGrid, 0, 1, "LANGUAGE", project.HasClaudeMd ? "C# / XAML" : "C#", monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);
        AddDetailCard(detailGrid, 1, 0, "SESSIONS", project.SessionCount.ToString(), monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);
        AddDetailCard(detailGrid, 1, 1, "BRANCH", string.IsNullOrEmpty(project.CurrentBranch) ? "n/a" : project.CurrentBranch, monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);
        AddDetailCard(detailGrid, 2, 0, "LAST ACTIVE", lastActiveText, monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);
        AddDetailCard(detailGrid, 2, 1, "WORKTREES", project.WorktreeCount.ToString(), monoFamily, surfaceBg, borderSubtle, textMuted, textPrimary);

        root.Children.Add(detailGrid);

        // CLAUDE.md badge
        if (project.HasClaudeMd)
        {
            var claudeBadge = new Border
            {
                Background = surfaceBg,
                BorderBrush = borderSubtle,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8)
            };
            var claudeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            claudeRow.Children.Add(new FontIcon
            {
                Glyph = "\uE73E",
                FontSize = 14,
                Foreground = accentGreen,
                VerticalAlignment = VerticalAlignment.Center
            });
            claudeRow.Children.Add(new TextBlock
            {
                Text = "CLAUDE.md present",
                FontFamily = monoFamily,
                FontSize = 11,
                Foreground = accentGreen,
                VerticalAlignment = VerticalAlignment.Center
            });
            claudeBadge.Child = claudeRow;
            root.Children.Add(claudeBadge);
        }

        return root;
    }

    private static void AddDetailCard(Grid grid, int row, int col, string label, string value,
        FontFamily font, Brush bg, Brush border, Brush labelFg, Brush valueFg)
    {
        var card = new Border
        {
            Background = bg,
            BorderBrush = border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10, 12, 10)
        };

        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontFamily = font,
            FontSize = 9,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = labelFg,
            CharacterSpacing = 60
        });
        stack.Children.Add(new TextBlock
        {
            Text = value,
            FontFamily = font,
            FontSize = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = valueFg,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        card.Child = stack;
        Grid.SetRow(card, row);
        Grid.SetColumn(card, col);
        grid.Children.Add(card);
    }

    private static string FormatRelativeTime(DateTime dt)
    {
        if (dt == default) return "n/a";
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        return dt.ToString("MMM d");
    }
}

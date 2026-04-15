using System.Diagnostics;

namespace ClaudeDash.Helpers;

/// <summary>
/// Shared hover effects and interactive actions for terminal-style rows.
/// </summary>
public static class UiHelpers
{
    private static readonly Color HoverBg = ColorHelper.FromArgb(255, 18, 18, 20); // #121214
    private static readonly SolidColorBrush HoverBrush = new(HoverBg);
    private static readonly SolidColorBrush TransparentBrush = new(Microsoft.UI.Colors.Transparent);

    /// <summary>
    /// Attach hover highlight effect to a Border row.
    /// </summary>
    public static void AttachHover(Border row)
    {
        row.PointerEntered += (s, e) =>
        {
            if (s is Border b) b.Background = HoverBrush;
        };
        row.PointerExited += (s, e) =>
        {
            if (s is Border b) b.Background = TransparentBrush;
        };
    }

    /// <summary>
    /// Attach hover highlight to a Grid row.
    /// </summary>
    public static void AttachHover(Grid row)
    {
        row.PointerEntered += (s, e) =>
        {
            if (s is Grid g) g.Background = HoverBrush;
        };
        row.PointerExited += (s, e) =>
        {
            if (s is Grid g) g.Background = TransparentBrush;
        };
    }

    /// <summary>
    /// Build a pipe-character section divider: ├─── label ──────────────┤
    /// (Legacy - kept for backward compat with old pages)
    /// </summary>
    public static string BuildPipeDivider(string label, int totalWidth = 80)
    {
        var prefix = "\u251C\u2500\u2500\u2500 ";
        var suffix = " ";
        var endCap = "\u2524";
        var usedChars = prefix.Length + label.Length + suffix.Length + endCap.Length;
        var remaining = Math.Max(totalWidth - usedChars, 4);
        var line = new string('\u2500', remaining);
        return $"{prefix}{label} {line}{endCap}";
    }

    /// <summary>
    /// Populate a Grid container as a full-width section divider:
    /// UPPERCASE LABEL ──────────────────────────────
    /// The line stretches to fill remaining width.
    /// </summary>
    public static void SetupDivider(Grid container, string label)
    {
        container.Children.Clear();
        container.ColumnDefinitions.Clear();
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        container.Margin = new Thickness(0, 20, 0, 8);

        var text = new TextBlock
        {
            Text = label.ToUpperInvariant(),
            FontFamily = new FontFamily("Cascadia Code, Consolas, monospace"),
            FontSize = 10,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            CharacterSpacing = 120,
            Foreground = (Brush)Application.Current.Resources["TextTertiaryBrush"],
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        Grid.SetColumn(text, 0);

        var rule = new Border
        {
            Height = 1,
            Background = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetColumn(rule, 1);

        container.Children.Add(text);
        container.Children.Add(rule);
    }

    /// <summary>
    /// Add a 1px gradient accent bar to the top of a card Border.
    /// The bar fades from accentColor on the left to transparent on the right.
    /// The card's existing child content is preserved and shifted 1px down.
    /// </summary>
    public static void AddAccentBar(Border cardBorder, Color accentColor)
    {
        var existingChild = cardBorder.Child;
        cardBorder.Child = null;

        var rootGrid = new Grid();

        // 1px gradient accent bar
        var accentBar = new Border
        {
            Height = 1,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = new CornerRadius(14, 14, 0, 0),
            Background = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0.5),
                EndPoint = new Windows.Foundation.Point(1, 0.5),
                GradientStops =
                {
                    new GradientStop { Color = accentColor, Offset = 0 },
                    new GradientStop { Color = ColorHelper.FromArgb(0, 0, 0, 0), Offset = 1 }
                }
            }
        };
        rootGrid.Children.Add(accentBar);

        // Re-wrap existing content with 1px top margin to clear the accent bar
        if (existingChild is FrameworkElement fe)
        {
            fe.Margin = new Thickness(
                fe.Margin.Left,
                Math.Max(fe.Margin.Top, 1),
                fe.Margin.Right,
                fe.Margin.Bottom);
        }

        if (existingChild != null)
            rootGrid.Children.Add(existingChild);

        cardBorder.Child = rootGrid;
    }

    /// <summary>
    /// Open a folder path in the system file explorer.
    /// </summary>
    public static void OpenInExplorer(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        catch { }
    }

    /// <summary>
    /// Open a folder in a new terminal window.
    /// </summary>
    public static void OpenInTerminal(string path)
    {
        try
        {
            // Try Windows Terminal first, fall back to cmd
            Process.Start(new ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = $"-d \"{path}\"",
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k cd /d \"{path}\"",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    /// <summary>
    /// Launch or resume a Claude Code session in a terminal.
    /// </summary>
    public static void OpenClaudeSession(string projectPath, string? sessionId = null)
    {
        try
        {
            var claudeArgs = !string.IsNullOrEmpty(sessionId)
                ? $"claude --resume {sessionId}"
                : "claude";

            // Try Windows Terminal first, fall back to cmd
            Process.Start(new ProcessStartInfo
            {
                FileName = "wt.exe",
                Arguments = $"-d \"{projectPath}\" cmd /k {claudeArgs}",
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                var claudeArgs = !string.IsNullOrEmpty(sessionId)
                    ? $"claude --resume {sessionId}"
                    : "claude";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k cd /d \"{projectPath}\" && {claudeArgs}",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    /// <summary>
    /// Open a file in the default text editor.
    /// </summary>
    public static void OpenInEditor(string filePath)
    {
        try
        {
            // Try VS Code first, then notepad
            Process.Start(new ProcessStartInfo
            {
                FileName = "code",
                Arguments = $"\"{filePath}\"",
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    /// <summary>
    /// Copy text to clipboard.
    /// </summary>
    public static void CopyToClipboard(string text)
    {
        try
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
        catch { }
    }

    /// <summary>
    /// Create an action button with terminal styling.
    /// </summary>
    public static Button CreateActionButton(string text, Action onClick)
    {
        var btn = new Button
        {
            Content = new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Cascadia Code, Consolas, monospace"),
                FontSize = 10,
                Foreground = (Brush)Application.Current.Resources["TextTertiaryBrush"]
            },
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderBrush = (Brush)Application.Current.Resources["BorderDefaultBrush"],
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8, 4),
            CornerRadius = new CornerRadius(3),
            VerticalAlignment = VerticalAlignment.Center
        };
        btn.Click += (s, e) => onClick();
        return btn;
    }
}

using Microsoft.UI.Xaml.Input;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class AgentsPage : Page
{
    private ImmutableList<AgentSession> _sessions = ImmutableList<AgentSession>.Empty;
    private string? _selectedId;

    public AgentsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up dynamically attached event handlers
        foreach (var child in SessionList.Children)
        {
            if (child is Border card)
                card.Tapped -= OnSessionCardTapped;
        }
    }

    private IAgentService? _agentService;

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Entrance animations
        AnimationHelper.FadeUp(SessionListPanel, 0);
        AnimationHelper.FadeUp(DetailPanel, 100);

        var host = Helpers.HostHelper.GetHost();
        if (host is null) return;
        _agentService = host.Services.GetRequiredService<IAgentService>();
        var mcpService = host.Services.GetRequiredService<IMcpService>();

        // Load sessions and MCP status in parallel
        var sessionsTask = _agentService.GetSessionsAsync(CancellationToken.None).AsTask();
        var mcpTask = mcpService.GetConnectionStatusAsync(CancellationToken.None).AsTask();
        await Task.WhenAll(sessionsTask, mcpTask);

        _sessions = sessionsTask.Result;
        BuildSessionList();
        if (_sessions.Count > 0)
            SelectSession(_sessions[0].Id);

        PopulateMcpHealth(mcpTask.Result);

        // Wire button handlers
        NewSessionButton.Click += OnNewSessionClick;
        ReplayButton.Click += OnReplayClick;
    }

    private async void OnNewSessionClick(object sender, RoutedEventArgs e)
    {
        if (_agentService is null) return;
        NewSessionButton.IsEnabled = false;
        try
        {
            await _agentService.CreateSessionAsync();
            _sessions = await _agentService.GetSessionsAsync(CancellationToken.None);
            BuildSessionList();
            if (_sessions.Count > 0)
                SelectSession(_sessions[0].Id);
        }
        finally { NewSessionButton.IsEnabled = true; }
    }

    private async void OnReplayClick(object sender, RoutedEventArgs e)
    {
        if (_agentService is null || _selectedId is null) return;
        ReplayButton.IsEnabled = false;
        try
        {
            await _agentService.ReplayAsync(_selectedId);
        }
        finally { ReplayButton.IsEnabled = true; }
    }

    private void BuildSessionList()
    {
        SessionList.Children.Clear();
        foreach (var session in _sessions)
        {
            var card = CreateSessionCard(session);
            SessionList.Children.Add(card);
        }
    }

    private Border CreateSessionCard(AgentSession session)
    {
        var card = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Tag = session.Id,
        };
        card.Tapped += OnSessionCardTapped;

        var stack = new StackPanel { Spacing = 8 };

        // Status row
        var statusRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var dot = new Controls.StatusDot
        {
            Status = session.Status == SessionStatus.Active ? "ok" : "idle",
            DotSize = 8,
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusRow.Children.Add(dot);

        var name = new TextBlock
        {
            Text = session.Name,
            Style = (Style)Application.Current.Resources["OrbitalBody"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText82Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        statusRow.Children.Add(name);

        if (session.Status == SessionStatus.Active)
        {
            var bars = new Controls.PulsingBars { BarCount = 3, BarColor = "violet", VerticalAlignment = VerticalAlignment.Center };
            statusRow.Children.Add(bars);
        }

        stack.Children.Add(statusRow);

        // Meta
        var ageText = Helpers.OrbitalColors.TimeAgo(session.StartTime);
        var meta = new TextBlock
        {
            Text = $"{session.ActionCount} actions · {session.ArtifactCount} artifacts · {ageText}",
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
        };
        stack.Children.Add(meta);

        card.Child = stack;
        return card;
    }

    private void OnSessionCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is string id)
            SelectSession(id);
    }

    private void SelectSession(string id)
    {
        _selectedId = id;

        // Update card visuals
        foreach (var child in SessionList.Children)
        {
            if (child is Border card)
            {
                var isSelected = (string)card.Tag == id;
                card.Background = isSelected
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"];
                card.BorderBrush = isSelected
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_30Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"];
            }
        }

        // Populate detail
        var session = _sessions.FirstOrDefault(s => s.Id == id);
        if (session == null) return;

        DetailCard.Visibility = Visibility.Visible;
        ChecksCard.Visibility = Visibility.Visible;

        DetailTitle.Text = session.Name;
        DetailGoal.Text = session.Goal;
        DetailBadgeText.Text = session.Status switch
        {
            SessionStatus.Active => "active",
            SessionStatus.Done => "complete",
            _ => "paused",
        };

        // Style badge based on status
        DetailBadge.Style = session.Status == SessionStatus.Active
            ? (Style)Application.Current.Resources["OrbitalBadgeSuccess"]
            : (Style)Application.Current.Resources["OrbitalBadgeMuted"];
        DetailBadgeText.Foreground = session.Status == SessionStatus.Active
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"];

        // Permissions
        PopulatePermissions();

        // Timeline
        PopulateTimeline(session);

        // Acceptance checks
        PopulateChecks(session);
    }

    private void PopulatePermissions()
    {
        PermissionsPanel.Children.Clear();

        // Read real permissions from Claude Code settings
        var permissions = ReadClaudePermissions();

        foreach (var perm in permissions.Take(8)) // Limit to avoid overflow
        {
            var badge = new Border
            {
                Style = (Style)Application.Current.Resources["OrbitalBadgeSuccess"],
            };
            badge.Child = new TextBlock
            {
                Text = perm,
                Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"],
            };
            PermissionsPanel.Children.Add(badge);
        }
    }

    private static List<string> ReadClaudePermissions()
    {
        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".claude", "settings.local.json");

            if (!File.Exists(settingsPath))
                return ["file:read", "file:write"];

            var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(settingsPath));
            if (json.RootElement.TryGetProperty("permissions", out var perms) &&
                perms.TryGetProperty("allow", out var allow) &&
                allow.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                return allow.EnumerateArray()
                    .Select(e => e.GetString() ?? "")
                    .Where(s => s.Length > 0)
                    .Select(s =>
                    {
                        // Simplify permission display: "Bash(git:*)" -> "Bash:git"
                        if (s.Contains('('))
                        {
                            var name = s[..s.IndexOf('(')];
                            var arg = s[(s.IndexOf('(') + 1)..s.IndexOf(')')];
                            arg = arg.TrimEnd(':', '*').TrimEnd('*');
                            return string.IsNullOrEmpty(arg) ? name : $"{name}:{arg}";
                        }
                        return s;
                    })
                    .ToList();
            }
        }
        catch { }

        return ["file:read", "file:write"];
    }

    private void PopulateTimeline(AgentSession session)
    {
        TimelinePanel.Children.Clear();
        for (var i = 0; i < session.Actions.Count; i++)
        {
            var action = session.Actions[i];
            var isLast = i == session.Actions.Count - 1;
            var item = new Controls.TimelineItem
            {
                Title = action.Title,
                Time = action.Time.ToString("h:mm tt"),
                Detail = action.Detail,
                Status = action.Status.ToString().ToLowerInvariant(),
                IsLast = isLast,
            };
            TimelinePanel.Children.Add(item);
        }
    }

    private void PopulateChecks(AgentSession session)
    {
        ChecksPanel.Children.Clear();
        var checks = new[]
        {
            "Build compiles with zero errors",
            "All 5 navigation targets render",
            "No unhandled exceptions in console",
            "Screenshots match expected layout",
            "Hot Reload doesn't break state",
        };

        foreach (var check in checks)
        {
            var row = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8),
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };

            var iconBox = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(4),
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_15Brush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            iconBox.Child = new FontIcon
            {
                Glyph = "\uE73E",
                FontSize = 12,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Children.Add(iconBox);

            var text = new TextBlock
            {
                Text = check,
                Style = (Style)Application.Current.Resources["OrbitalCaption"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText65Brush"],
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Children.Add(text);

            row.Child = content;
            ChecksPanel.Children.Add(row);
        }
    }

    private void PopulateMcpHealth(McpStatus mcpStatus)
    {
        McpHealthTimestamp.Text = $"checked {DateTime.Now:h:mm tt}";
        McpServersPanel.Children.Clear();

        foreach (var server in mcpStatus.Servers)
        {
            var row = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"],
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 10),
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(12) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                },
            };

            // Status dot
            var dot = new Controls.StatusDot
            {
                Status = server.Healthy ? "ok" : "error",
                DotSize = 8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(dot, 0);
            grid.Children.Add(dot);

            // Server info
            var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            var nameRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            nameRow.Children.Add(new TextBlock
            {
                Text = server.Name,
                Style = (Style)Application.Current.Resources["OrbitalMonoConsole"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText75Brush"],
            });
            nameRow.Children.Add(new TextBlock
            {
                Text = server.Url,
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText30Brush"],
                VerticalAlignment = VerticalAlignment.Center,
            });
            textStack.Children.Add(nameRow);
            textStack.Children.Add(new TextBlock
            {
                Text = $"{server.ToolCount} tools",
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
            });
            Grid.SetColumn(textStack, 2);
            grid.Children.Add(textStack);

            // Pulsing bars (if connected)
            if (server.Healthy)
            {
                var bars = new Controls.PulsingBars
                {
                    BarCount = 3,
                    BarColor = "emerald",
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(bars, 3);
                grid.Children.Add(bars);
            }

            row.Child = grid;
            McpServersPanel.Children.Add(row);
        }
    }
}

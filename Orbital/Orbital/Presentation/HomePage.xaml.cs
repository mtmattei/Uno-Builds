using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class HomePage : Page
{
    private readonly List<Border> _breatheCards = [];

    public HomePage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Staggered fade-up entrance per design spec section 8
        AnimationHelper.FadeUp(HeroSection, 0);
        AnimationHelper.FadeUp(ClockSection, 80);
        AnimationHelper.FadeUp(VersionStrip, 140);
        AnimationHelper.FadeUp(TerminalSection, 200);
        AnimationHelper.FadeUp(Card0, 300);
        AnimationHelper.FadeUp(Card1, 370);
        AnimationHelper.FadeUp(Card2, 440);
        AnimationHelper.FadeUp(Card3, 510);
        AnimationHelper.FadeUp(ActionsPanel, 600);
        AnimationHelper.FadeUp(SessionPanel, 660);
        AnimationHelper.FadeUp(ProjectsSection, 750);

        // Border breathe on status cards
        AnimationHelper.StartBorderBreathe(Card0);
        AnimationHelper.StartBorderBreathe(Card1);
        AnimationHelper.StartBorderBreathe(Card2);
        AnimationHelper.StartBorderBreathe(Card3);
        _breatheCards.AddRange([Card0, Card1, Card2, Card3]);

        // Start persistent orb float
        OrbFloatAnimation.Begin();

        // Wire action buttons
        QuickRunUnoCheck.Click += async (_, _) => await NavigateToRouteAsync("Diagnostics");
        QuickBuildRun.Click += OnBuildRunClick;
        QuickOpenDocs.Click += async (_, _) =>
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://platform.uno/docs/articles/intro.html"));
        QuickNewProject.Click += OnNewProjectClick;
        BrowseAllButton.Click += OnBrowseAllClick;
        SessionPanel.Tapped += async (_, _) => await NavigateToRouteAsync("Agents");

        // Populate data that can't be MVUX-bound (dynamic UI building)
        try { await PopulateRealDataAsync(); } catch { }
        try { await PopulateRecentProjectsAsync(); } catch { }
    }

    private async Task NavigateToRouteAsync(string route)
    {
        var navigator = this.Navigator();
        if (navigator is not null)
            await navigator.NavigateRouteAsync(this, route);
    }

    private async void OnBuildRunClick(object sender, RoutedEventArgs e)
    {
        var host = HostHelper.GetHost();
        if (host is null) return;

        var btn = (Button)sender;
        btn.IsEnabled = false;
        try
        {
            await host.Services.GetRequiredService<IBuildService>().BuildRunVerifyAsync();
            await NavigateToRouteAsync("Project");
        }
        finally { btn.IsEnabled = true; }
    }

    private async void OnNewProjectClick(object sender, RoutedEventArgs e)
    {
        var host = HostHelper.GetHost();
        if (host is null) return;
        var app = (App)Application.Current;

        var nameBox = new TextBox
        {
            PlaceholderText = "MyUnoApp",
            Header = "Project Name",
            Margin = new Thickness(0, 0, 0, 12),
        };

        var pathDisplay = new TextBlock
        {
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText50Brush"],
        };
        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dev");
        pathDisplay.Text = defaultPath;

        var browseBtn = new Button
        {
            Content = "Browse...",
            Style = (Style)Application.Current.Resources["OrbitalGhostButtonSm"],
            Margin = new Thickness(0, 4, 0, 0),
        };
        browseBtn.Click += async (_, _) =>
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(app.AppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
                pathDisplay.Text = folder.Path;
        };

        var pathPanel = new StackPanel { Spacing = 4 };
        pathPanel.Children.Add(new TextBlock
        {
            Text = "Output Directory",
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText65Brush"],
        });
        pathPanel.Children.Add(pathDisplay);
        pathPanel.Children.Add(browseBtn);

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(nameBox);
        panel.Children.Add(pathPanel);

        var dialog = new ContentDialog
        {
            Title = "New Uno Platform Project",
            Content = panel,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var projectName = nameBox.Text?.Trim();
        var outputPath = pathDisplay.Text?.Trim();
        if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(outputPath)) return;

        QuickNewProject.IsEnabled = false;
        try
        {
            var envService = host.Services.GetRequiredService<IEnvironmentService>();
            var createResult = await envService.CreateProjectAsync(projectName, outputPath, CancellationToken.None);

            var resultDialog = new ContentDialog
            {
                Title = createResult.Success ? "Project Created" : "Creation Failed",
                Content = createResult.Success
                    ? $"Project '{projectName}' created at:\n{createResult.ProjectPath}"
                    : $"Error:\n{createResult.Output}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await resultDialog.ShowAsync();
        }
        finally { QuickNewProject.IsEnabled = true; }
    }

    private async Task PopulateRealDataAsync()
    {
        // Status cards, version pills, avatar initial, greeting, clock, and date
        // are now populated via MVUX bindings from HomeModel feeds.
        // Only the active session timeline remains as code-behind (dynamic UI).

        var host = await HostHelper.WaitForHostAsync();
        if (host is null) return;

        await PopulateActiveSessionAsync(host);
    }

    private async Task PopulateActiveSessionAsync(IHost host)
    {
        try
        {
            var agentService = host.Services.GetRequiredService<IAgentService>();
            var session = await agentService.GetActiveSessionAsync(CancellationToken.None);

            if (session is null)
            {
                SessionName.Text = "No sessions found";
                SessionMeta.Text = "Start a Claude Code session to see activity here";
                SessionStatusText.Text = "None";
                SessionStatusText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"];
                SessionPulse.Visibility = Visibility.Collapsed;
                return;
            }

            SessionName.Text = session.Name;
            SessionMeta.Text = $"{session.Repo} \u00B7 {session.Branch} \u00B7 {session.ActionCount} actions \u00B7 {session.ArtifactCount} artifacts";

            if (session.Status == SessionStatus.Active)
            {
                SessionStatusText.Text = "Active";
                SessionPulse.Visibility = Visibility.Visible;
            }
            else
            {
                SessionStatusText.Text = "Done";
                SessionStatusText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"];
                SessionPulse.Visibility = Visibility.Collapsed;
            }

            // Populate timeline from real actions (up to 5)
            SessionTimeline.Children.Clear();
            var actions = session.Actions;
            for (var i = 0; i < actions.Count && i < 5; i++)
            {
                var action = actions[i];
                var item = new Controls.TimelineItem
                {
                    Title = action.Title,
                    Time = action.Time.ToString("h:mmtt").ToLowerInvariant(),
                    Status = action.Status == ActionStatus.Ok ? "ok"
                           : action.Status == ActionStatus.Warn ? "warn"
                           : action.Status == ActionStatus.Error ? "error" : "idle",
                    Detail = action.Detail,
                    IsLast = i == actions.Count - 1 || i == 4,
                };
                SessionTimeline.Children.Add(item);
            }

            if (actions.Count == 0)
            {
                SessionTimeline.Children.Add(new Controls.TimelineItem
                {
                    Title = "Session started",
                    Time = session.StartTime.ToString("h:mmtt").ToLowerInvariant(),
                    Status = "ok",
                    Detail = $"{session.ActionCount} tool uses recorded",
                    IsLast = true,
                });
            }
        }
        catch
        {
            SessionName.Text = "Error loading session";
            SessionMeta.Text = "";
            SessionStatusText.Text = "Error";
        }
    }

    private async void OnBrowseAllClick(object sender, RoutedEventArgs e)
    {
        var host = HostHelper.GetHost();
        if (host is null) return;

        var app = (App)Application.Current;
        var ctx = host.Services.GetRequiredService<IProjectContext>();

        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(app.AppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder is null) return;

        var project = await ctx.OpenProjectAsync(folder.Path, CancellationToken.None);
        if (project is not null)
        {
            await PopulateRecentProjectsAsync();
            await NavigateToRouteAsync("Project");
        }
        else
        {
            var errorDialog = new ContentDialog
            {
                Title = "Could not open project",
                Content = $"No .csproj or .sln found at:\n{folder.Path}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await errorDialog.ShowAsync();
        }
    }

    private async Task PopulateRecentProjectsAsync()
    {
        var host = HostHelper.GetHost();
        if (host is null) return;

        try
        {
            var ctx = host.Services.GetRequiredService<IProjectContext>();
            var recents = await ctx.GetRecentProjectsAsync(CancellationToken.None);

            RecentProjectsContainer.Children.Clear();
            foreach (var project in recents)
            {
                var row = CreateProjectRow(project, ctx);
                RecentProjectsContainer.Children.Add(row);
            }
        }
        catch { }
    }

    private Grid CreateProjectRow(OrbitalProject project, IProjectContext ctx)
    {
        var statusStr = project.Status switch
        {
            HealthStatus.Ok => "ok",
            HealthStatus.Warn => "warn",
            HealthStatus.Error => "error",
            _ => "idle",
        };

        var timeAgo = OrbitalColors.TimeAgo(project.LastOpened);

        var grid = new Grid
        {
            Padding = new Thickness(16, 12),
            CornerRadius = new CornerRadius(8),
            Tag = project,
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var dot = new Controls.StatusDot { Status = statusStr, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(dot, 0);
        grid.Children.Add(dot);

        var nameStack = new StackPanel { Spacing = 2 };
        nameStack.Children.Add(new TextBlock
        {
            Text = project.Name,
            Style = (Style)Application.Current.Resources["OrbitalBody"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText80Brush"],
        });
        nameStack.Children.Add(new TextBlock
        {
            Text = project.RootDirectory,
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
        });
        Grid.SetColumn(nameStack, 2);
        grid.Children.Add(nameStack);

        var branchBadge = new Border
        {
            Style = (Style)Application.Current.Resources["OrbitalBadgeMuted"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        branchBadge.Child = new TextBlock
        {
            Text = project.Branch ?? "main",
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"],
        };
        Grid.SetColumn(branchBadge, 3);
        grid.Children.Add(branchBadge);

        var timeText = new TextBlock
        {
            Text = timeAgo,
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText30Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(timeText, 5);
        grid.Children.Add(timeText);

        var chevron = new FontIcon
        {
            Glyph = "\uE76C",
            FontSize = 14,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText25Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(chevron, 7);
        grid.Children.Add(chevron);

        // Hover + tap
        grid.PointerEntered += (s, _) =>
            ((Grid)s).Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"];
        grid.PointerExited += (s, _) =>
            ((Grid)s).Background = null;
        grid.Tapped += async (s, _) =>
        {
            if (((FrameworkElement)s).Tag is OrbitalProject p)
            {
                ctx.SetActiveProject(p);
                await NavigateToRouteAsync("Project");
            }
        };

        return grid;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        OrbFloatAnimation.Stop();
        foreach (var card in _breatheCards)
            AnimationHelper.StopBorderBreathe(card);
        _breatheCards.Clear();
    }
}

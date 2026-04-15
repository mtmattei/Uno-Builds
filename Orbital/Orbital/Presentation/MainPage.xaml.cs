using Microsoft.UI.Xaml.Media.Animation;
using Orbital.Controls;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class MainPage : Page
{
    private readonly DispatcherTimer _spinTimer;
    private static readonly SolidColorBrush _paneBrush =
        new(Windows.UI.Color.FromArgb(0xFF, 0x0A, 0x0A, 0x0B));

    private List<SearchResult> _searchIndex = [];

    public MainPage()
    {
        this.InitializeComponent();

        // Periodic logo spin every 8 seconds
        _spinTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        _spinTimer.Tick += (_, _) => SpinLogo();

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _spinTimer.Start();
        _ = InitialSpinAsync();
        ForcePaneBackground(NavView);
        ForceNavItemCornerRadius(NavView);

        // Project selector — use Click on Button (Tapped on Border gets swallowed by NavigationView pane)
        ProjectSelectorButton.Click += OnProjectSelectorClick;

        // Sidebar collapse: hide text-heavy elements when pane is icon-only
        NavView.PaneOpening += (_, _) => SetPaneContentVisibility(true);
        NavView.PaneClosing += (_, _) => SetPaneContentVisibility(false);

        // Search: wire events
        PageHeader.SearchRequested += OnSearchRequested;
        SearchInput.TextChanged += OnSearchTextChanged;
        SearchInput.KeyDown += OnSearchKeyDown;
        SearchBackdrop.Tapped += (_, _) => CloseSearch();

        // Ctrl+K accelerator
        this.KeyDown += OnPageKeyDown;

        // Build search index eagerly in background so Ctrl+K is instant
        _ = BuildSearchIndexAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _spinTimer.Stop();
        PageHeader.SearchRequested -= OnSearchRequested;
        this.KeyDown -= OnPageKeyDown;
    }

    // ─── Search ───────────────────────────────────────────────

    private void OnPageKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.K &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            OpenSearch();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Escape && SearchOverlay.Visibility == Visibility.Visible)
        {
            CloseSearch();
            e.Handled = true;
        }
    }

    private void OnSearchRequested(object? sender, EventArgs e) => OpenSearch();

    private void OpenSearch()
    {
        SearchOverlay.Visibility = Visibility.Visible;
        SearchInput.Text = "";
        PopulateResults(_searchIndex);

        // Focus the text box after a frame so it's visible
        DispatcherQueue.TryEnqueue(() => SearchInput.Focus(FocusState.Programmatic));
    }

    private void CloseSearch()
    {
        SearchOverlay.Visibility = Visibility.Collapsed;
        SearchInput.Text = "";
    }

    private void OnSearchKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            CloseSearch();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Enter)
        {
            // Select the first visible result
            if (SearchResultsList.Children.Count > 0 &&
                SearchResultsList.Children[0] is FrameworkElement firstItem &&
                firstItem.Tag is SearchResult result)
            {
                _ = NavigateToResultAsync(result);
                e.Handled = true;
            }
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchInput.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(query))
        {
            PopulateResults(_searchIndex);
            return;
        }

        var filtered = _searchIndex
            .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        r.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        PopulateResults(filtered);
    }

    private void PopulateResults(List<SearchResult> results)
    {
        SearchResultsList.Children.Clear();
        SearchEmpty.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SearchResultsScroller.Visibility = results.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        string? lastCategory = null;
        foreach (var result in results)
        {
            // Category header
            if (result.Category != lastCategory)
            {
                lastCategory = result.Category;
                var header = new TextBlock
                {
                    Text = result.Category.ToUpperInvariant(),
                    Style = (Style)Application.Current.Resources["OrbitalSectionSubLabel"],
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText30Brush"],
                    Margin = new Thickness(8, results.IndexOf(result) > 0 ? 8 : 4, 8, 4),
                };
                SearchResultsList.Children.Add(header);
            }

            var row = CreateResultRow(result);
            SearchResultsList.Children.Add(row);
        }
    }

    private Border CreateResultRow(SearchResult result)
    {
        var row = new Border
        {
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Tag = result,
        };

        // Hover effect
        row.PointerEntered += (s, _) =>
            ((Border)s).Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"];
        row.PointerExited += (s, _) =>
            ((Border)s).Background = null;
        row.Tapped += async (s, args) =>
        {
            if (((FrameworkElement)s).Tag is SearchResult r)
                await NavigateToResultAsync(r);
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(10) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // Icon
        var icon = new FontIcon
        {
            Glyph = result.Icon,
            FontSize = 14,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText42Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        // Name + description
        var textStack = new StackPanel { Spacing = 1, VerticalAlignment = VerticalAlignment.Center };
        textStack.Children.Add(new TextBlock
        {
            Text = result.Name,
            Style = (Style)Application.Current.Resources["OrbitalMonoConsole"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText75Brush"],
        });
        if (!string.IsNullOrEmpty(result.Description))
        {
            textStack.Children.Add(new TextBlock
            {
                Text = result.Description,
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
                MaxLines = 1,
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
        }
        Grid.SetColumn(textStack, 2);
        grid.Children.Add(textStack);

        // Route badge
        var badge = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };
        badge.Child = new TextBlock
        {
            Text = result.Route,
            FontFamily = (Microsoft.UI.Xaml.Media.FontFamily)Application.Current.Resources["OrbitalMonoFont"],
            FontSize = 10,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText30Brush"],
        };
        Grid.SetColumn(badge, 3);
        grid.Children.Add(badge);

        row.Child = grid;
        return row;
    }

    private async Task NavigateToResultAsync(SearchResult result)
    {
        CloseSearch();
        var navigator = this.Navigator();
        if (navigator is not null)
            await navigator.NavigateRouteAsync(this, result.Route);
    }

    private async Task BuildSearchIndexAsync()
    {
        var index = new List<SearchResult>();

        // Static: Navigation pages
        index.AddRange([
            new("Home", "Dashboard overview with status cards", "Pages", "Home", "\uE80F"),
            new("Project", "Build, run, and artifacts", "Pages", "Project", "\uE943"),
            new("Agent Sessions", "Claude Code session history", "Pages", "Agents", "\uE99A"),
            new("Studio", "Uno Platform Studio license and connectors", "Pages", "Studio", "\uE8D7"),
            new("Skills", "Claude Code skill library", "Pages", "Skills", "\uE945"),
            new("Diagnostics", "Environment checks and dependencies", "Pages", "Diagnostics", "\uE9D9"),
            new("Settings", "Preferences and terminal", "Pages", "Settings", "\uE713"),
        ]);

        // Static: Quick actions
        index.AddRange([
            new("Run Uno Check", "Verify development environment", "Actions", "Diagnostics", "\uE9D9"),
            new("Build + Run", "Build and launch the app", "Actions", "Project", "\uE768"),
            new("New Project", "Create a new Uno Platform project", "Actions", "Home", "\uE8F4"),
            new("Open Docs", "Uno Platform documentation", "Actions", "Home", "\uE8A5"),
        ]);

        // Dynamic: load from services
        var host = await HostHelper.WaitForHostAsync();
        if (host is not null)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var ct = cts.Token;

            // Fire all service calls in parallel
            var skillsTask = SafeCall(() => host.Services.GetRequiredService<ISkillsService>().GetSkillsAsync(ct));
            var depsTask = SafeCall(() => host.Services.GetRequiredService<IDiagnosticsService>().GetDependenciesAsync(ct));
            var toolsTask = SafeCall(() => host.Services.GetRequiredService<IDiagnosticsService>().GetRuntimeToolsAsync(ct));
            var featuresTask = SafeCall(() => host.Services.GetRequiredService<IStudioService>().GetFeaturesAsync(ct));
            var connectorsTask = SafeCall(() => host.Services.GetRequiredService<IStudioService>().GetConnectorsAsync(ct));
            var sessionsTask = SafeCall(() => host.Services.GetRequiredService<IAgentService>().GetSessionsAsync(ct));
            var platformsTask = SafeCall(() => host.Services.GetRequiredService<IDiagnosticsService>().GetPlatformTargetsAsync(ct));

            await Task.WhenAll(skillsTask, depsTask, toolsTask, featuresTask, connectorsTask, sessionsTask, platformsTask);

            if (skillsTask.Result is { } skills)
                foreach (var s in skills) index.Add(new(s.Name, s.Description, "Skills", "Skills", "\uE945"));
            if (depsTask.Result is { } deps)
                foreach (var d in deps) index.Add(new(d.Package, $"v{d.CurrentVersion}", "Dependencies", "Diagnostics", "\uE74C"));
            if (toolsTask.Result is { } tools)
                foreach (var t in tools) index.Add(new(t.Name, t.Description, "Tools", "Diagnostics", "\uE950"));
            if (featuresTask.Result is { } features)
                foreach (var f in features) index.Add(new(f.Name, f.Description, "Studio Features", "Studio", "\uE73E"));
            if (connectorsTask.Result is { } connectors)
                foreach (var c in connectors) index.Add(new(c.Name, c.Url, "Connectors", "Studio", "\uE703"));
            if (sessionsTask.Result is { } sessions)
                foreach (var s in sessions) index.Add(new(s.Name, $"{s.ActionCount} actions \u00B7 {s.ArtifactCount} artifacts", "Sessions", "Agents", "\uE99A"));
            if (platformsTask.Result is { } platforms)
                foreach (var p in platforms) index.Add(new(p.Name, p.Sdk, "Platforms", "Diagnostics", "\uE770"));
        }

        static async Task<T?> SafeCall<T>(Func<ValueTask<T>> call) where T : class
        {
            try { return await call(); } catch { return null; }
        }

        _searchIndex = index;
    }

    // ─── Project Selector ──────────────────────────────────────

    private async void OnProjectSelectorClick(object sender, RoutedEventArgs e)
    {
        try
        {
        var host = await HostHelper.WaitForHostAsync();
        if (host is null) return;

        var ctx = host.Services.GetRequiredService<IProjectContext>();
        var recents = await ctx.GetRecentProjectsAsync(CancellationToken.None);

        ProjectFlyoutContent.Children.Clear();

        foreach (var project in recents)
        {
            var isActive = ctx.ActiveProject?.SolutionPath == project.SolutionPath;

            var rowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                },
            };

            var btn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8, 6),
                Tag = project,
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            if (isActive)
            {
                stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Ellipse
                {
                    Width = 6, Height = 6,
                    Fill = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"],
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            stack.Children.Add(new TextBlock
            {
                Text = project.Name,
                Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
                Foreground = isActive
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText65Brush"],
                VerticalAlignment = VerticalAlignment.Center,
            });
            btn.Content = stack;

            btn.Click += async (s, _) =>
            {
                ProjectFlyout.Hide();
                if (((Button)s).Tag is OrbitalProject p)
                {
                    ctx.SetActiveProject(p);
                    ProjectSelectorName.Text = p.Name;
                }
            };
            Grid.SetColumn(btn, 0);
            rowGrid.Children.Add(btn);

            // Remove button (not on the active project)
            if (!isActive)
            {
                var solutionPath = project.SolutionPath;
                var removeBtn = new Button
                {
                    Content = new FontIcon { Glyph = "\uE711", FontSize = 10 },
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(4),
                    MinWidth = 0,
                    MinHeight = 0,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.4,
                };
                removeBtn.Click += (_, _) =>
                {
                    ctx.RemoveRecentProject(solutionPath);
                    // Remove the row from the flyout
                    ProjectFlyoutContent.Children.Remove(rowGrid);
                };
                Grid.SetColumn(removeBtn, 1);
                rowGrid.Children.Add(removeBtn);
            }

            ProjectFlyoutContent.Children.Add(rowGrid);
        }

        // "Open Project..." button
        var openBtn = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(8, 6),
            Margin = new Thickness(0, 4, 0, 0),
        };
        var openStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        openStack.Children.Add(new FontIcon
        {
            Glyph = "\uE8E5",
            FontSize = 12,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText42Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        });
        openStack.Children.Add(new TextBlock
        {
            Text = "Open Project...",
            Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText42Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        });
        openBtn.Content = openStack;
        openBtn.Click += async (_, _) =>
        {
            ProjectFlyout.Hide();
            await ShowOpenProjectDialogAsync(ctx);
        };
        ProjectFlyoutContent.Children.Add(openBtn);
        }
        catch { /* fire-and-forget safety */ }
    }

    private async Task ShowOpenProjectDialogAsync(IProjectContext ctx)
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        folderPicker.FileTypeFilter.Add("*");

        // Initialize with window handle for Uno.WinUI
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).AppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder is null) return;

        var path = folder.Path;
        var project = await ctx.OpenProjectAsync(path, CancellationToken.None);
        if (project is not null)
        {
            ProjectSelectorName.Text = project.Name;
        }
        else
        {
            var errorDialog = new ContentDialog
            {
                Title = "Could not open project",
                Content = $"No .csproj or .sln found at:\n{path}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await errorDialog.ShowAsync();
        }
    }

    // ─── Pane collapse ─────────────────────────────────────

    private void SetPaneContentVisibility(bool expanded)
    {
        var vis = expanded ? Visibility.Visible : Visibility.Collapsed;
        LogoTextBlock.Visibility = vis;
        ProjectSectionLabel.Visibility = vis;
        ProjectSelectorButton.Visibility = vis;
        NavigateSectionLabel.Visibility = vis;
        ClockRow.Visibility = vis;
        McpCard.Visibility = vis;
    }

    // ─── Logo spin / Visual tree ────────────────────────────

    private async Task InitialSpinAsync()
    {
        await Task.Delay(1500);
        SpinLogo();
    }

    private void SpinLogo()
    {
        var sb = new Storyboard();
        var spinAnim = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromMilliseconds(800)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTarget(spinAnim, LogoRotate);
        Storyboard.SetTargetProperty(spinAnim, "Angle");
        sb.Children.Add(spinAnim);
        sb.Begin();
    }

    private static readonly SolidColorBrush _transparent = new(Windows.UI.Color.FromArgb(0, 0, 0, 0));
    private static readonly CornerRadius _navItemRadius = new(8);

    private static void ForceNavItemCornerRadius(DependencyObject root)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is Microsoft.UI.Xaml.Controls.Primitives.NavigationViewItemPresenter presenter)
            {
                presenter.CornerRadius = _navItemRadius;
                continue;
            }
            ForceNavItemCornerRadius(child);
        }
    }

    private static void ForcePaneBackground(DependencyObject root)
    {
        var sv = FindDescendant<SplitView>(root);
        if (sv is null) return;

        sv.PaneBackground = _paneBrush;

        var paneRoot = FindDescendantByName(sv, "PaneRoot") as Grid;
        if (paneRoot is not null)
        {
            ClearInternalBackgrounds(paneRoot);
        }
    }

    private static void ClearInternalBackgrounds(DependencyObject root)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is Grid g && g.Background is not null)
            {
                g.Background = _transparent;
            }
            else if (child is Border b && b.Background is not null)
            {
                b.Background = _transparent;
            }
            if (child is ContentControl) continue;
            ClearInternalBackgrounds(child);
        }
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : class
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match) return match;
            var result = FindDescendant<T>(child);
            if (result is not null) return result;
        }
        return null;
    }

    private static DependencyObject? FindDescendantByName(DependencyObject root, string name)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is FrameworkElement fe && fe.Name == name) return fe;
            var result = FindDescendantByName(child, name);
            if (result is not null) return result;
        }
        return null;
    }
}

using Microsoft.UI.Xaml.Input;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class ProjectPage : Page
{
    private static readonly SolidColorBrush _transparentBrush = new(Microsoft.UI.Colors.Transparent);
    private IBuildService? _buildService;
    private IProjectContext? _ctx;

    public ProjectPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimationHelper.FadeUp(MetaStrip, 0);
        AnimationHelper.FadeUp(TaskBar, 100);
        AnimationHelper.FadeUp(ConsoleCard, 200);

        var host = HostHelper.GetHost();
        if (host is null) return;
        _buildService = host.Services.GetRequiredService<IBuildService>();
        _ctx = host.Services.GetRequiredService<IProjectContext>();
        _ctx.ActiveProjectChanged += OnActiveProjectChanged;

        // Load all console data in parallel
        var buildTask = _buildService.GetLastBuildOutputAsync(CancellationToken.None).AsTask();
        var runTask = _buildService.GetLastRunOutputAsync(CancellationToken.None).AsTask();
        var artifactsTask = _buildService.GetArtifactsAsync(CancellationToken.None).AsTask();
        await Task.WhenAll(buildTask, runTask, artifactsTask);

        BuildConsole.SetLines(buildTask.Result);
        RunConsole.SetLines(runTask.Result);
        PopulateArtifacts(artifactsTask.Result);

        // Wire button handlers
        BuildButton.Click += OnBuildClick;
        RunButton.Click += OnRunClick;
        BuildRunVerifyButton.Click += OnBuildRunVerifyClick;
        PackageButton.Click += OnPackageClick;
        HotReloadButton.Click += OnHotReloadClick;
        SmokeTestButton.Click += OnSmokeTestClick;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_ctx is not null)
            _ctx.ActiveProjectChanged -= OnActiveProjectChanged;
    }

    private void OnActiveProjectChanged()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_buildService is not null)
                _ = ReloadDataAsync();
        });
    }

    private async Task ReloadDataAsync()
    {
        var buildTask = _buildService!.GetLastBuildOutputAsync(CancellationToken.None).AsTask();
        var runTask = _buildService.GetLastRunOutputAsync(CancellationToken.None).AsTask();
        var artifactsTask = _buildService.GetArtifactsAsync(CancellationToken.None).AsTask();
        await Task.WhenAll(buildTask, runTask, artifactsTask);
        BuildConsole.SetLines(buildTask.Result);
        RunConsole.SetLines(runTask.Result);
        PopulateArtifacts(artifactsTask.Result);
    }


    private async void OnBuildClick(object sender, RoutedEventArgs e)
    {
        if (_buildService is null) return;
        BuildButton.IsEnabled = false;
        SetActiveTab(0);
        BuildConsole.SetLines(ImmutableList.Create(new Controls.ConsoleLine("Building...", "dim")));
        try
        {
            await _buildService.BuildAsync();
            BuildConsole.SetLines(await _buildService.GetLastBuildOutputAsync(CancellationToken.None));
        }
        finally { BuildButton.IsEnabled = true; }
    }

    private async void OnRunClick(object sender, RoutedEventArgs e)
    {
        if (_buildService is null) return;
        RunButton.IsEnabled = false;
        SetActiveTab(1);
        RunConsole.SetLines(ImmutableList.Create(new Controls.ConsoleLine("Starting...", "dim")));
        try
        {
            await _buildService.RunAsync();
            RunConsole.SetLines(await _buildService.GetLastRunOutputAsync(CancellationToken.None));
        }
        finally { RunButton.IsEnabled = true; }
    }

    private async void OnBuildRunVerifyClick(object sender, RoutedEventArgs e)
    {
        if (_buildService is null) return;
        BuildRunVerifyButton.IsEnabled = false;
        SetActiveTab(0);
        BuildConsole.SetLines(ImmutableList.Create(new Controls.ConsoleLine("Build > Run > Verify...", "dim")));
        try
        {
            await _buildService.BuildRunVerifyAsync();
            BuildConsole.SetLines(await _buildService.GetLastBuildOutputAsync(CancellationToken.None));
        }
        finally { BuildRunVerifyButton.IsEnabled = true; }
    }

    private async void OnPackageClick(object sender, RoutedEventArgs e)
    {
        if (_buildService is null) return;
        PackageButton.IsEnabled = false;
        SetActiveTab(0);
        BuildConsole.SetLines(ImmutableList.Create(new Controls.ConsoleLine("Packaging...", "dim")));
        try
        {
            await _buildService.PackageAsync();
            BuildConsole.SetLines(await _buildService.GetLastBuildOutputAsync(CancellationToken.None));
        }
        finally { PackageButton.IsEnabled = true; }
    }

    private void OnHotReloadClick(object sender, RoutedEventArgs e)
    {
        // Hot Reload is managed by the runtime — show current status
        SetActiveTab(1);
        RunConsole.SetLines(ImmutableList.Create(
            new Controls.ConsoleLine("$ Hot Reload Status", "dim"),
            new Controls.ConsoleLine("", "dim"),
            new Controls.ConsoleLine($"  DOTNET_MODIFIABLE_ASSEMBLIES = {Environment.GetEnvironmentVariable("DOTNET_MODIFIABLE_ASSEMBLIES") ?? "(not set)"}", "info"),
            new Controls.ConsoleLine($"  Runtime: .NET {Environment.Version}", "info"),
            new Controls.ConsoleLine($"  PID: {Environment.ProcessId}", "info"),
            new Controls.ConsoleLine("", "dim"),
            new Controls.ConsoleLine("  Hot Reload is managed by the IDE or launch script.", "dim"),
            new Controls.ConsoleLine("  Set DOTNET_MODIFIABLE_ASSEMBLIES=debug before launch to enable.", "dim")));
    }

    private async void OnSmokeTestClick(object sender, RoutedEventArgs e)
    {
        if (_buildService is null) return;
        SmokeTestButton.IsEnabled = false;
        SetActiveTab(0);
        BuildConsole.SetLines(ImmutableList.Create(new Controls.ConsoleLine("Running UI smoke test...", "dim")));
        try
        {
            var results = await _buildService.RunSmokeTestAsync();
            BuildConsole.SetLines(results);
        }
        finally { SmokeTestButton.IsEnabled = true; }
    }

    private void PopulateArtifacts(ImmutableList<Artifact> artifacts)
    {
        ArtifactsPanel.Children.Clear();
        foreach (var artifact in artifacts)
        {
            var row = new Border
            {
                Style = (Style)Application.Current.Resources["OrbitalConsoleSurfaceStyle"],
                Margin = new Thickness(0, 0, 0, 0),
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(16) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                },
            };

            var icon = new FontIcon
            {
                Glyph = "\uE8B7",
                FontSize = 16,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText40Brush"],
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(icon, 0);
            grid.Children.Add(icon);

            var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            var fileName = new TextBlock
            {
                Text = artifact.FileName,
                Style = (Style)Application.Current.Resources["OrbitalMonoConsole"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText75Brush"],
            };
            var meta = new TextBlock
            {
                Text = $"{artifact.Type} · {artifact.Size}",
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
            };
            textStack.Children.Add(fileName);
            textStack.Children.Add(meta);
            Grid.SetColumn(textStack, 2);
            grid.Children.Add(textStack);

            var time = new TextBlock
            {
                Text = artifact.Created.ToString("h:mm tt"),
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText30Brush"],
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(time, 3);
            grid.Children.Add(time);

            row.Child = grid;
            ArtifactsPanel.Children.Add(row);
        }
    }

    private void OnBuildTabTapped(object sender, TappedRoutedEventArgs e) => SetActiveTab(0);
    private void OnRunTabTapped(object sender, TappedRoutedEventArgs e) => SetActiveTab(1);
    private void OnArtifactsTabTapped(object sender, TappedRoutedEventArgs e) => SetActiveTab(2);

    private void SetActiveTab(int index)
    {
        TabBuild.Background = index == 0
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"]
            : _transparentBrush;
        TabRun.Background = index == 1
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"]
            : _transparentBrush;
        TabArtifacts.Background = index == 2
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"]
            : _transparentBrush;

        // Update tab text foreground
        ((TextBlock)TabBuild.Child).Foreground = index == 0
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText85Brush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText40Brush"];
        ((TextBlock)TabRun.Child).Foreground = index == 1
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText85Brush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText40Brush"];
        ((TextBlock)TabArtifacts.Child).Foreground = index == 2
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText85Brush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText40Brush"];

        BuildConsole.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
        RunConsole.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
        ArtifactsPanel.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
    }
}

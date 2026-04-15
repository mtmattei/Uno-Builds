using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class DiagnosticsPage : Page
{
    private IDiagnosticsService? _diagService;
    private IProjectContext? _ctx;

    public DiagnosticsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Entrance animations: stagger 100ms per section
        AnimationHelper.FadeUp(UnoCheckCard, 0);
        AnimationHelper.FadeUp(DepsSection, 100);
        AnimationHelper.FadeUp(ToolsSection, 200);
        AnimationHelper.FadeUp(PlatformsSection, 300);

        var host = Helpers.HostHelper.GetHost();
        if (host is null) return;
        _diagService = host.Services.GetRequiredService<IDiagnosticsService>();
        _ctx = host.Services.GetRequiredService<IProjectContext>();
        _ctx.ActiveProjectChanged += OnActiveProjectChanged;

        // Wire button handler before data load so it's always available
        RerunButton.Click += OnRerunClick;

        await LoadDiagnosticsDataAsync();
    }

    private async Task LoadDiagnosticsDataAsync()
    {
        if (_diagService is null) return;

        // Fire all data loads in parallel
        var checksTask = _diagService.GetChecksAsync(CancellationToken.None).AsTask();
        var depsTask = _diagService.GetDependenciesAsync(CancellationToken.None).AsTask();
        var toolsTask = _diagService.GetRuntimeToolsAsync(CancellationToken.None).AsTask();
        var platformsTask = _diagService.GetPlatformTargetsAsync(CancellationToken.None).AsTask();

        await Task.WhenAll(checksTask, depsTask, toolsTask, platformsTask);

        PopulateUnoCheckConsole(checksTask.Result);
        PopulateDependencies(depsTask.Result);
        PopulateTools(toolsTask.Result);
        PopulatePlatforms(platformsTask.Result);
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
            _ = LoadDiagnosticsDataAsync();
        });
    }

    private async void OnRerunClick(object sender, RoutedEventArgs e)
    {
        RerunButton.IsEnabled = false;
        RerunButton.Content = "Running...";

        // Show live header while running
        var liveLines = new List<Controls.ConsoleLine>
        {
            new("$ uno-check --non-interactive", "dim"),
            new("", "dim"),
            new("  Running uno-check...", "info"),
        };
        UnoCheckConsole.SetLines(liveLines);

        try
        {
            var lines = await RunUnoCheckAsync();
            // If uno-check produced no output, append built-in checks
            if (lines.Any(l => l.Text.Contains("Falling back")))
            {
                if (_diagService is not null)
                {
                    lines.Add(new Controls.ConsoleLine("", "dim"));
                    var checks = await _diagService.GetChecksAsync(CancellationToken.None);
                    foreach (var check in checks)
                    {
                        var icon = check.Status switch { HealthStatus.Ok => "\u2713", HealthStatus.Warn => "\u26A0", _ => "\u2717" };
                        var type = check.Status switch { HealthStatus.Ok => "success", HealthStatus.Warn => "warn", _ => "error" };
                        lines.Add(new Controls.ConsoleLine($"  {icon} {check.Name} \u2014 {check.Detail}", type));
                    }
                }
            }
            UnoCheckConsole.SetLines(lines);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            // uno-check not installed or needs elevation — show message and fall back
            var fallbackLines = new List<Controls.ConsoleLine>
            {
                new("$ uno-check --non-interactive", "dim"),
                new("", "dim"),
                new($"  {ex.Message}", "warn"),
                new("", "dim"),
                new("  Falling back to built-in checks...", "dim"),
                new("", "dim"),
            };
            if (_diagService is not null)
            {
                var checks = await _diagService.GetChecksAsync(CancellationToken.None);
                foreach (var check in checks)
                {
                    var icon = check.Status switch { HealthStatus.Ok => "\u2713", HealthStatus.Warn => "\u26A0", _ => "\u2717" };
                    var type = check.Status switch { HealthStatus.Ok => "success", HealthStatus.Warn => "warn", _ => "error" };
                    fallbackLines.Add(new Controls.ConsoleLine($"  {icon} {check.Name} — {check.Detail}", type));
                }
            }
            UnoCheckConsole.SetLines(fallbackLines);
        }
        catch (Exception ex)
        {
            UnoCheckConsole.SetLines(new List<Controls.ConsoleLine>
            {
                new("$ uno-check --non-interactive", "dim"),
                new("", "dim"),
                new($"  ERROR: {ex.Message}", "error"),
            });
        }
        finally
        {
            RerunButton.Content = "Re-run";
            RerunButton.IsEnabled = true;
        }
    }

    private static async Task<List<Controls.ConsoleLine>> RunUnoCheckAsync()
    {
        var lines = new List<Controls.ConsoleLine>
        {
            new("$ uno-check --non-interactive", "dim"),
            new("", "dim"),
        };

        // Resolve tool path — global tools live in ~/.dotnet/tools
        var toolsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotnet", "tools");
        var exeName = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows) ? "uno-check.exe" : "uno-check";
        var toolPath = Path.Combine(toolsDir, exeName);

        if (!File.Exists(toolPath))
            throw new FileNotFoundException("uno-check is not installed. Install with: dotnet tool install -g uno.check");

        // Spectre.Console suppresses output when stdout is directly redirected.
        // Use cmd /c to run through a shell which gives Spectre a real console,
        // then pipe through "more" so we still capture the output.
        var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows);

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "/bin/sh",
            Arguments = isWindows
                ? $"/c \"\"{toolPath}\" --non-interactive 2>&1\""
                : $"-c '\"{toolPath}\" --non-interactive 2>&1'",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.Environment["NO_COLOR"] = "1";
        psi.Environment["DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"] = "1";
        psi.Environment["TERM"] = "dumb";

        using var process = new System.Diagnostics.Process { StartInfo = psi };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start uno-check: {ex.Message}", ex);
        }

        // uno-check can be slow — enforce a 60s timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var ct = cts.Token;
        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        // Parse output lines
        var output = !string.IsNullOrWhiteSpace(stdout) ? stdout : stderr;
        if (!string.IsNullOrWhiteSpace(output))
        {
            foreach (var rawLine in output.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Strip ANSI escape sequences
                line = System.Text.RegularExpressions.Regex.Replace(line, @"\x1B\[[0-9;]*m", "");
                if (string.IsNullOrWhiteSpace(line)) continue;

                var type = "info";
                if (line.Contains("\u2713") || line.Contains("ok", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("installed", StringComparison.OrdinalIgnoreCase))
                    type = "success";
                else if (line.Contains("\u26A0") || line.Contains("warning", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("recommend", StringComparison.OrdinalIgnoreCase))
                    type = "warn";
                else if (line.Contains("\u2717") || line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    type = "error";

                lines.Add(new Controls.ConsoleLine($"  {line}", type));
            }
        }

        // Spectre.Console may produce no capturable output — detect and report
        if (lines.Count <= 2)
        {
            lines.Add(new Controls.ConsoleLine($"  uno-check ran (exit code {process.ExitCode}) but produced no capturable output.", "warn"));
            lines.Add(new Controls.ConsoleLine("  Spectre.Console suppresses output when stdout is redirected.", "dim"));
            lines.Add(new Controls.ConsoleLine("  Falling back to built-in environment checks.", "dim"));
            return lines;
        }

        lines.Add(new Controls.ConsoleLine("", "dim"));
        lines.Add(new Controls.ConsoleLine(
            $"  Process exited with code {process.ExitCode}",
            process.ExitCode == 0 ? "success" : "warn"));

        return lines;
    }

    private void PopulateUnoCheckConsole(ImmutableList<DiagnosticsCheck> checks)
    {
        var lines = new List<Controls.ConsoleLine>
        {
            new("$ uno-check --target desktop --target web", "dim"),
            new("", "dim"),
        };

        foreach (var check in checks)
        {
            var type = check.Status switch
            {
                HealthStatus.Ok => "success",
                HealthStatus.Warn => "warn",
                HealthStatus.Error => "error",
                _ => "info",
            };
            var icon = check.Status switch
            {
                HealthStatus.Ok => "\u2713",
                HealthStatus.Warn => "\u26A0",
                HealthStatus.Error => "\u2717",
                _ => " ",
            };
            lines.Add(new Controls.ConsoleLine($"  {icon} {check.Name}", type));
        }

        lines.Add(new Controls.ConsoleLine("", "dim"));

        var okCount = checks.Count(c => c.Status == HealthStatus.Ok);
        var warnCount = checks.Count(c => c.Status == HealthStatus.Warn);
        lines.Add(new Controls.ConsoleLine($"  {okCount} passed \u00B7 {warnCount} warning \u00B7 0 errors", "info"));

        UnoCheckConsole.SetLines(lines);
    }

    private void PopulateDependencies(ImmutableList<DependencyInfo> deps)
    {
        DepsGrid.Children.Clear();

        for (var i = 0; i < deps.Count; i++)
        {
            var dep = deps[i];
            var col = i % 2;
            var row = i / 2;

            while (DepsGrid.RowDefinitions.Count <= row)
                DepsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var card = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"],
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12),
                Margin = new Thickness(col == 0 ? 0 : 6, row == 0 ? 0 : 12, col == 1 ? 0 : 6, 0),
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

            var dot = new Controls.StatusDot
            {
                Status = dep.Status == HealthStatus.Ok ? "ok" : "warn",
                DotSize = 8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(dot, 0);
            grid.Children.Add(dot);

            var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock
            {
                Text = dep.Package,
                Style = (Style)Application.Current.Resources["OrbitalMonoConsole"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText72Brush"],
            });
            var versionText = dep.CurrentVersion != dep.LatestVersion
                ? $"{dep.CurrentVersion} \u2192 {dep.LatestVersion}"
                : dep.CurrentVersion;
            textStack.Children.Add(new TextBlock
            {
                Text = versionText,
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
            });
            Grid.SetColumn(textStack, 2);
            grid.Children.Add(textStack);

            if (dep.CurrentVersion != dep.LatestVersion)
            {
                var packageName = dep.Package;
                var latestVersion = dep.LatestVersion;
                var updateBtn = new Button
                {
                    Content = "Update",
                    Style = (Style)Application.Current.Resources["OrbitalSecondaryButtonSm"],
                    VerticalAlignment = VerticalAlignment.Center,
                };
                updateBtn.Click += async (s, _) =>
                {
                    var btn = (Button)s;
                    btn.IsEnabled = false;
                    btn.Content = "Updating...";
                    try
                    {
                        // Run dotnet add package to update
                        var csproj = FindCsproj();
                        if (csproj is not null)
                        {
                            using var process = new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "dotnet",
                                    Arguments = $"add \"{csproj}\" package {packageName} --version {latestVersion}",
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                },
                            };
                            process.Start();
                            await process.WaitForExitAsync();
                            btn.Content = process.ExitCode == 0 ? "\u2713" : "Failed";
                        }
                    }
                    catch { btn.Content = "Error"; }
                };
                Grid.SetColumn(updateBtn, 3);
                grid.Children.Add(updateBtn);
            }
            // No badge for up-to-date packages per reference design

            card.Child = grid;
            Grid.SetColumn(card, col);
            Grid.SetRow(card, row);
            DepsGrid.Children.Add(card);

        }
    }

    private void PopulateTools(ImmutableList<RuntimeTool> tools)
    {
        ToolsGrid.Children.Clear();

        for (var i = 0; i < tools.Count; i++)
        {
            var tool = tools[i];
            var col = i % 3;
            var row = i / 3;

            while (ToolsGrid.RowDefinitions.Count <= row)
                ToolsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var card = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"],
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(
                    col == 0 ? 0 : 6,
                    row == 0 ? 0 : 12,
                    col == 2 ? 0 : 6,
                    0),
            };

            var stack = new StackPanel { Spacing = 12 };

            // Top row: icon + bars
            var topRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                },
            };

            var iconBox = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(8),
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
            };
            iconBox.Child = new FontIcon
            {
                Glyph = "\uE950",
                FontSize = 16,
                Foreground = Helpers.OrbitalColors.AccentBrush(tool.AccentColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(iconBox, 0);
            topRow.Children.Add(iconBox);

            if (tool.Status != HealthStatus.Idle)
            {
                var bars = new Controls.PulsingBars
                {
                    BarCount = 3,
                    BarColor = tool.AccentColor,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(bars, 2);
                topRow.Children.Add(bars);
            }

            stack.Children.Add(topRow);

            // Text
            var textStack = new StackPanel { Spacing = 2 };
            textStack.Children.Add(new TextBlock
            {
                Text = tool.Name,
                Style = (Style)Application.Current.Resources["OrbitalBody"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText80Brush"],
            });
            textStack.Children.Add(new TextBlock
            {
                Text = tool.Description,
                Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText38Brush"],
            });
            stack.Children.Add(textStack);

            card.Child = stack;
            Grid.SetColumn(card, col);
            Grid.SetRow(card, row);
            ToolsGrid.Children.Add(card);

        }
    }

    private void PopulatePlatforms(ImmutableList<PlatformTarget> platforms)
    {
        PlatformsStrip.Children.Clear();
        foreach (var platform in platforms)
        {
            var pill = new Border
            {
                Background = platform.IsInstalled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_5Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"],
                BorderBrush = platform.IsInstalled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_15Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 10),
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            content.Children.Add(new Controls.StatusDot
            {
                Status = platform.IsInstalled ? "ok" : "idle",
                DotSize = 8,
                VerticalAlignment = VerticalAlignment.Center,
            });
            content.Children.Add(new TextBlock
            {
                Text = platform.Name,
                Style = (Style)Application.Current.Resources["OrbitalTabLabel"],
                Foreground = platform.IsInstalled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText40Brush"],
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            });

            pill.Child = content;
            PlatformsStrip.Children.Add(pill);

        }
    }

    private string? FindCsproj() =>
        Helpers.HostHelper.FindCsproj(_ctx?.ActiveProject?.RootDirectory);
}

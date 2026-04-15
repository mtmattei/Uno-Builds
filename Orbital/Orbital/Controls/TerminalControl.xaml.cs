using System.Diagnostics;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class TerminalControl : UserControl
{
    private Process? _shellProcess;
    private readonly List<string> _commandHistory = [];
    private int _historyIndex = -1;
    private Border? _currentOutputBubble;
    private StackPanel? _currentOutputLines;
    private bool _wired;
    private DispatcherTimer? _greetingTimer;
    private int _greetingIndex;

    private static readonly string[] Greetings =
    [
        "What are we building today?",
        "Ready to ship something great...",
        "Type a command to get started",
        "Build, test, deploy \u2014 all from here",
        "What's on the backlog?",
        "Let's squash some bugs",
        "Hot reload is standing by...",
        "Your workspace is ready",
        "dotnet build \u2014 dotnet run \u2014 dotnet ship",
        "Create something amazing",
        "Push pixels, not buttons",
        "One platform, every screen",
    ];

    public TerminalControl()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_wired)
        {
            InputBox.KeyDown += OnInputKeyDown;
            SendButton.Click += (_, _) => SubmitCommand();
            InputBox.GotFocus += (_, _) => StopGreetingCycle();
            _wired = true;
        }
        StartGreetingCycle();
        EnsureShellRunning();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopGreetingCycle();
        StopShell();
    }

    private void StartGreetingCycle()
    {
        _greetingIndex = Random.Shared.Next(Greetings.Length);
        InputBox.PlaceholderText = Greetings[_greetingIndex];

        _greetingTimer?.Stop();
        _greetingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _greetingTimer.Tick += (_, _) =>
        {
            _greetingIndex = (_greetingIndex + 1) % Greetings.Length;
            InputBox.PlaceholderText = Greetings[_greetingIndex];
        };
        _greetingTimer.Start();
    }

    private void StopGreetingCycle()
    {
        _greetingTimer?.Stop();
        _greetingTimer = null;
    }

    private void EnsureShellRunning()
    {
        if (_shellProcess is not null && !_shellProcess.HasExited) return;
        StartShell();
    }

    private void StartShell()
    {
        StopShell();
        try
        {
            var shell = GetShellPath();
            var workDir = GetWorkingDirectory();
            var psi = new ProcessStartInfo
            {
                FileName = shell.path,
                Arguments = shell.args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workDir,
            };
            psi.Environment["NO_COLOR"] = "1";
            psi.Environment["TERM"] = "dumb";

            _shellProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _shellProcess.OutputDataReceived += OnOutputReceived;
            _shellProcess.ErrorDataReceived += OnErrorReceived;
            _shellProcess.Exited += (_, _) => DispatcherQueue.TryEnqueue(() =>
                AddSystemBubble("Shell process ended. Type a command to restart."));

            _shellProcess.Start();
            _shellProcess.BeginOutputReadLine();
            _shellProcess.BeginErrorReadLine();

            ShellBadge.Text = Path.GetFileNameWithoutExtension(shell.path);
            AddSystemBubble($"Connected to {Path.GetFileName(shell.path)} \u00B7 ready");
        }
        catch (Exception ex)
        {
            AddSystemBubble($"Failed to start shell: {ex.Message}");
        }
    }

    private void StopShell()
    {
        if (_shellProcess is not null)
        {
            try
            {
                if (!_shellProcess.HasExited)
                    _shellProcess.Kill(entireProcessTree: true);
            }
            catch { }
            _shellProcess.Dispose();
            _shellProcess = null;
        }
    }

    private void OnInputKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            SubmitCommand();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Up)
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                InputBox.Text = _commandHistory[_historyIndex];
                InputBox.SelectionStart = InputBox.Text.Length;
            }
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Down)
        {
            if (_historyIndex < _commandHistory.Count - 1)
            {
                _historyIndex++;
                InputBox.Text = _commandHistory[_historyIndex];
                InputBox.SelectionStart = InputBox.Text.Length;
            }
            else
            {
                _historyIndex = _commandHistory.Count;
                InputBox.Text = "";
            }
            e.Handled = true;
        }
    }

    private void SubmitCommand()
    {
        var command = InputBox.Text?.Trim() ?? "";
        InputBox.Text = "";
        if (string.IsNullOrEmpty(command)) return;

        _commandHistory.Add(command);
        _historyIndex = _commandHistory.Count;

        if (command.Equals("clear", StringComparison.OrdinalIgnoreCase) ||
            command.Equals("cls", StringComparison.OrdinalIgnoreCase))
        {
            OutputPanel.Children.Clear();
            _currentOutputBubble = null;
            _currentOutputLines = null;
            return;
        }

        if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            StopShell();
            AddSystemBubble("Shell terminated.");
            return;
        }

        AddCommandBubble(command);

        _currentOutputLines = new StackPanel { Spacing = 1 };
        _currentOutputBubble = new Border
        {
            Background = (Brush)Application.Current.Resources["OrbitalSurface15Brush"],
            CornerRadius = new CornerRadius(12, 12, 12, 4),
            Padding = new Thickness(14, 10),
            MaxWidth = 600,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = _currentOutputLines,
        };
        OutputPanel.Children.Add(_currentOutputBubble);

        if (_shellProcess is null || _shellProcess.HasExited)
        {
            StartShell();
        }

        try
        {
            _shellProcess?.StandardInput.WriteLine(command);
            _shellProcess?.StandardInput.Flush();
        }
        catch (Exception ex)
        {
            AppendToCurrentBubble($"Error: {ex.Message}", "error");
        }

        SmoothScrollToBottom();
    }

    private void OnOutputReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null) return;
        DispatcherQueue.TryEnqueue(() =>
        {
            AppendToCurrentBubble(e.Data, "info");
            SmoothScrollToBottom();
        });
    }

    private void OnErrorReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is null) return;
        DispatcherQueue.TryEnqueue(() =>
        {
            AppendToCurrentBubble(e.Data, "error");
            SmoothScrollToBottom();
        });
    }

    private void AddCommandBubble(string command)
    {
        TrimOldBubbles();
        var bubble = new Border
        {
            Background = (Brush)Application.Current.Resources["OrbitalEmerald500_15Brush"],
            CornerRadius = new CornerRadius(12, 12, 4, 12),
            Padding = new Thickness(14, 8),
            MaxWidth = 500,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        content.Children.Add(new FontIcon
        {
            Glyph = "\uE756",
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["OrbitalEmerald400Brush"],
            VerticalAlignment = VerticalAlignment.Center,
        });
        content.Children.Add(new TextBlock
        {
            Text = command,
            FontFamily = (FontFamily)Application.Current.Resources["OrbitalMonoFont"],
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["OrbitalEmerald400Brush"],
            TextWrapping = TextWrapping.Wrap,
        });
        bubble.Child = content;
        OutputPanel.Children.Add(bubble);
    }

    private void AddSystemBubble(string text)
    {
        TrimOldBubbles();
        var bubble = new Border
        {
            Background = (Brush)Application.Current.Resources["OrbitalSurface2Brush"],
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4),
        };
        bubble.Child = new TextBlock
        {
            Text = text,
            FontFamily = (FontFamily)Application.Current.Resources["OrbitalMonoFont"],
            FontSize = 11,
            Foreground = (Brush)Application.Current.Resources["OrbitalText40Brush"],
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
        };
        OutputPanel.Children.Add(bubble);
        SmoothScrollToBottom();
    }

    private void AppendToCurrentBubble(string text, string type)
    {
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[0-9;]*[a-zA-Z]", "");

        if (_currentOutputLines is null)
        {
            _currentOutputLines = new StackPanel { Spacing = 1 };
            _currentOutputBubble = new Border
            {
                Background = (Brush)Application.Current.Resources["OrbitalSurface15Brush"],
                CornerRadius = new CornerRadius(12, 12, 12, 4),
                Padding = new Thickness(14, 10),
                MaxWidth = 600,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = _currentOutputLines,
            };
            OutputPanel.Children.Add(_currentOutputBubble);
        }

        var tb = new TextBlock
        {
            Text = text,
            FontFamily = (FontFamily)Application.Current.Resources["OrbitalMonoFont"],
            FontSize = 12,
            Foreground = GetBrush(type),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
        };
        _currentOutputLines.Children.Add(tb);

        while (_currentOutputLines.Children.Count > 100)
            _currentOutputLines.Children.RemoveAt(0);
    }

    private void TrimOldBubbles()
    {
        while (OutputPanel.Children.Count > 50)
            OutputPanel.Children.RemoveAt(0);
    }

    private void SmoothScrollToBottom()
    {
        // Defer to let layout update, then scroll
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            var target = OutputScroller.ScrollableHeight;
            OutputScroller.ChangeView(null, target, null, false);
        });
    }

    private static readonly SolidColorBrush _errorBrush = new(OrbitalColors.Error);
    private static readonly SolidColorBrush _warnBrush = new(OrbitalColors.Warn);
    private static readonly SolidColorBrush _dimBrush = new(OrbitalColors.Dim);
    private static readonly SolidColorBrush _infoBrush = new(OrbitalColors.Info);

    private static SolidColorBrush GetBrush(string type) => type switch
    {
        "error" => _errorBrush,
        "warn" => _warnBrush,
        "dim" => _dimBrush,
        _ => _infoBrush,
    };

    private static (string path, string args) GetShellPath()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
        {
            var pwshCore = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "PowerShell", "7", "pwsh.exe");
            if (File.Exists(pwshCore))
                return (pwshCore, "-NoLogo -NoProfile");

            var pwsh = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "WindowsPowerShell", "v1.0", "powershell.exe");
            if (File.Exists(pwsh))
                return (pwsh, "-NoLogo -NoProfile");

            return ("cmd.exe", "/Q");
        }

        if (File.Exists("/bin/bash"))
            return ("/bin/bash", "--norc --noprofile");
        return ("/bin/sh", "");
    }

    private static string GetWorkingDirectory()
    {
        try
        {
            var host = HostHelper.GetHost();
            if (host is not null)
            {
                var ctx = host.Services.GetRequiredService<IProjectContext>();
                if (ctx.ActiveProject is not null)
                    return ctx.ActiveProject.RootDirectory;
            }
        }
        catch { }

        return HostHelper.FindProjectRoot() ?? AppContext.BaseDirectory;
    }
}

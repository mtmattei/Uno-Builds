using System.ComponentModel;
using System.Text;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 8 (terminal) — Scaffold. Three internal stages:
///   1. <c>TerminalStage</c>  — shows the dotnet new command + Continue →
///   2. <c>SummaryStage</c>   — composition recap + Accept ✓ (or Back)
///   3. <c>DownloadStage</c>  — Accepted confirmation + Download bundle ↓
///
/// Stages are local to the canvas — no model state — because they're a
/// purely presentational walk-through, not part of the layer state machine.
/// </summary>
public sealed partial class ScaffoldTerminal : UserControl
{
    private INotifyPropertyChanged? _vm;

    public ScaffoldTerminal()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded            += (_, _)    => Detach();
    }

    private void Attach(object? dc)
    {
        Detach();
        if (dc is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }
        Render();
    }

    private void Detach()
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "AppName" or "AppDescription" or "Runtime"
            or "IsWebSelected" or "IsIOSSelected" or "IsAndroidSelected"
            or "IsWindowsSelected" or "IsMacOSSelected" or "IsDesktopSelected"
            or "LayerStates")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        CommandText.Text = BuildCommand();
        FillSummary();
    }

    private void FillSummary()
    {
        var dc = DataContext;
        if (dc is null) return;
        var name = (Composer.Views.MvuxValueReader.ReadRaw(dc, "AppName") as string) ?? string.Empty;
        var desc = (Composer.Views.MvuxValueReader.ReadRaw(dc, "AppDescription") as string) ?? string.Empty;
        SummaryAppName.Text     = string.IsNullOrWhiteSpace(name) ? "Composer app" : name;
        SummaryDescription.Text = string.IsNullOrWhiteSpace(desc) ? "—"            : desc;

        var platforms = CollectPlatforms(dc);
        var runtime   = (Composer.Views.MvuxValueReader.ReadRaw(dc, "Runtime") as string) ?? ".NET 10";
        var lockedCount = CountLocked(dc);

        SummaryFactsGrid.Children.Clear();
        AddFact(0, "PLATFORMS", platforms);
        AddFact(1, "RUNTIME",   runtime);
        AddFact(2, "LAYERS LOCKED", $"{lockedCount} of {Composer.Models.Layers.All.Length}");
        AddFact(3, "OUTPUT", "ZIP · 9 files");
    }

    private void AddFact(int row, string label, string value)
    {
        var l = new TextBlock
        {
            Text = label,
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 10,
            CharacterSpacing = 160,
            Foreground = AppBrushes.Ink3,
            Margin = new Thickness(0, 6, 8, 6),
        };
        Grid.SetRow(l, row);
        Grid.SetColumn(l, 0);
        SummaryFactsGrid.Children.Add(l);

        var v = new TextBlock
        {
            Text = value,
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 14,
            Foreground = AppBrushes.Ink,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 6),
        };
        Grid.SetRow(v, row);
        Grid.SetColumn(v, 1);
        SummaryFactsGrid.Children.Add(v);
    }

    private static int CountLocked(object dc)
    {
        var statesObj = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
        if (statesObj is not System.Collections.Generic.IDictionary<LayerKind, LayerState> dict)
            return 0;
        var n = 0;
        foreach (var kv in dict) if (kv.Value == LayerState.Locked) n++;
        return n;
    }

    private string BuildCommand()
    {
        var dc = DataContext;
        if (dc is null) return string.Empty;

        var name    = ((Composer.Views.MvuxValueReader.ReadRaw(dc, "AppName") as string) ?? "App").Replace(" ", "");
        var runtime = MapRuntime((Composer.Views.MvuxValueReader.ReadRaw(dc, "Runtime") as string) ?? ".NET 10");
        var platforms = MapPlatforms(CollectPlatforms(dc));

        var sb = new StringBuilder();
        sb.Append("dotnet new unoapp \\").AppendLine();
        sb.Append("  -n ").Append(string.IsNullOrWhiteSpace(name) ? "App" : name).Append(" \\").AppendLine();
        sb.Append("  --tfm ").Append(runtime).Append(" \\").AppendLine();
        sb.Append("  --platforms ").Append(platforms).Append(" \\").AppendLine();
        sb.Append("  --markup xaml \\").AppendLine();
        sb.Append("  --presentation mvux \\").AppendLine();
        sb.Append("  --theme material \\").AppendLine();
        sb.Append("  --features config,http,logging,nav,mvux");
        return sb.ToString();
    }

    private static string MapRuntime(string r) => r switch
    {
        ".NET 9"  => "net9.0",
        ".NET 10" => "net10.0",
        _         => "net10.0",
    };

    private static string CollectPlatforms(object dc)
    {
        var parts = new System.Collections.Generic.List<string>();
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsWebSelected")     as bool?) == true) parts.Add("Web");
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsIOSSelected")     as bool?) == true) parts.Add("iOS");
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsAndroidSelected") as bool?) == true) parts.Add("Android");
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsWindowsSelected") as bool?) == true) parts.Add("Windows");
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsMacOSSelected")   as bool?) == true) parts.Add("macOS");
        if ((Composer.Views.MvuxValueReader.ReadRaw(dc, "IsDesktopSelected") as bool?) == true) parts.Add("Desktop");
        return parts.Count == 0 ? "Web, iOS, Android" : string.Join(", ", parts);
    }

    private static string MapPlatforms(string platforms)
    {
        var parts = platforms.Split(new[] { ',', '·', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        var mapped = new System.Collections.Generic.List<string>();
        foreach (var raw in parts)
        {
            var p = raw.Trim().ToLowerInvariant();
            mapped.Add(p switch
            {
                "web"     => "wasm",
                "ios"     => "ios",
                "android" => "android",
                "windows" => "windows",
                "macos"   => "maccatalyst",
                "desktop" => "desktop",
                _         => p,
            });
        }
        return mapped.Count == 0 ? "wasm,ios,android" : string.Join(",", mapped);
    }

    // ─── Stage transitions ──────────────────────────────────────────────

    // StageEyebrow stays "scaffold.command" (filename, set in XAML). Only the
    // dot-prefixed StageBadge and right-side italic StageMeta change between
    // the three stages — matches prototype ScaffoldCanvas state-aware header.

    private void OnContinueToSummaryClick(object sender, RoutedEventArgs e)
    {
        TerminalStage.Visibility = Visibility.Collapsed;
        SummaryStage.Visibility  = Visibility.Visible;
        DownloadStage.Visibility = Visibility.Collapsed;
        StageBadge.Text   = "·  REVIEW";
        StageMeta.Text    = "compose summary · ready to confirm";
    }

    private void OnBackToTerminalClick(object sender, RoutedEventArgs e)
    {
        TerminalStage.Visibility = Visibility.Visible;
        SummaryStage.Visibility  = Visibility.Collapsed;
        DownloadStage.Visibility = Visibility.Collapsed;
        StageBadge.Text   = "·  READY";
        StageMeta.Text    = "9 files · ready to ship";
    }

    private void OnAcceptSummaryClick(object sender, RoutedEventArgs e)
    {
        TerminalStage.Visibility = Visibility.Collapsed;
        SummaryStage.Visibility  = Visibility.Collapsed;
        DownloadStage.Visibility = Visibility.Visible;
        StageBadge.Text   = "·  DOWNLOAD";
        StageMeta.Text    = "accepted · zip awaiting save";
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        var pkg = new DataPackage();
        pkg.SetText(CommandText.Text);
        Clipboard.SetContent(pkg);
    }

    private void OnDownloadClick(object sender, RoutedEventArgs e)
    {
        var page = FindParent<Page>(this);
        MvuxCommandInvoker.Invoke(page?.DataContext, "DownloadBundle");
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}

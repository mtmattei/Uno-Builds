using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 2 — Architecture. Renders module nodes + labeled edges on a Canvas.
/// Brief 02 keeps this read-only — edits flow through the composer prompt only.
///
/// Render pipeline is pooled per audit 2026-05-12 §3:
///   - Grid backdrop is built once on Loaded and never rebuilt.
///   - Modules + edges keep per-index slot pools so re-rendering reuses
///     existing <see cref="Border"/> / <see cref="Line"/> / <see cref="TextBlock"/>
///     instances instead of allocating fresh ones every PropertyChanged.
///   - Extra slots (when a render produces fewer modules/edges than before)
///     stay in the visual tree but flip to <see cref="Visibility.Collapsed"/>.
/// </summary>
public sealed partial class ArchitectureBlueprint : UserControl
{
    private INotifyPropertyChanged? _vm;

    // Pools — slot index = blueprint index. Grow-only; extras get hidden.
    private readonly List<ModuleSlot> _modulePool = new();
    private readonly List<EdgeSlot>   _edgePool   = new();
    private bool _gridBuilt;

    public ArchitectureBlueprint()
    {
        this.InitializeComponent();
        this.Loaded             += (_, _)    => EnsureGridBuilt();
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
        if (e.PropertyName == "Architecture")
            DispatcherQueue?.TryEnqueue(Render);
    }

    /// <summary>Lay down the static blueprint grid backdrop one time. 1px
    /// hairline lines at 50% opacity on 20dp cells per Brief 04 §3.</summary>
    private void EnsureGridBuilt()
    {
        if (_gridBuilt) return;
        const double Cell = 20;
        var w = BoardCanvas.Width;
        var h = BoardCanvas.Height;
        for (var x = 0d; x <= w; x += Cell)
        {
            BoardCanvas.Children.Add(new Line
            {
                X1 = x, Y1 = 0, X2 = x, Y2 = h,
                Stroke = AppBrushes.Hairline,
                StrokeThickness = 1,
                Opacity = 0.5,
            });
        }
        for (var y = 0d; y <= h; y += Cell)
        {
            BoardCanvas.Children.Add(new Line
            {
                X1 = 0, Y1 = y, X2 = w, Y2 = y,
                Stroke = AppBrushes.Hairline,
                StrokeThickness = 1,
                Opacity = 0.5,
            });
        }
        _gridBuilt = true;
    }

    private void Render()
    {
        // Fall back to Default if the bindable proxy hasn't subscribed the
        // IState yet (its initial provider only runs on first subscription —
        // a reflection read can return null before any binding lights up).
        var bp = ReadBlueprint() ?? Composer.Models.ArchitectureBlueprint.Default;

        EnsureGridBuilt();
        const double ModuleW = 180, ModuleH = 50;

        // ─── Edges (render below modules) ───────────────────────────────
        var modulesById = bp.Modules.ToDictionary(m => m.Id);
        for (var i = 0; i < bp.Edges.Length; i++)
        {
            var edge = bp.Edges[i];
            if (!modulesById.TryGetValue(edge.FromId, out var from) ||
                !modulesById.TryGetValue(edge.ToId,   out var to))
            {
                if (i < _edgePool.Count) _edgePool[i].SetVisible(false);
                continue;
            }

            var endpoints = ComputeEdgeEndpoints(from.Position, to.Position, ModuleW, ModuleH);
            var slot = EnsureEdgeSlot(i);
            slot.Apply(edge.Label, endpoints);
        }
        // Hide trailing edges from a previous (larger) render.
        for (var i = bp.Edges.Length; i < _edgePool.Count; i++)
            _edgePool[i].SetVisible(false);

        // ─── Modules (render above edges) ───────────────────────────────
        for (var i = 0; i < bp.Modules.Length; i++)
        {
            var m = bp.Modules[i];
            var surfaceBrush = ResolveBrush(m.ColorKey, surface: true);
            var inkBrush     = ResolveBrush(m.ColorKey, surface: false);
            var slot = EnsureModuleSlot(i);
            slot.Apply(m, surfaceBrush, inkBrush);
        }
        for (var i = bp.Modules.Length; i < _modulePool.Count; i++)
            _modulePool[i].SetVisible(false);

        MetaText.Text = $"·  {bp.Modules.Length} MODULES · {bp.Edges.Length} EDGES";

        // Light-themed solution tree appendix below the canvas card. Brief 04 §5.
        SolutionTreeBlock.Apply(
            fileLabel:   "SOLUTION_TREE.TXT",
            sourceLabel: "DERIVED FROM BLUEPRINT",
            code:        BuildSolutionTree(),
            language:    "tree");
    }

    private static (double X1, double Y1, double X2, double Y2) ComputeEdgeEndpoints(
        Windows.Foundation.Point from, Windows.Foundation.Point to, double moduleW, double moduleH)
    {
        var fromCx = from.X + moduleW / 2;
        var fromCy = from.Y + moduleH / 2;
        var toCx   = to.X   + moduleW / 2;
        var toCy   = to.Y   + moduleH / 2;

        var sameRow = System.Math.Abs(fromCy - toCy) < 1;
        var sameCol = System.Math.Abs(fromCx - toCx) < 1;
        if (sameRow)
            return fromCx < toCx
                ? (from.X + moduleW, fromCy, to.X,            toCy)
                : (from.X,           fromCy, to.X + moduleW,  toCy);
        if (sameCol)
            return fromCy < toCy
                ? (fromCx, from.Y + moduleH, toCx, to.Y)
                : (fromCx, from.Y,            toCx, to.Y + moduleH);
        // Diagonal — center-to-center as a fallback.
        return (fromCx, fromCy, toCx, toCy);
    }

    private EdgeSlot EnsureEdgeSlot(int index)
    {
        while (_edgePool.Count <= index)
        {
            var slot = EdgeSlot.Create();
            BoardCanvas.Children.Add(slot.Line);
            BoardCanvas.Children.Add(slot.LabelHost);
            _edgePool.Add(slot);
        }
        return _edgePool[index];
    }

    private ModuleSlot EnsureModuleSlot(int index)
    {
        while (_modulePool.Count <= index)
        {
            var slot = ModuleSlot.Create(this);
            BoardCanvas.Children.Add(slot.Border);
            _modulePool.Add(slot);
        }
        return _modulePool[index];
    }

    /// <summary>Synthesize the solution tree from the same DataContext data
    /// the model uses — falls back to a sensible placeholder when the
    /// AppName / platform flags aren't yet readable through reflection.</summary>
    private string BuildSolutionTree()
    {
        var dc = DataContext;
        if (dc is null) return string.Empty;
        // ReadRaw caches the GetProperty lookup per (proxy type, name).
        var name = (Composer.Views.MvuxValueReader.ReadRaw(dc, "AppName") as string ?? "").Trim();
        if (string.IsNullOrEmpty(name)) name = "ComposerApp";

        bool ReadBool(string prop) =>
            Composer.Views.MvuxValueReader.ReadRaw(dc, prop) is bool b && b;
        var web     = ReadBool("IsWebSelected");
        var android = ReadBool("IsAndroidSelected");
        var ios     = ReadBool("IsIOSSelected");
        var windows = ReadBool("IsWindowsSelected");
        var macOS   = ReadBool("IsMacOSSelected");
        var desktop = ReadBool("IsDesktopSelected");

        var heads = System.Collections.Immutable.ImmutableArray.CreateBuilder<string>();
        if (ios || android) heads.Add("Mobile");
        if (windows || macOS || desktop) heads.Add("Desktop");
        if (web) heads.Add("Wasm");

        return Composer.Models.LayerMarkdownTemplates.BuildSolutionTree(name, heads.ToImmutable());
    }

    private void OnRegenerateClick(object sender, RoutedEventArgs e)
    {
        var page = FindParent<Page>(this);
        Composer.Views.MvuxCommandInvoker.Invoke(
            page?.DataContext,
            "RegenerateArchitecture");
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }

    private static Brush ResolveBrush(string colorKey, bool surface) => colorKey switch
    {
        "indigo" => surface ? new SolidColorBrush(Color.FromArgb(0x10, 0x3D, 0x3D, 0xFF)) : AppBrushes.Indigo,
        "amber"  => surface ? new SolidColorBrush(Color.FromArgb(0x10, 0xD9, 0x77, 0x06)) : AppBrushes.Amber,
        "ink2"   => surface ? AppBrushes.Paper2 : AppBrushes.Ink2,
        _        => surface ? AppBrushes.Paper  : AppBrushes.Ink,
    };

    private Composer.Models.ArchitectureBlueprint? ReadBlueprint() =>
        Composer.Views.MvuxValueReader.Read<Composer.Models.ArchitectureBlueprint>(DataContext, "Architecture");

    // ─── Pool slot helpers ──────────────────────────────────────────────

    private sealed class EdgeSlot
    {
        public required Line Line { get; init; }
        public required Border LabelHost { get; init; }
        public required TextBlock LabelText { get; init; }

        public static EdgeSlot Create()
        {
            var line = new Line
            {
                Stroke = AppBrushes.Ink3,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 3, 3 },
            };
            var labelText = new TextBlock
            {
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 10,
                CharacterSpacing = 160,
                Foreground = AppBrushes.Ink3,
            };
            var labelHost = new Border
            {
                Background = AppBrushes.Paper2,
                Padding = new Thickness(4, 1, 4, 1),
                Child = labelText,
            };
            return new EdgeSlot { Line = line, LabelHost = labelHost, LabelText = labelText };
        }

        public void Apply(string label, (double X1, double Y1, double X2, double Y2) endpoints)
        {
            Line.X1 = endpoints.X1; Line.Y1 = endpoints.Y1;
            Line.X2 = endpoints.X2; Line.Y2 = endpoints.Y2;
            LabelText.Text = label;
            Canvas.SetLeft(LabelHost, ((endpoints.X1 + endpoints.X2) / 2) - 17);
            Canvas.SetTop (LabelHost, ((endpoints.Y1 + endpoints.Y2) / 2) - 8);
            SetVisible(true);
        }

        public void SetVisible(bool show)
        {
            var v = show ? Visibility.Visible : Visibility.Collapsed;
            Line.Visibility = v;
            LabelHost.Visibility = v;
        }
    }

    private sealed class ModuleSlot
    {
        public required Border    Border    { get; init; }
        public required TextBlock TitleText { get; init; }
        public required TextBlock DescText  { get; init; }
        public Brush?  CurrentSurface { get; set; }
        public Brush?  CurrentInk     { get; set; }
        public ModuleDef?           CurrentModule { get; set; }

        public static ModuleSlot Create(ArchitectureBlueprint owner)
        {
            var titleText = new TextBlock
            {
                FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
                FontSize = 11,
                CharacterSpacing = 160,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            };
            var descText = new TextBlock
            {
                FontFamily = (FontFamily)Application.Current.Resources["SerifLightItalicFontFamily"],
                FontSize = 10,
                Foreground = AppBrushes.Ink3,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 12, 0) };
            stack.Children.Add(titleText);
            stack.Children.Add(descText);

            var border = new Border
            {
                Width = 180,
                Height = 50,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = stack,
            };

            var slot = new ModuleSlot
            {
                Border = border,
                TitleText = titleText,
                DescText = descText,
            };

            // Hover handlers read from the slot's current module/brushes —
            // pooled slots get re-applied each render, so closures over the
            // slot avoid stale captures.
            border.PointerEntered += (_, _) =>
            {
                if (slot.CurrentModule is null) return;
                border.Background     = slot.CurrentInk;
                titleText.Foreground  = AppBrushes.Paper;
                descText.Foreground   = AppBrushes.Paper;
                owner.HoverModuleLabel.Text       = slot.CurrentModule.Label.ToUpperInvariant();
                owner.HoverModuleDescription.Text = slot.CurrentModule.Description;
                owner.HoverDetailPanel.Visibility = Visibility.Visible;
            };
            border.PointerExited += (_, _) =>
            {
                border.Background    = slot.CurrentSurface;
                titleText.Foreground = slot.CurrentInk;
                descText.Foreground  = AppBrushes.Ink3;
                owner.HoverDetailPanel.Visibility = Visibility.Collapsed;
            };
            return slot;
        }

        public void Apply(ModuleDef m, Brush surface, Brush ink)
        {
            CurrentModule  = m;
            CurrentSurface = surface;
            CurrentInk     = ink;
            Border.Background  = surface;
            Border.BorderBrush = ink;
            TitleText.Text       = m.Label.ToUpperInvariant();
            TitleText.Foreground = ink;
            DescText.Text        = m.Description;
            Canvas.SetLeft(Border, m.Position.X);
            Canvas.SetTop (Border, m.Position.Y);
            SetVisible(true);
        }

        public void SetVisible(bool show)
        {
            Border.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

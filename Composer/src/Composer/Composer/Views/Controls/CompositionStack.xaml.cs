using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Controls;

/// <summary>
/// Left rail (280px). Lists the eight layers with their per-layer state
/// reflected in left-border color and opacity. Active layer highlighted
/// amber; locked layers shown ink2 with ✓ glyph; future layers dimmed.
/// </summary>
public sealed partial class CompositionStack : UserControl
{
    private INotifyPropertyChanged? _vm;

    public CompositionStack()
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
        if (e.PropertyName is "ActiveIndex" or "LayerStates")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        var dc = DataContext;
        var (activeIndex, states) = ReadLayerSnapshot(dc);
        var rows = ImmutableArray.CreateBuilder<LayerRow>(Composer.Models.Layers.All.Length);
        for (var i = 0; i < Composer.Models.Layers.All.Length; i++)
        {
            var def = Composer.Models.Layers.All[i];
            var s = states.TryGetValue(def.Kind, out var v) ? v : LayerState.Clean;
            var isActive = i == activeIndex;
            var isLocked = s == LayerState.Locked;
            var dimmed   = !isActive && !isLocked && i > activeIndex;
            rows.Add(LayerRow.For(def, isActive, isLocked, dimmed));
        }
        if (LayerRows is { } repeater)
            repeater.ItemsSource = rows.MoveToImmutable();
    }

    private static (int index, IDictionary<LayerKind, LayerState> states) ReadLayerSnapshot(object? dc)
    {
        if (dc is null) return (0, new Dictionary<LayerKind, LayerState>());

        var t = dc.GetType();
        var idx = (t.GetProperty("ActiveIndex")?.GetValue(dc) as int?) ?? 0;

        var statesObj = t.GetProperty("LayerStates")?.GetValue(dc);
        if (statesObj is IDictionary<LayerKind, LayerState> dict)
            return (idx, dict);
        if (statesObj is IReadOnlyDictionary<LayerKind, LayerState> rdict)
        {
            var copy = new Dictionary<LayerKind, LayerState>();
            foreach (var kv in rdict) copy[kv.Key] = kv.Value;
            return (idx, copy);
        }
        return (idx, new Dictionary<LayerKind, LayerState>());
    }

    private void OnLayerRowClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is LayerRow row)
        {
            var page = FindParent<Page>(this);
            MvuxCommandInvoker.Invoke(page?.DataContext, "Jump", row.Index);
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : class
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }
}

/// <summary>Per-row projection bound by the rail. Brief 02 will derive a richer
/// summary from per-layer state (e.g., Intent's app type); brief 01 uses the
/// static <see cref="LayerDef.Hint"/> as the summary.</summary>
public sealed record LayerRow(
    int Index,
    string IndexLabel,
    string LabelUpper,
    string Hint,
    string Glyph,
    Brush LeftBorderBrush,
    double Opacity)
{
    public static LayerRow For(LayerDef def, bool isActive, bool isLocked, bool dimmed)
    {
        var glyph = isLocked ? "✓" : string.Empty;
        var border = isActive
            ? AppBrushes.Amber
            : isLocked
                ? AppBrushes.Ink2
                : AppBrushes.Transparent;
        var opacity = dimmed ? 0.42 : 1.0;
        return new LayerRow(
            Index:           def.Index,
            IndexLabel:      $"{(def.Index + 1):D2}",
            LabelUpper:      def.Label.ToUpperInvariant(),
            Hint:            def.Hint,
            Glyph:           glyph,
            LeftBorderBrush: border,
            Opacity:         opacity);
    }
}

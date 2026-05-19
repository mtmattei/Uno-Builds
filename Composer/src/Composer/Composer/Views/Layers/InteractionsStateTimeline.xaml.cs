using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Composer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Layers;

/// <summary>
/// Layer 5 — Interactions. Flow tabs across the top, six state pills, and a
/// description panel for the active state. Read-only in brief 02.
/// </summary>
public sealed partial class InteractionsStateTimeline : UserControl
{
    private INotifyPropertyChanged? _vm;
    private string _activeFlowId  = "create-job";
    private string _activeStateId = "default";

    public InteractionsStateTimeline()
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
        if (e.PropertyName is "Interactions" or "LayerStates")
            DispatcherQueue?.TryEnqueue(Render);
    }

    /// <summary>True when the Interactions layer is in `previewing` state.
    /// Reads the LayerStates dict via reflection.</summary>
    private bool IsPreviewing()
    {
        var dc = DataContext;
        if (dc is null) return false;
        var statesProp = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
        if (statesProp is null) return false;
        var dict = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState>>(statesProp);
        if (dict is null && statesProp is System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState> direct)
            dict = direct;
        if (dict is null) return false;
        return dict.TryGetValue(LayerKind.Interactions, out var s) && s == LayerState.Previewing;
    }

    private InteractionsMatrix? ReadProposedMatrix()
    {
        var raw = Composer.Views.MvuxValueReader.InvokeMethod(DataContext, "GetPreviewValues", LayerKind.Interactions);
        return raw as InteractionsMatrix
            ?? Composer.Views.MvuxValueReader.Unwrap<InteractionsMatrix>(raw);
    }

    private void Render()
    {
        var currentFlows = ReadFlows();
        var previewing   = IsPreviewing();
        var proposed     = previewing ? ReadProposedMatrix() : null;

        // In previewing state, the displayed pill timeline + detail panel
        // reflect the AI's proposed matrix; current values feed the diff
        // signals. In all other states, displayed == current.
        var flows = (proposed is { Flows.IsDefaultOrEmpty: false } pm)
            ? pm.Flows
            : currentFlows;

        FlowTabsRow.Children.Clear();
        StatePillsRow.Children.Clear();
        if (flows.IsDefaultOrEmpty) return;

        if (!flows.Any(f => f.Id == _activeFlowId))
            _activeFlowId = flows[0].Id;

        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];

        foreach (var flow in flows)
        {
            // Brief 06 §3 — non-active flows that have changes in their
            // proposed states get a small amber dot on the tab. The active
            // flow gets the dot too if any of its states changed.
            var hasDiff = previewing && FlowHasChanges(flow.Id, currentFlows, flows);
            var tab = MakeFlowTabButton(flow.Label.ToUpperInvariant(),
                                         isActive: flow.Id == _activeFlowId,
                                         showDiffDot: hasDiff);
            tab.Tag = flow.Id;
            tab.Click += (s, _) => { _activeFlowId = (string)((Button)s).Tag; _activeStateId = "default"; Render(); };
            FlowTabsRow.Children.Add(tab);
        }

        var active = flows.First(f => f.Id == _activeFlowId);
        MetaText.Text = $"·  {flows.Length} FLOWS · 6 STATES";

        for (var i = 0; i < active.States.Length; i++)
        {
            var st = active.States[i];
            var pill = MakePillButton(st.Label.ToUpperInvariant(), isActive: st.Id == _activeStateId, mono: true);
            pill.Tag = st.Id;
            pill.Click += (s, _) => { _activeStateId = (string)((Button)s).Tag; Render(); };
            StatePillsRow.Children.Add(pill);

            // 18×1 hairline divider between pills — gives the row its
            // timeline reading (states in canonical order) per brief 06 §2.
            if (i < active.States.Length - 1)
            {
                StatePillsRow.Children.Add(new Border
                {
                    Width  = 18,
                    Height = 1,
                    Background        = AppBrushes.Hairline,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
        }

        var activeState = active.States.FirstOrDefault(s => s.Id == _activeStateId)
                          ?? active.States[0];
        StateLabelText.Text = $"{activeState.Label.ToUpperInvariant()} STATE";
        StateDescriptionText.Text = string.IsNullOrWhiteSpace(activeState.Description)
            ? GenericStateTemplate(activeState.Id, active.Label)
            : activeState.Description;

        // Diff annotation in the detail panel — when previewing and the
        // active state's description changed, show "WAS: {old}" in amber
        // mono caps below the description (brief 06 §3).
        ApplyDescriptionDiff(previewing, activeState, currentFlows);
    }

    private void ApplyDescriptionDiff(bool previewing, StateDef activeState, ImmutableArray<InteractionFlow> currentFlows)
    {
        // Locate the optional WAS annotation control via x:FindName fallback —
        // we render it dynamically inside the detail panel host so we avoid
        // a per-canvas XAML edit just for this string.
        if (DetailPanelStack.Children.Count > 2)
            DetailPanelStack.Children.RemoveAt(2);

        if (!previewing) return;

        var currentFlow = currentFlows.FirstOrDefault(f => f.Id == _activeFlowId);
        if (currentFlow is null) return;
        var oldState = currentFlow.States.FirstOrDefault(s => s.Kind == activeState.Kind);
        if (oldState is null) return;
        if (string.Equals(oldState.Description, activeState.Description, System.StringComparison.Ordinal)) return;

        var monoFont = (FontFamily)Application.Current.Resources["MonoFontFamily"];
        DetailPanelStack.Children.Add(new TextBlock
        {
            Text = $"WAS: {oldState.Description}".ToUpperInvariant(),
            FontFamily = monoFont,
            FontSize = 9,
            CharacterSpacing = 180,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = AppBrushes.Amber,
            TextWrapping = TextWrapping.Wrap,
        });
    }

    /// <summary>True when any state of the named flow's proposed shape
    /// differs from the current shape in description text.</summary>
    private static bool FlowHasChanges(string flowId, ImmutableArray<InteractionFlow> current, ImmutableArray<InteractionFlow> proposed)
    {
        var oldFlow = current.FirstOrDefault(f => f.Id == flowId);
        var newFlow = proposed.FirstOrDefault(f => f.Id == flowId);
        if (oldFlow is null || newFlow is null) return false;
        for (var i = 0; i < newFlow.States.Length; i++)
        {
            var n = newFlow.States[i];
            var o = oldFlow.States.FirstOrDefault(s => s.Kind == n.Kind);
            if (o is null) return true;
            if (!string.Equals(o.Description, n.Description, System.StringComparison.Ordinal)) return true;
        }
        return false;
    }

    /// <summary>Templated description for when the AI seeding hasn't filled
    /// in a state's prose. Each state has a default reading rooted in the
    /// active flow's label so the user sees something meaningful even before
    /// (or in lieu of) AI seeding.</summary>
    private static string GenericStateTemplate(string stateId, string flowLabel) => stateId switch
    {
        "default" => $"Entry point for the {flowLabel.ToLowerInvariant()} flow — primary action visible, no prior state.",
        "loading" => $"Brief pause while {flowLabel.ToLowerInvariant()} data loads. Skeleton placeholders preferred over a spinner.",
        "empty"   => $"No {flowLabel.ToLowerInvariant()} content yet — surface the action that creates the first item.",
        "error"   => $"Something failed in the {flowLabel.ToLowerInvariant()} flow — explain what, offer a retry, keep prior input.",
        "success" => $"{flowLabel} confirmed — show what's next or return cleanly to the prior surface.",
        "offline" => $"Connection lost mid-{flowLabel.ToLowerInvariant()} — show what's cached, queue any pending writes.",
        _         => $"State for the {flowLabel.ToLowerInvariant()} flow.",
    };

    /// <summary>State pill — large rounded pill, mono caps, fits in the
    /// canvas timeline. Brief 06 §2.</summary>
    private Button MakePillButton(string label, bool isActive, bool mono = true)
    {
        var btn = new Button
        {
            Content = label,
            Padding = new Thickness(14, 8, 14, 8),
            BorderThickness = new Thickness(1),
            BorderBrush = isActive ? AppBrushes.Ink : AppBrushes.Hairline,
            Background = isActive ? AppBrushes.Ink : AppBrushes.Paper2,
            Foreground = isActive ? AppBrushes.Paper : AppBrushes.Ink2,
            CornerRadius = new CornerRadius(999),
            FontFamily = mono
                ? (FontFamily)Application.Current.Resources["MonoFontFamily"]
                : (FontFamily)Application.Current.Resources["SerifFontFamily"],
            FontSize = 10,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            CharacterSpacing = mono ? 140 : 0,
        };
        return btn;
    }

    /// <summary>Flow tab — compact rectangular pill that lives in the header
    /// action slot. Smaller than the state pills. Brief 06 §2.
    /// In `previewing` state, tabs whose flow has staged changes get a 4×4
    /// amber dot to the right of the label (brief 06 §3).</summary>
    private Button MakeFlowTabButton(string label, bool isActive, bool showDiffDot = false)
    {
        var content = label as object;
        if (showDiffDot)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
            stack.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
            stack.Children.Add(new Microsoft.UI.Xaml.Shapes.Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = AppBrushes.Amber,
                VerticalAlignment = VerticalAlignment.Center,
            });
            content = stack;
        }

        return new Button
        {
            Content = content,
            Padding = new Thickness(8, 4, 8, 4),
            BorderThickness = new Thickness(1),
            BorderBrush = isActive ? AppBrushes.Ink : AppBrushes.Hairline,
            Background  = isActive ? AppBrushes.Ink : AppBrushes.Paper,
            Foreground  = isActive ? AppBrushes.Paper : AppBrushes.Ink2,
            CornerRadius = new CornerRadius(4),
            FontFamily = (FontFamily)Application.Current.Resources["MonoFontFamily"],
            FontSize = 9,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            CharacterSpacing = 140,
        };
    }

    private ImmutableArray<InteractionFlow> ReadFlows()
    {
        var v = Composer.Views.MvuxValueReader.ReadRaw(DataContext, "Interactions");
        if (v is null) return InteractionsMatrix.Default.Flows;

        // Direct hit — bound to the matrix record.
        if (v is InteractionsMatrix mx && !mx.Flows.IsDefaultOrEmpty) return mx.Flows;

        // MVUX wraps the matrix in a BindableInteractionsMatrixViewModel — pull
        // its Flows property out, then unwrap any inner element bindables.
        var unwrappedMatrix = Composer.Views.MvuxValueReader.Unwrap<InteractionsMatrix>(v);
        if (unwrappedMatrix is { } um && !um.Flows.IsDefaultOrEmpty) return um.Flows;

        // Legacy / direct array shape (kept for safety during the migration).
        if (v is ImmutableArray<InteractionFlow> arr && !arr.IsDefaultOrEmpty) return arr;

        if (v is System.Collections.IEnumerable enumerable)
        {
            var list = new System.Collections.Generic.List<InteractionFlow>();
            foreach (var item in enumerable)
            {
                if (item is InteractionFlow flow) { list.Add(flow); continue; }
                var unwrapped = Composer.Views.MvuxValueReader.Unwrap<InteractionFlow>(item);
                if (unwrapped is not null) list.Add(unwrapped);
            }
            if (list.Count > 0) return list.ToImmutableArray();
        }

        return InteractionsMatrix.Default.Flows;
    }
}

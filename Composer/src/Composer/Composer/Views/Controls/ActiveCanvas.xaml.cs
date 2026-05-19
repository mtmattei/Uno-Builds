using System;
using System.Collections.Generic;
using System.ComponentModel;
using Composer.Models;
using Composer.Views.Layers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace Composer.Views.Controls;

/// <summary>
/// Center column. Picks the right per-layer canvas (brief 02), stacks
/// locked-context cards above (brief 01 §4), and routes the composer footer
/// state to the layer's current LayerState. Brief 03 layers on the progress
/// hairline at the top and the future-preview card stack below the canvas
/// slot at progressively decreasing opacity.
/// </summary>
public sealed partial class ActiveCanvas : UserControl
{
    // M6: ActiveCanvas no longer forwards a Header DP — the inner
    // ActiveLayerHeader binds its six DPs directly to ShellModel's
    // ActiveLayer* feeds (in ActiveCanvas.xaml). Shell.xaml passes nothing
    // header-related to ActiveCanvas.

    private INotifyPropertyChanged? _vm;

    public ActiveCanvas()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded            += (_, _)    => Detach();
        // Progress is now self-managing inside ProgressIndicator.
    }

    private void Attach(object? dc)
    {
        Detach();
        if (dc is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }
        SyncSlot();
        SyncFooter();
        SyncLockedStack();
        SyncFutureStack();
        SyncFocusedFirst();
    }

    private void Detach()
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "ActiveIndex" or "ActiveLayer")
        {
            DispatcherQueue?.TryEnqueue(SyncSlot);
            DispatcherQueue?.TryEnqueue(SyncFutureStack);
        }
        if (e.PropertyName is "ActiveLayerHeader" or "ActiveLayerLayerState" or "LayerStates")
            DispatcherQueue?.TryEnqueue(SyncFooter);
        if (e.PropertyName is "ActiveLayer" or "LayerStates" or "Architecture" or "UX" or "Design" or "Data" or "Implementation")
            DispatcherQueue?.TryEnqueue(SyncLockedStack);
        if (e.PropertyName is "RailsVisible" or "IsFocusedFirst" or "LayerStates")
        {
            DispatcherQueue?.TryEnqueue(SyncFutureStack);
            DispatcherQueue?.TryEnqueue(SyncFocusedFirst);
        }
    }

    /// <summary>Read RailsVisible / IsFocusedFirst off the bound model and
    /// animate the canvas chrome between focused-first dimensions
    /// (max-width 720, padding-top 64) and rails-open dimensions
    /// (max-width 880, padding-top 32). Per design brief §2.1 / §2.2.</summary>
    private bool _focusedFirstApplied = true;

    private void SyncFocusedFirst()
    {
        var dc = DataContext;
        if (dc is null) return;
        // Reads go through MvuxValueReader.ReadRaw — caches the first-level
        // GetProperty lookup so repeated property-change handlers don't pay
        // reflection cost. The bool? cast unwraps an unboxed value; if the
        // feed is wrapped, fall back to IsFocusedFirst.
        var rv = Composer.Views.MvuxValueReader.ReadRaw(dc, "RailsVisible") as bool?;
        if (rv is null)
        {
            var ff = Composer.Views.MvuxValueReader.ReadRaw(dc, "IsFocusedFirst") as bool?;
            if (ff.HasValue) rv = !ff.Value;
        }
        var railsVisible = rv ?? false;
        var focused = !railsVisible;
        if (focused == _focusedFirstApplied) return;
        _focusedFirstApplied = focused;

        var targetMin    = focused ? 640d : 820d;
        var targetMax    = focused ? 720d : 880d;
        var targetTopPad = focused ? 64d  : 28d;

        if (!Composer.Views.MotionPreferences.AnimationsEnabled)
        {
            CanvasColumn.MinWidth = targetMin;
            CanvasColumn.MaxWidth = targetMax;
            CanvasScrollHost.Padding = new Thickness(32, targetTopPad, 32, 40);
            return;
        }

        // 480ms ease-out-quint per brief — matches the rail reveal rhythm
        // so canvas + rails animate together. The Storyboard + animations
        // are pooled per control so we don't allocate fresh objects every
        // toggle (audit 2026-05-12 §7).
        if (_focusedFirstStoryboard is null)
        {
            _focusedFirstStoryboard = new Storyboard();
            _focusedFirstMinAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(480),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(_focusedFirstMinAnim, CanvasColumn);
            Storyboard.SetTargetProperty(_focusedFirstMinAnim, "MinWidth");
            _focusedFirstStoryboard.Children.Add(_focusedFirstMinAnim);

            _focusedFirstMaxAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(480),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(_focusedFirstMaxAnim, CanvasColumn);
            Storyboard.SetTargetProperty(_focusedFirstMaxAnim, "MaxWidth");
            _focusedFirstStoryboard.Children.Add(_focusedFirstMaxAnim);
        }

        _focusedFirstStoryboard.Stop();
        _focusedFirstMinAnim!.To = targetMin;
        _focusedFirstMaxAnim!.To = targetMax;
        _focusedFirstStoryboard.Begin();

        // Padding doesn't animate cleanly via DoubleAnimation on Skia; snap it.
        CanvasScrollHost.Padding = new Thickness(32, targetTopPad, 32, 40);
    }

    // Pooled storyboard + animations for SyncFocusedFirst — built lazily on
    // first run, then reused with new To values per audit 2026-05-12 §7.
    private Storyboard? _focusedFirstStoryboard;
    private DoubleAnimation? _focusedFirstMinAnim;
    private DoubleAnimation? _focusedFirstMaxAnim;

    // OnHeaderChanged removed in M6 — the header now binds its own DPs directly
    // from XAML. SyncFooter / SyncProgress are still triggered from
    // OnVmPropertyChanged when the relevant feeds change.

    // NavigateToActiveLayer removed — region-based navigation reverted to
    // inline canvas hosting via SyncSlot. Pages + RouteMap stay registered;
    // a future pass will wire navigation properly.


    private void SyncFooter()
    {
        var state = ReadActiveLayerState();
        FooterRegion.State = state;

        var dc = DataContext;
        if (dc is null) return;
        var prompt = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActivePrompt") as string) ?? string.Empty;
        FooterRegion.Prompt = prompt;

        // Scaffold is terminal — its own canvas owns the Download bundle
        // action. Hiding the standard composer footer here so the user
        // can't get stuck clicking "Continue →" with nowhere to go.
        var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
        var clamped = System.Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
        var activeKind = Composer.Models.Layers.All[clamped].Kind;
        FooterRegion.Visibility = activeKind == LayerKind.Scaffold
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    /// <summary>Read the active layer's state from the bound dictionary on
    /// ComposerViewModel. Walks <c>LayerStates</c> at the active index;
    /// falls back to <c>Clean</c> when anything along the path is missing.
    /// Reflection lookups go through MvuxValueReader.ReadRaw for caching.</summary>
    private LayerState ReadActiveLayerState()
    {
        var dc = DataContext;
        if (dc is null) return LayerState.Clean;
        var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
        var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
        var kind = Composer.Models.Layers.All[clamped].Kind;
        var statesRaw = Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates");
        var states = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState>>(statesRaw);
        if (states is null && statesRaw is System.Collections.Immutable.IImmutableDictionary<LayerKind, LayerState> direct)
            states = direct;
        return states is not null && states.TryGetValue(kind, out var s) ? s : LayerState.Clean;
    }

    // SyncProgress removed in M4-step-C — the extracted ProgressIndicator
    // control manages its own animation off the Fraction DP. Label and
    // Counter bind directly to ComposerModel.ProgressLabel/ProgressCounter.

    private void SyncFutureStack()
    {
        FutureStack.Children.Clear();
        var dc = DataContext;
        if (dc is null) return;

        var railsVisible = (Composer.Views.MvuxValueReader.ReadRaw(dc, "RailsVisible") as bool?) ?? false;
        if (!railsVisible) return; // Brief §2.4: future cards only render when rails are visible.

        var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
        var all = Composer.Models.Layers.All;
        var animations = Composer.Views.MotionPreferences.AnimationsEnabled;

        for (var i = idx + 1; i < all.Length; i++)
        {
            var distance = i - idx;
            // Brief §2.4 — opacity = max(0.05, 0.40 - distance * 0.08).
            //   Distance 1 → 0.32  Distance 4 → 0.08
            //   Distance 2 → 0.24  Distance 5+ → 0.05
            //   Distance 3 → 0.16
            var targetOpacity = Math.Max(0.05, 0.40 - (distance * 0.08));
            var card = new FuturePreviewCard
            {
                LayerLabel = all[i].Label.ToUpperInvariant(),
                Hint       = all[i].Hint,
                Opacity    = animations ? 0 : targetOpacity,
            };
            FutureStack.Children.Add(card);

            if (animations)
            {
                // 480ms QuinticEase EaseOut — matches the rail-reveal rhythm
                // so the cards fade up alongside the workspace opening.
                var sb = new Storyboard();
                var anim = new DoubleAnimation
                {
                    To = targetOpacity,
                    Duration = TimeSpan.FromMilliseconds(480),
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                };
                Storyboard.SetTarget(anim, card);
                Storyboard.SetTargetProperty(anim, "Opacity");
                sb.Children.Add(anim);
                sb.Begin();
            }
        }
    }

    private void SyncLockedStack()
    {
        LockedStack.Children.Clear();
        var dc = DataContext;
        if (dc is null) return;

        if (Composer.Views.MvuxValueReader.ReadRaw(dc, "LayerStates") is not IDictionary<LayerKind, LayerState> states)
            return;

        // Read DefaultExpandedKinds — the two most recently locked layers
        // stay expanded; older ones collapse by default. Per v11 brief §10.
        var defaultExpanded = ReadDefaultExpandedKinds(dc);

        foreach (var def in Composer.Models.Layers.All)
        {
            if (!states.TryGetValue(def.Kind, out var s) || s != LayerState.Locked) continue;
            var (summary, facts) = LockedSummaryFor(def.Kind, dc);
            var card = new LockedContextCard
            {
                LayerKind  = def.Kind,
                Summary    = summary,
                Facts      = facts,
                IsExpanded = defaultExpanded?.Contains(def.Kind) ?? true,
            };
            LockedStack.Children.Add(card);
        }
    }

    private static System.Collections.Generic.HashSet<LayerKind>? ReadDefaultExpandedKinds(object dc)
    {
        var raw = Composer.Views.MvuxValueReader.ReadRaw(dc, "DefaultExpandedKinds");
        if (raw is null) return null;
        if (raw is System.Collections.Immutable.IImmutableSet<LayerKind> set)
            return new System.Collections.Generic.HashSet<LayerKind>(set);
        var unwrapped = Composer.Views.MvuxValueReader.Unwrap<System.Collections.Immutable.IImmutableSet<LayerKind>>(raw);
        return unwrapped is null ? null : new System.Collections.Generic.HashSet<LayerKind>(unwrapped);
    }

    /// <summary>Read the active layer's <see cref="LayerKind"/> off the
    /// bound model and place the matching canvas into CanvasSlot. Direct
    /// hosting — the Region-based navigation didn't successfully mount
    /// pages, so canvases get instantiated here per Brief 02. The Pages
    /// + RouteMap stay as future-friendly scaffolding.</summary>
    private void SyncSlot()
    {
        var dc = DataContext;
        if (dc is null) return;
        var idx = (Composer.Views.MvuxValueReader.ReadRaw(dc, "ActiveIndex") as int?) ?? 0;
        var clamped = Math.Clamp(idx, 0, Composer.Models.Layers.All.Length - 1);
        var kind = Composer.Models.Layers.All[clamped].Kind;
        var canvas = CreateCanvas(kind);
        if (canvas is FrameworkElement fe)
            fe.DataContext = dc;
        CanvasSlot.Content = canvas;
    }

    private static UserControl CreateCanvas(LayerKind kind) => kind switch
    {
        LayerKind.Intent          => new IntentCard(),
        LayerKind.UX              => new UXFlowStrip(),
        LayerKind.Architecture    => new Composer.Views.Layers.ArchitectureBlueprint(),
        LayerKind.DesignSystem    => new DesignTokenGrid(),
        LayerKind.Interactions    => new InteractionsStateTimeline(),
        LayerKind.Data            => new DataContractGrid(),
        LayerKind.Implementation  => new ImplementationPhaseGrid(),
        LayerKind.Scaffold        => new ScaffoldTerminal(),
        _ => new IntentCard(),
    };

    private static (string Summary, IList<KeyValuePair<string, string>> Facts) LockedSummaryFor(LayerKind kind, object dc)
    {
        var t = dc.GetType();
        switch (kind)
        {
            case LayerKind.Intent:
                if (t.GetProperty("Intent")?.GetValue(dc) is Composer.Models.IntentValues iv)
                {
                    var summary = $"\"{iv.AppType}, for {iv.PrimaryUser.ToLowerInvariant()}. {iv.Workflow}.\"";
                    return (summary,
                        new List<KeyValuePair<string, string>>
                        {
                            new("App type",     iv.AppType),
                            new("Primary user", iv.PrimaryUser),
                            new("Workflow",     iv.Workflow),
                            new("Platforms",    iv.Platforms),
                        });
                }
                break;
            case LayerKind.Architecture:
                if (t.GetProperty("Architecture")?.GetValue(dc) is Composer.Models.ArchitectureBlueprint bp)
                {
                    var pattern = bp.Modules.Any(m => m.Id == "mvux") ? "MVUX"
                                : bp.Modules.Any(m => m.Id == "mvvm") ? "MVVM"
                                : "Custom";
                    var hasStorage = bp.Modules.Any(m => m.Id == "storage");
                    var persistence = hasStorage ? "Offline-first" : "Server-only";
                    return ($"\"{LayerMarkdownTemplates.DeriveArchitectureSummary(bp)}\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Modules",     bp.Modules.Length.ToString()),
                            new("Connections", bp.Edges.Length.ToString()),
                            new("Pattern",     pattern),
                            new("Persistence", persistence),
                        });
                }
                break;
            case LayerKind.UX:
                if (t.GetProperty("UX")?.GetValue(dc) is UXFlow flow)
                {
                    return ($"\"{flow.Screens.Length}-screen {flow.Name.ToLowerInvariant()} flow.\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Screens",       flow.Screens.Length.ToString()),
                            new("Primary flow",  flow.Name),
                            new("Empty states",  "4"),
                            new("Error states",  "2"),
                        });
                }
                break;
            case LayerKind.DesignSystem:
                if (t.GetProperty("Design")?.GetValue(dc) is DesignTokens d)
                {
                    var action = $"#{d.Action.R:X2}{d.Action.G:X2}{d.Action.B:X2}";
                    return ($"\"{d.BodyFont} body, action {action}, Material rhythm.\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Body",       d.BodyFont),
                            new("Action",     action),
                            new("Type scale", "4 levels"),
                            new("Spacing",    "4px grid"),
                        });
                }
                break;
            case LayerKind.Interactions:
                if (UnwrapInteractionsFlows(t.GetProperty("Interactions")?.GetValue(dc)) is { } flows)
                {
                    var allStates    = flows.SelectMany(f => f.States).ToList();
                    var hasOffline   = allStates.Any(s => s.Kind == StateKind.Offline
                                                          && !string.IsNullOrWhiteSpace(s.Description)
                                                          && !s.Description.Contains("not applicable", System.StringComparison.OrdinalIgnoreCase));
                    var hasPerm      = allStates.Any(s =>
                        s.Description.Contains("permission", System.StringComparison.OrdinalIgnoreCase)
                        || s.Description.Contains("camera", System.StringComparison.OrdinalIgnoreCase)
                        || s.Description.Contains("location", System.StringComparison.OrdinalIgnoreCase)
                        || s.Description.Contains("notification", System.StringComparison.OrdinalIgnoreCase));
                    var offlineDesc  = hasOffline ? "offline-first throughout" : "online-only";
                    return ($"\"Six states across {flows.Length} flows; {offlineDesc}.\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Flows",             flows.Length.ToString()),
                            new("States/flow",       "6"),
                            new("Offline",           hasOffline ? "Yes" : "No"),
                            new("Permission states", hasPerm    ? "Yes" : "No"),
                        });
                }
                break;
            case LayerKind.Data:
                if (t.GetProperty("Data")?.GetValue(dc) is DataContracts dc2)
                {
                    var fieldCount = 0;
                    foreach (var ent in dc2.Entities) fieldCount += ent.Fields.Length;
                    return ($"\"{dc2.Entities.Length} entities with explicit nullability.\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Entities",    dc2.Entities.Length.ToString()),
                            new("Fields",      fieldCount.ToString()),
                            new("Records",     dc2.Entities.Length.ToString()),
                            new("Collections", "—"),
                        });
                }
                break;
            case LayerKind.Implementation:
                if (t.GetProperty("Implementation")?.GetValue(dc) is BuildPlan plan)
                {
                    return ($"\"{plan.Phases.Length} phases, scaffold → polish, with explicit dependencies.\"",
                        new List<KeyValuePair<string, string>>
                        {
                            new("Phases",       plan.Phases.Length.ToString()),
                            new("Acceptance",   "Per phase"),
                            new("Verification", "Per phase"),
                            new("Order",        "Linear"),
                        });
                }
                break;
        }
        return (Composer.Models.Layers.Get(kind).Hint, new List<KeyValuePair<string, string>>());
    }

    /// <summary>Unwrap the Interactions state value into its flows array,
    /// regardless of whether the binding gave us the raw record, the matrix
    /// wrapper, an MVUX bindable proxy, or the legacy direct array shape.</summary>
    private static System.Collections.Immutable.ImmutableArray<InteractionFlow>? UnwrapInteractionsFlows(object? value)
    {
        if (value is null) return null;

        if (value is InteractionsMatrix mx && !mx.Flows.IsDefaultOrEmpty)
            return mx.Flows;

        var unwrappedMatrix = Composer.Views.MvuxValueReader.Unwrap<InteractionsMatrix>(value);
        if (unwrappedMatrix is { } um && !um.Flows.IsDefaultOrEmpty)
            return um.Flows;

        if (value is System.Collections.Immutable.ImmutableArray<InteractionFlow> arr && !arr.IsDefaultOrEmpty)
            return arr;

        return null;
    }
}

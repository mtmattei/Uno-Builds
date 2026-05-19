using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Top-level orchestrator for the nine-layer composition. Owns cross-cutting
/// state (active index, per-layer state machine, locked set) and derives the
/// shell-level feeds — progress hairline, rail visibility, file rail status,
/// active-layer header projection.
///
/// Per <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §7.4.
///
/// M2a slice: ShellModel owns cross-cutting state. Per-layer canonical state
/// (Stack, Intent, UX, …) still lives on <see cref="ComposerModel"/> until
/// the per-layer models materialize in M2b. State-machine commands that mutate
/// per-layer values stay on ComposerModel; <see cref="Revisit"/> moves here
/// because it only touches the layer state map + active index.
/// </summary>
public partial record ShellModel
{
    // ─── Cross-cutting IState owners ────────────────────────────────────

    /// <summary>Index of the active layer (0–8). Initial = 0 (Stack).</summary>
    public IState<int> ActiveIndex => State<int>.Value(this, () => 0);

    /// <summary>Per-layer state machine values. Layers start Clean.</summary>
    public IState<IImmutableDictionary<LayerKind, LayerState>> LayerStates =>
        State<IImmutableDictionary<LayerKind, LayerState>>.Value(this, AllClean);

    private static IImmutableDictionary<LayerKind, LayerState> AllClean() =>
        Layers.All.ToImmutableDictionary(l => l.Kind, _ => LayerState.Clean);

    // ─── Derived shell-level feeds ──────────────────────────────────────

    /// <summary>Current LayerDef — clamped to a valid index.</summary>
    public IFeed<LayerDef> ActiveLayer =>
        ActiveIndex.Select(i => Layers.All[System.Math.Clamp(i, 0, Layers.All.Length - 1)]);

    /// <summary>Locked layers, regardless of order.</summary>
    public IFeed<IImmutableSet<LayerKind>> LockedLayers =>
        LayerStates.Select(d => d
            .Where(kv => kv.Value == LayerState.Locked)
            .Select(kv => kv.Key)
            .ToImmutableHashSet() as IImmutableSet<LayerKind>);

    /// <summary>Default-expanded locked cards — the two most-recently-locked
    /// layers stay expanded; older locks collapse. Mirrors the prototype's
    /// <c>lockedCards</c> <c>useMemo</c> (jsx 2653–2680).</summary>
    public IFeed<IImmutableSet<LayerKind>> DefaultExpandedKinds =>
        LayerStates.Select(d =>
        {
            var locked = d.Where(kv => kv.Value == LayerState.Locked)
                          .Select(kv => kv.Key)
                          .OrderByDescending(k => Layers.IndexOf(k))
                          .Take(2)
                          .ToImmutableHashSet();
            return (IImmutableSet<LayerKind>)locked;
        });

    /// <summary>Per-layer file rail status — Writing for the active layer,
    /// Drafted for any locked layer, Planned otherwise.</summary>
    public IFeed<IImmutableDictionary<LayerKind, FileStatus>> FileStatuses =>
        Feed.Combine(ActiveLayer, LockedLayers).Select(t =>
        {
            var (active, locked) = t;
            var b = ImmutableDictionary.CreateBuilder<LayerKind, FileStatus>();
            foreach (var l in Layers.All)
            {
                b[l.Kind] = locked.Contains(l.Kind) ? FileStatus.Drafted
                          : l.Kind == active.Kind   ? FileStatus.Writing
                          : FileStatus.Planned;
            }
            return (IImmutableDictionary<LayerKind, FileStatus>)b.ToImmutable();
        });

    /// <summary>Legacy projection — kept so the (about-to-be-deprecated)
    /// <c>LayerHeaderModel</c> consumers compile while M6 lands. Will be
    /// dropped once nothing reads <c>ComposerModel.ActiveLayerHeader</c>.</summary>
    public IFeed<LayerHeaderModel> ActiveLayerHeader =>
        Feed.Combine(ActiveLayer, LayerStates).Select(t =>
        {
            var (def, states) = t;
            var idx = (def.Index + 1).ToString("D2");
            return new LayerHeaderModel(
                LayerLabel: $"{idx} · {def.Label.ToUpperInvariant()}",
                Title:      def.Title,
                Subtitle:   def.Subtitle,
                State:      states.TryGetValue(def.Kind, out var s) ? s : LayerState.Clean);
        });

    // ─── Per-DP feeds for the new ActiveLayerHeader 6-DP shape (M6).
    //     Per docs/ARCHITECTURE-BRIEF-from-scratch.md §9/§12.1 each layer's
    //     header is built from these six discrete bindings. Surfaced here on
    //     ShellModel (single-page Shell) so ActiveCanvas can bind them; when
    //     the per-page header migration lands these move onto each layer
    //     model and are bound directly from the Page.

    /// <summary>Zero-based index of the active layer. 0 = Stack, 8 = Scaffold.</summary>
    public IFeed<int> ActiveLayerIndex => ActiveLayer.Select(d => d.Index);

    /// <summary>Eyebrow label — "01 · STACK" / "02 · INTENT" / …</summary>
    public IFeed<string> ActiveLayerLabel =>
        ActiveLayer.Select(d => $"{(d.Index + 1):D2} · {d.Label.ToUpperInvariant()}");

    /// <summary>Per-layer state machine value for the active layer.</summary>
    public IFeed<LayerState> ActiveLayerLayerState =>
        Feed.Combine(ActiveLayer, LayerStates).Select(t =>
            t.Item2.TryGetValue(t.Item1.Kind, out var s) ? s : LayerState.Clean);

    /// <summary>Italic ↳ recap above the eyebrow. Null for Stack (cold-launch
    /// surface, no prior layer). Per brief §9 every other layer has one.</summary>
    public IFeed<string?> ActiveLayerRecap =>
        ActiveLayer.Select(d => Recaps.TryGetValue(d.Kind, out var r) ? r : null);

    /// <summary>"What are we building on?" / "What are we building?" / …</summary>
    public IFeed<string> ActiveLayerTitle => ActiveLayer.Select(d => d.Title);

    /// <summary>"Pattern, markup, renderer, navigation, theme, platforms…" / …</summary>
    public IFeed<string> ActiveLayerSubtitle => ActiveLayer.Select(d => d.Subtitle);

    /// <summary>Static per-layer recap copy. Stack has no recap (it's the
    /// first surface); subsequent layers reflect what just locked.</summary>
    private static readonly System.Collections.Generic.IReadOnlyDictionary<LayerKind, string> Recaps =
        new System.Collections.Generic.Dictionary<LayerKind, string>
        {
            // Intent is layer 0 (cold-launch) — no recap.
            [LayerKind.UX]             = "We've named what we're building — now let's trace how someone uses it.",
            [LayerKind.Architecture]   = "With the user's path mapped — let's figure out the shape underneath.",
            [LayerKind.DesignSystem]   = "Modules in place — let's give the surface a feel.",
            [LayerKind.Interactions]   = "The design's settled — now every state of every flow.",
            [LayerKind.Data]           = "How it feels is decided — let's pin down the shapes underneath.",
            [LayerKind.Implementation] = "The pieces are named. Time to phase how they get built.",
            [LayerKind.Scaffold]       = "Every layer locked. Here's the runnable starting point.",
        };

    /// <summary>Per-layer lead question shown between the COMPOSER eyebrow
    /// and the textarea. Verbatim from prototype JSX / brief §9.x.</summary>
    public IFeed<string> ActiveLeadQuestion =>
        ActiveLayer.Select(d => LeadQuestions.TryGetValue(d.Kind, out var q) ? q : string.Empty);

    private static readonly System.Collections.Generic.IReadOnlyDictionary<LayerKind, string> LeadQuestions =
        new System.Collections.Generic.Dictionary<LayerKind, string>
        {
            [LayerKind.Intent]         = "If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?",
            [LayerKind.UX]             = "Drag-to-reorder schedule, or list-with-time-pickers? Stay on screen after dispatch, or return to dashboard?",
            [LayerKind.Architecture]   = "Two open questions before locking — does job logic live in a service, or stay inside State (MVUX)? And do technicians authenticate, or is access role-less?",
            [LayerKind.DesignSystem]   = "The palette is low-chroma for outdoor visibility. Want a brand override on Action, or stay with amber?",
            [LayerKind.Interactions]   = "Offline state — queue silently, or always show a sync-pending banner?",
            [LayerKind.Data]           = "Audit fields on every entity, or just the ones that change? Nullable Schedule for unscheduled jobs, or required with a stub date?",
            [LayerKind.Implementation] = "Front-load tests with phase 1, or thread them through every phase? Defer offline cache to a later phase, or stand it up next to the contracts?",
            [LayerKind.Scaffold]       = "Bundle and quit, or download the prompt-context.md and walk through it?",
        };

    /// <summary>Italic hint shown to the right of the Continue / Lock-and-continue
    /// button. Default "accepting the recommendation" for Clean state — per
    /// prototype, this hint reframes the click as deference to the suggested
    /// values rather than a destructive lock.</summary>
    public IFeed<string> ActivePrimaryHint =>
        ActiveLayerLayerState.Select(s => s switch
        {
            LayerState.Clean      => "accepting the recommendation",
            LayerState.Dirty      => "with your edits",
            LayerState.Previewing => "the AI's pass",
            _                     => string.Empty,
        });

    /// <summary>True once at least one layer has locked — gates the Reset
    /// button on AppTitleRow. Per brief §7.1 / audit §1.2 the button only
    /// appears after a lock so the cold-launch surface stays uncluttered.</summary>
    public IFeed<bool> HasLockedLayers =>
        LockedLayers.Select(l => l.Count > 0);

    /// <summary>"01 / 09" counter shown alongside the progress hairline.</summary>
    public IFeed<string> ProgressCounter =>
        ActiveIndex.Select(i => $"{(i + 1):D2} / {Layers.All.Length:D2}");

    /// <summary>"STACK" / "INTENT" / … — eyebrow label for the active layer
    /// alongside the progress counter. Brief 03 §4.</summary>
    public IFeed<string> ProgressLabel =>
        ActiveLayer.Select(d => d.Label.ToUpperInvariant());

    /// <summary>Fraction in [0..1] for the amber progress segment width.</summary>
    public IFeed<double> ProgressFraction =>
        ActiveIndex.Select(i => (i + 1) / (double)Layers.All.Length);

    /// <summary>True once at least one layer has locked OR the user has
    /// advanced past Stack (layer 0). Matches the prototype's
    /// <c>railsVisible = lockedIds.size &gt; 0 || activeIndex &gt; 0</c>
    /// (jsx line 2562).</summary>
    public IFeed<bool> RailsVisible =>
        Feed.Combine(LockedLayers, ActiveIndex)
            .Select(t => t.Item1.Count > 0 || t.Item2 > 0);

    /// <summary>Inverse of <see cref="RailsVisible"/>. Canvases read this
    /// to apply focused-first chrome before any layer has locked.</summary>
    public IFeed<bool> IsFocusedFirst =>
        RailsVisible.Select(v => !v);

    // ─── Commands ────────────────────────────────────────────────────────

    /// <summary>Click a Composition Stack row or a locked-card "Revisit" link.
    /// Setting state back to Clean preserves canonical values — revisit is
    /// non-destructive per brief 01 §4.</summary>
    public async ValueTask Jump(int index, CancellationToken ct = default)
    {
        if (index < 0 || index >= Layers.All.Length) return;
        await ActiveIndex.UpdateAsync(_ => index, ct);
    }

    public async ValueTask Revisit(LayerKind kind, CancellationToken ct = default)
    {
        var idx = Layers.IndexOf(kind);
        await ActiveIndex.UpdateAsync(_ => idx, ct);
        await SetLayerStateInternal(kind, LayerState.Clean, ct);
    }

    /// <summary>Internal — invoked by <see cref="ComposerModel"/>'s state-machine
    /// commands so per-layer setters that mutate canonical state can flip
    /// the layer's state value without owning <see cref="LayerStates"/>
    /// directly. Public because the partial record cannot expose internal
    /// helpers across files.</summary>
    public async ValueTask SetLayerStateInternal(LayerKind kind, LayerState s, CancellationToken ct = default)
    {
        await LayerStates.UpdateAsync(d => d?.SetItem(kind, s) ?? AllClean(), ct);
    }

    /// <summary>Advance ActiveIndex by one if not at the terminal layer.
    /// Invoked by <see cref="ComposerModel.LockAndContinue"/> /
    /// <see cref="ComposerModel.AcceptAndLock"/> after the layer transitions
    /// to Locked.</summary>
    public async ValueTask AdvanceIfPossible(CancellationToken ct = default)
    {
        var i = await ActiveIndex;
        if (i < Layers.All.Length - 1)
            await ActiveIndex.UpdateAsync(_ => i + 1, ct);
    }
}

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Composer.Services;
using Microsoft.Extensions.Options;
using Uno.Extensions.Reactive;

namespace Composer.Models;

/// <summary>
/// Top-level MVUX model for the context-engine composer. Owns the eight-layer
/// composition: per-layer canonical values (<see cref="Intent"/>,
/// <see cref="Architecture"/>, …), per-layer state (<see cref="LayerStates"/>),
/// per-layer prompt drafts, and rollback snapshots / preview values. The
/// <see cref="Stack"/> field is the static stack-preferences default used by
/// the bundle markdown emitter; no user-facing canvas mutates it.
///
/// See <c>docs/AUDIT-prototype-vs-current.md</c> for the canonical layer
/// order and the Shell + per-page decomposition pending in a later pass.
/// </summary>
public partial record ComposerModel(
    Composer.Models.Presentation.ShellModel Shell,
    Composer.Models.Presentation.IntentModel IntentLayer,
    Composer.Models.Presentation.UXModel UXLayer,
    Composer.Models.Presentation.ArchitectureModel ArchitectureLayer,
    Composer.Models.Presentation.DesignModel DesignLayer,
    Composer.Models.Presentation.InteractionsModel InteractionsLayer,
    Composer.Models.Presentation.DataModel DataLayer,
    Composer.Models.Presentation.ImplementationModel ImplementationLayer,
    IBundleExporter Exporter,
    IUnoSdkVersionService SdkVersionService,
    ILayerPreviewService PreviewService,
    IContextDeriver ContextDeriver,
    IOptions<AnthropicConfig> AnthropicOptions)
{
    private const string DefaultRuntime = ".NET 10";
    private const string UnoSdkFallback = "6.5.29";

    private AnthropicConfig Cfg => AnthropicOptions?.Value ?? new AnthropicConfig();

    // ─── Live Uno.Sdk version chip ────────────────────────────────────────
    private Task<string>? _latestUnoSdkTask;

    public IFeed<string> LatestUnoSdk => Feed.Async(async ct =>
    {
        if (_latestUnoSdkTask is null)
        {
            Interlocked.CompareExchange(
                ref _latestUnoSdkTask,
                LoadLatestUnoSdkAsync(),
                null);
        }
        return await _latestUnoSdkTask!.WaitAsync(ct).ConfigureAwait(false);
    });

    private async Task<string> LoadLatestUnoSdkAsync()
    {
        try
        {
            var live = await SdkVersionService.GetLatestStableAsync().ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(live) ? UnoSdkFallback : live;
        }
        catch { return UnoSdkFallback; }
    }

    // ─── Project metadata (filled later by Intent canvas + AppTitleRow) ──
    public IState<string> AppName        => State<string>.Value(this, () => "");
    public IState<string> AppDescription => State<string>.Value(this, () => "");
    public IState<string> Runtime        => State<string>.Value(this, () => DefaultRuntime);

    public IState<bool> IsWebSelected     => State<bool>.Value(this, () => false);
    public IState<bool> IsAndroidSelected => State<bool>.Value(this, () => false);
    public IState<bool> IsIOSSelected     => State<bool>.Value(this, () => false);
    public IState<bool> IsWindowsSelected => State<bool>.Value(this, () => false);
    public IState<bool> IsMacOSSelected   => State<bool>.Value(this, () => false);
    public IState<bool> IsDesktopSelected => State<bool>.Value(this, () => false);

    public IFeed<bool> IsRuntimeNet10 => Runtime.Select(r => r == ".NET 10");
    public IFeed<bool> IsRuntimeNet9  => Runtime.Select(r => r == ".NET 9");

    public IFeed<string> AppNameCharCounter =>
        AppName.Select(n => $"{(n ?? string.Empty).Length} / 64");

    public IFeed<string> NamespacePreview =>
        AppName.Select(n => $"namespace: Company.{SanitizeIdentifier(n)}");

    /// <summary>Number of reference screenshots attached to the project. UX +
    /// DesignSystem canvases use this to surface a "vision input" indicator
    /// so the user knows the AI saw their references.</summary>
    public IState<int> ReferenceScreenshotCount => State<int>.Value(this, () => 0);

    /// <summary>Screenshot file paths attached to the project — read by the
    /// DesignTokenGrid canvas to render its thumbnail strip.</summary>
    public IState<ImmutableArray<string>> ReferenceScreenshotPaths =>
        State<ImmutableArray<string>>.Value(this, () => ImmutableArray<string>.Empty);

    // ─── Cross-cutting state — owned by ShellModel, re-exposed here for
    //     binding compatibility with the current single-page Shell. M2a slice
    //     per docs/MIGRATION-PLAN.md — bindings flip in M3 when pages bind
    //     to ShellModel directly via Region Navigation.

    public IState<int> ActiveIndex => Shell.ActiveIndex;

    public IFeed<LayerDef> ActiveLayer => Shell.ActiveLayer;

    public IState<IImmutableDictionary<LayerKind, LayerState>> LayerStates => Shell.LayerStates;

    public IFeed<IImmutableSet<LayerKind>> LockedLayers => Shell.LockedLayers;

    /// <summary>Files Rail status per layer.</summary>
    public IFeed<IImmutableDictionary<LayerKind, FileStatus>> FileStatuses => Shell.FileStatuses;

    /// <summary>Legacy single-Header projection. Kept while M6 callers transition.</summary>
    public IFeed<LayerHeaderModel> ActiveLayerHeader => Shell.ActiveLayerHeader;

    // Six discrete header feeds per brief §12.1 — ActiveLayerHeader binds these.
    public IFeed<int>         ActiveLayerIndex      => Shell.ActiveLayerIndex;
    public IFeed<string>      ActiveLayerLabel      => Shell.ActiveLayerLabel;
    public IFeed<LayerState>  ActiveLayerLayerState => Shell.ActiveLayerLayerState;
    // ActiveLayerRecap moved below — reactive via DerivedContext.
    public IFeed<string>      ActiveLayerTitle      => Shell.ActiveLayerTitle;
    public IFeed<string>      ActiveLayerSubtitle   => Shell.ActiveLayerSubtitle;

    /// <summary>Live project name — derives from <c>Intent.AppType</c> so
    /// AppTitleRow updates as the user edits the Intent canvas. Empty /
    /// whitespace falls back to "Untitled" in the consuming control. Per
    /// brief §7.1 and audit §1.4 — replaces the deleted standalone AppName state.</summary>
    public IFeed<string> ProjectName =>
        Intent.Select(i => i?.AppType ?? string.Empty);

    /// <summary>Gates the Reset button on AppTitleRow.</summary>
    public IFeed<bool> HasLockedLayers => Shell.HasLockedLayers;

    /// <summary>Lead question shown above the textarea in ComposerFooter.
    /// Reactive over the active layer + Intent values through
    /// <see cref="IContextDeriver"/> — re-derives entity-aware phrasings
    /// when the user changes AppType from the field-service archetype to
    /// their own project (e.g., job → habit, technician → runner). Per
    /// brief §11.2.</summary>
    public IFeed<string> ActiveLeadQuestion =>
        Feed.Combine(Shell.ActiveLayer, IntentLayer.Values).Select(t =>
        {
            var (def, intent) = t;
            var ctx = ContextDeriver.Derive(intent);
            return BuildLeadQuestion(def.Kind, ctx);
        });

    /// <summary>Italic ↳ recap line above the active layer's eyebrow.
    /// Reactive in case downstream wording needs context. Stack returns null
    /// implicitly (no entry → empty string → row hides).</summary>
    public IFeed<string?> ActiveLayerRecap =>
        Feed.Combine(Shell.ActiveLayer, IntentLayer.Values).Select(t =>
        {
            var (def, intent) = t;
            var ctx = ContextDeriver.Derive(intent);
            return BuildRecap(def.Kind, ctx);
        });

    private static string BuildLeadQuestion(LayerKind kind, DerivedContext ctx) => kind switch
    {
        LayerKind.Intent =>
            "If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?",
        LayerKind.UX =>
            $"How does the primary flow read — drag-to-reorder, or list with pickers? Stay on screen after each {ctx.EntityNoun}, or return to dashboard?",
        LayerKind.Architecture =>
            $"Two open questions before locking — does {ctx.EntityNoun} logic live in a service, or stay inside State (MVUX)? And do {ctx.UserNoun} authenticate, or is access role-less?",
        LayerKind.DesignSystem =>
            "The palette is low-chroma for screen-reading clarity. Want a brand override on Action, or stay with amber?",
        LayerKind.Interactions =>
            $"Offline state for a queued {ctx.EntityNoun} — silent until the user opens the screen, or always show a sync-pending banner?",
        LayerKind.Data =>
            $"Audit fields on every {ctx.EntityNoun}, or just the ones that change? Nullable Schedule for unscheduled rows, or required with a stub date?",
        LayerKind.Implementation =>
            $"Front-load tests with phase 1, or thread them through every phase? Stand up the {ctx.EntityPlural} contract first, or the {ctx.UserNoun}-facing screens?",
        LayerKind.Scaffold =>
            "Bundle and quit, or download the prompt-context.md and walk through it?",
        _ => string.Empty,
    };

    /// <summary>Body copy for the IntentCard "WHY THIS INTENT" annotation.
    /// Templates the user's workflow + derived entity / user nouns so the
    /// rationale stays accurate to what they actually typed. Per audit
    /// 2026-05-12 — addresses missing contextualization in static XAML.</summary>
    public IFeed<string> WhyThisIntentBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            var workflow = string.IsNullOrWhiteSpace(intent?.Workflow)
                ? "the primary workflow"
                : intent!.Workflow.Trim();
            var appType = string.IsNullOrWhiteSpace(intent?.AppType) ? "This" : $"\"{intent!.AppType}\"";
            return $"{appType} is the workflow we're naming — {workflow}. " +
                   $"The agent now has a canonical noun ({ctx.EntityTitle}) and a {ctx.UserNoun} surface to build against from §06 with confidence.";
        });

    /// <summary>Body copy for the IntentCard "AGENT PROMPT" annotation.
    /// Lists the verbatim domain terms the agent should preserve.</summary>
    public IFeed<string> AgentPromptBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"Treat this as the canonical app description. Future layers should reference these terms verbatim — " +
                   $"{ctx.EntityTitle}, {Capitalize(ctx.UserNoun)} — not synonyms. Keep the workflow phrasing intact when generating code paths.";
        });

    // ─── Per-canvas annotation bodies — same reactive pattern as Intent.
    //     Each derives from IntentLayer.Values through IContextDeriver so
    //     the WhyThis / AgentPrompt copy references the user's app rather
    //     than the field-service defaults. Per audit 2026-05-12.

    public IFeed<string> WhyThisFlowBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"A linear primary path is the spine — branches and edge cases hang off it. " +
                   $"Naming the core screens gives the agent concrete entry points so {ctx.UserNoun} can move through {ctx.EntityPlural} predictably.";
        });

    public IFeed<string> AgentPromptFlowBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"Treat each screen as a navigation region. Generate one Page per screen and one route entry per arrow. " +
                   $"Use {ctx.EntityTitle} as the canonical noun on detail screens; do not collapse adjacent screens into a single Page unless the brief says so.";
        });

    public IFeed<string> WhyThisArchitectureBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            var persistence = ctx.IsOfflineFirst
                ? "offline-first state with a local cache"
                : "server-first state with optimistic updates";
            return $"MVUX gives both views identical reactive feeds — no MVVM ceremony, no Rx plumbing. " +
                   $"Region-based Navigation lets Shell and TabBar animate independently, and the {ctx.EntityNoun} service stays single-source-of-truth with {persistence}.";
        });

    public IFeed<string> AgentPromptArchitectureBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"When generating XAML, use uen:Region.Attached for navigation surfaces and bind ItemsSource to feeds (IFeed<{ctx.EntityTitle}>). " +
                   $"Never construct ViewModels in code-behind.";
        });

    public IFeed<string> WhyThisDesignBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            var rationale = ctx.IsMobileFirst
                ? "Mobile screens in mixed light mean low chroma, high contrast."
                : "Long sessions and dense lists mean low chroma, restrained accent.";
            return $"{rationale} Action is the only saturated hue and is reserved for the next thing a {ctx.UserNoun.TrimEnd('s')} should do — never decoration.";
        });

    public IFeed<string> AgentPromptDesignBody =>
        IntentLayer.Values.Select(_ =>
            "Apply tokens via ThemeResource — never hex. Reference SurfaceBrush, OnSurfaceBrush, SecondaryBrush. " +
            "Light and Dark theme dictionaries let the same XAML render correctly in both themes — no per-theme branching in markup.");

    public IFeed<string> WhyThisInteractionsBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            var offline = ctx.IsOfflineFirst
                ? "Offline-first means six states aren't optional"
                : "Six states keep the matrix complete";
            return $"{offline} — every flow's surface degrades to a queued / offline read at minimum so a {ctx.UserNoun.TrimEnd('s')} is never staring at a blank screen. The contract is by name so generated VSM XAML can be verified.";
        });

    public IFeed<string> AgentPromptInteractionsBody =>
        IntentLayer.Values.Select(_ =>
            "Every screen must implement all six states. Use VisualStateManager groups named exactly: Default, Loading, Empty, Error, Success, Offline. " +
            "Bind to a single IState<string> via utu:VisualStateManager.States — no procedural VisualStateManager.GoToState in code-behind.");

    public IFeed<string> WhyThisDataBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"Concrete {ctx.EntityPlural} — not abstractions. Each field is named, typed, and explicit about nullability. " +
                   $"The agent will scaffold C# records and JSON contracts from these without inventing schema for {ctx.EntityTitle}.";
        });

    public IFeed<string> AgentPromptDataBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"Generate immutable C# records with init-only setters for {ctx.EntityTitle} and its related types. " +
                   $"Nullable reference types enabled — non-nullable means non-nullable. Use System.Collections.Immutable for collection fields.";
        });

    public IFeed<string> WhyThisPlanBody =>
        IntentLayer.Values.Select(intent =>
        {
            var ctx = ContextDeriver.Derive(intent);
            return $"Six phases, each verifiable on its own. Dependencies are explicit so the agent never starts a phase whose inputs aren't locked. " +
                   $"The order minimises rework: foundation → {ctx.EntityNoun} contracts → {ctx.UserNoun}-facing surface → polish.";
        });

    public IFeed<string> AgentPromptPlanBody =>
        IntentLayer.Values.Select(_ =>
            "Work phases strictly in order. Do not start phase N+1 until phase N's verification command passes. Each phase commits before the next begins.");

    private static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string? BuildRecap(LayerKind kind, DerivedContext ctx) => kind switch
    {
        LayerKind.Intent         => null, // cold-launch surface; no prior layer
        LayerKind.UX             => $"We've named what we're building — now let's trace how {ctx.UserNoun} move through it.",
        LayerKind.Architecture   => $"With the {ctx.UserNoun}' path mapped — let's figure out the shape underneath.",
        LayerKind.DesignSystem   => "Modules in place — let's give the surface a feel.",
        LayerKind.Interactions   => "The design's settled — now every state of every flow.",
        LayerKind.Data           => $"How it feels is decided — let's pin down the shape of a {ctx.EntityNoun}.",
        LayerKind.Implementation => "The pieces are named. Time to phase how they get built.",
        LayerKind.Scaffold       => "Every layer locked. Here's the runnable starting point.",
        _                         => null,
    };

    /// <summary>Italic hint shown next to the Continue / Lock-and-continue button.</summary>
    public IFeed<string> ActivePrimaryHint => Shell.ActivePrimaryHint;

    public IFeed<string> ProgressCounter  => Shell.ProgressCounter;
    public IFeed<double> ProgressFraction => Shell.ProgressFraction;
    public IFeed<string> ProgressLabel    => Shell.ProgressLabel;
    public IFeed<bool>   RailsVisible    => Shell.RailsVisible;
    public IFeed<bool>   IsFocusedFirst  => Shell.IsFocusedFirst;

    // ─── Per-layer canonical state — owned by the per-layer models in
    //     Models/Presentation/{Layer}Model.cs. ComposerModel re-exposes
    //     them as pass-throughs so canvas reflection
    //     (MvuxValueReader.Read<T>(DataContext, "<name>")) keeps working
    //     until canvases bind to the layer models directly in M2b-2.

    /// <summary>Static stack defaults baked into bundle markdown output via
    /// <c>LayerMarkdownTemplates.BuildStackPreferences</c>. No user-facing
    /// canvas mutates this — the architecture brief's Stack-at-0 design is
    /// superseded by the prototype. Lives here as a constant projection so
    /// existing consumers compile unchanged.</summary>
    public IState<StackPreferences>      Stack          => State<StackPreferences>.Value(this, () => StackPreferences.Default);
    public IState<IntentValues>          Intent         => IntentLayer.Values;
    public IState<ArchitectureBlueprint> Architecture   => ArchitectureLayer.Blueprint;
    public IState<UXFlow>                UX             => UXLayer.Flow;
    public IState<DesignTokens>          Design         => DesignLayer.Tokens;
    public IState<DataContracts>         Data           => DataLayer.Contracts;
    public IState<BuildPlan>             Implementation => ImplementationLayer.Plan;
    public IState<InteractionsMatrix>    Interactions   => InteractionsLayer.Matrix;

    /// <summary>Which flow tab is selected on the Interactions canvas.</summary>
    public IState<string> ActiveFlowId => InteractionsLayer.ActiveFlowId;

    /// <summary>Hovered module on the Architecture canvas.</summary>
    public IState<string?> HoveredModuleId => ArchitectureLayer.HoveredModuleId;

    /// <summary>Hovered state pill on the Interactions canvas.</summary>
    public IState<StateKind?> HoveredStateKind => InteractionsLayer.HoveredStateKind;

    /// <summary>Per-layer file overrides — the user can paste a custom body
    /// into the FilesRail editor and the bundle uses that instead of the
    /// synthesised markdown. Empty string is a valid override (means "emit
    /// nothing for this layer"); null removes the override entirely.</summary>
    public IState<IImmutableDictionary<LayerKind, string?>> Overrides =>
        State<IImmutableDictionary<LayerKind, string?>>.Value(this, AllEmptyOverrides);

    private static IImmutableDictionary<LayerKind, string?> AllEmptyOverrides() =>
        Layers.All.ToImmutableDictionary(l => l.Kind, _ => (string?)null);

    /// <summary>One acknowledgment line per layer in the previewing state —
    /// captured from the first sentence of the user prompt when
    /// <see cref="GeneratePreview"/> ran. Composer footer displays it above
    /// the primary action so the user sees a quote of what they asked for.</summary>
    public IState<IImmutableDictionary<LayerKind, string>> PreviewAcks =>
        State<IImmutableDictionary<LayerKind, string>>.Value(this, AllEmptyAcks);

    private static IImmutableDictionary<LayerKind, string> AllEmptyAcks() =>
        Layers.All.ToImmutableDictionary(l => l.Kind, _ => string.Empty);

    /// <summary>Locked-card auto-collapse — delegated to ShellModel.</summary>
    public IFeed<IImmutableSet<LayerKind>> DefaultExpandedKinds => Shell.DefaultExpandedKinds;

    /// <summary>Per-layer suggestion chips shown in the composer footer.
    /// Click sets the prompt + marks dirty + focuses the textarea. Per
    /// v11 interaction brief §5.</summary>
    public static readonly IImmutableDictionary<LayerKind, ImmutableArray<string>> SuggestionChips =
        new Dictionary<LayerKind, ImmutableArray<string>>
        {
            [LayerKind.Intent]         = ImmutableArray.Create("Mobile-first", "Offline-first", "No backend yet"),
            [LayerKind.UX]             = ImmutableArray.Create("Drag-to-reorder", "Return to dashboard", "Modal confirmation"),
            [LayerKind.Architecture]   = ImmutableArray.Create("Swap MVUX for MVVM", "Add auth service", "Drop offline storage"),
            [LayerKind.DesignSystem]   = ImmutableArray.Create("Use my brand color", "Darken the surface", "Tighten the type scale"),
            [LayerKind.Interactions]   = ImmutableArray.Create("Queue silently", "Always show banner", "Banner when queue > 0"),
            [LayerKind.Data]            = ImmutableArray.Create("Add audit fields", "Make Schedule nullable", "Split into commands"),
            [LayerKind.Implementation] = ImmutableArray.Create("Front-load tests", "Defer offline cache", "Phase domain first"),
            [LayerKind.Scaffold]       = ImmutableArray.Create("Show all the files", "Just the command", "Bundle and quit"),
        }.ToImmutableDictionary();

    /// <summary>Which state pill is the active focus on the Interactions canvas.</summary>
    public IState<StateKind> ActiveStateKind => InteractionsLayer.ActiveStateKind;

    /// <summary>Per-layer prompt draft text. Each layer keeps its own buffer so
    /// switching between layers preserves the in-flight composer-prompt content
    /// per brief 02 §1.</summary>
    public IState<IImmutableDictionary<LayerKind, string>> Prompts =>
        State<IImmutableDictionary<LayerKind, string>>.Value(this, AllEmptyPrompts);

    private static IImmutableDictionary<LayerKind, string> AllEmptyPrompts() =>
        Layers.All.ToImmutableDictionary(l => l.Kind, _ => string.Empty);

    /// <summary>The active layer's prompt — convenience feed for the composer footer.</summary>
    public IFeed<string> ActivePrompt =>
        Feed.Combine(ActiveLayer, Prompts).Select(t =>
            t.Item2.TryGetValue(t.Item1.Kind, out var p) ? p : string.Empty);

    // ─── Private mutable bookkeeping (not feeds — drives lifecycle only) ──
    private readonly Dictionary<LayerKind, LayerSnapshot> _snapshots = new();
    private readonly Dictionary<LayerKind, object> _previewValues = new();
    private readonly Dictionary<LayerKind, string> _previewSummary = new();

    /// <summary>Read by canvases when <see cref="LayerState.Previewing"/>. Returns
    /// null when no preview is staged for the layer.</summary>
    public object? GetPreviewValues(LayerKind kind) =>
        _previewValues.TryGetValue(kind, out var v) ? v : null;

    public string? GetPreviewSummary(LayerKind kind) =>
        _previewSummary.TryGetValue(kind, out var s) ? s : null;

    // ────────────────────────────────────────────────────────────────────
    //  Commands
    // ────────────────────────────────────────────────────────────────────

    /// <summary>Click a Composition Stack row or a locked-card "Revisit" link.
    /// Setting state back to Clean preserves canonical values — revisit is
    /// non-destructive per brief 01 §4.</summary>
    public async ValueTask Jump(int index, CancellationToken ct = default)
        => await Shell.Jump(index, ct);

    public async ValueTask Revisit(LayerKind kind, CancellationToken ct = default)
        => await Shell.Revisit(kind, ct);

    /// <summary>Marks the active layer dirty. Called by canvas edit handlers
    /// (brief 02) and by the prompt textarea when its text becomes non-empty.</summary>
    public async ValueTask MarkActiveDirty(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        await MarkDirty(def.Kind, ct);
    }

    public async ValueTask MarkDirty(LayerKind kind, CancellationToken ct = default)
    {
        var states = await LayerStates;
        if (!states.TryGetValue(kind, out var current) || current == LayerState.Dirty || current == LayerState.Previewing)
            return;
        if (current == LayerState.Locked) return; // locked layers don't go dirty without explicit Revisit

        // Snapshot canonical values on first transition into Dirty so
        // DiscardEdits has something to roll back to.
        await CaptureSnapshot(kind, ct);
        await SetLayerState(kind, LayerState.Dirty, ct);
    }

    /// <summary>Sets the active layer's prompt buffer; transitions Clean→Dirty
    /// when text becomes non-empty.</summary>
    public async ValueTask SetActivePrompt(string text, CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        await Prompts.UpdateAsync(d => d?.SetItem(def.Kind, text ?? "") ?? AllEmptyPrompts(), ct);
        if (!string.IsNullOrWhiteSpace(text))
            await MarkDirty(def.Kind, ct);
    }

    /// <summary>Clean-path advance: lock the active layer (no preview to adopt).</summary>
    public async ValueTask LockAndContinue(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        var states = await LayerStates;
        var s = states.TryGetValue(def.Kind, out var cs) ? cs : LayerState.Clean;
        if (s == LayerState.Previewing)
        {
            // If the preview pipeline somehow led here, treat as accept.
            await AcceptAndLock(ct);
            return;
        }

        ClearPreview(def.Kind);
        _snapshots.Remove(def.Kind);
        await SetLayerState(def.Kind, LayerState.Locked, ct);
        await ClearPrompt(def.Kind, ct);
        await AdvanceIfPossible(ct);
    }

    /// <summary>Sends the active layer to <see cref="ILayerPreviewService"/>
    /// and stages the proposed values. The proposed values are applied to
    /// canonical state immediately so the canvas re-renders WYSIWYG with
    /// the AI's proposal — the user clicks Accept to lock the layer or
    /// Discard preview to revert from the snapshot.</summary>
    public async ValueTask GeneratePreview(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        var current = await ReadLayerValues(def.Kind, ct);
        if (current is null) return;

        var prompts = await Prompts;
        var prompt = prompts.TryGetValue(def.Kind, out var p) ? p : string.Empty;

        var lockedSummaries = BuildLockedContextSummaries(await LockedLayers, ct);

        var result = await PreviewService.GeneratePreviewAsync(def.Kind, current, prompt, lockedSummaries, ct);
        _previewValues[def.Kind] = result.ProposedValues;
        _previewSummary[def.Kind] = result.Summary;

        // Capture an acknowledgment quote from the prompt so the composer
        // footer can show "You asked: …" above the primary action in the
        // Previewing state. Per v11 interaction brief §6.
        await CapturePreviewAck(def.Kind, prompt, ct);

        // Apply the proposal to canonical state right away so the canvas
        // re-renders with the AI's pass — the user shouldn't need to click
        // Accept just to *see* what changed.
        await ApplyLayerValues(def.Kind, result.ProposedValues, ct);

        await SetLayerState(def.Kind, LayerState.Previewing, ct);
    }

    /// <summary>Architecture-specific re-roll command per brief 04 §4 — fires
    /// the preview pipeline directly without going through the dirty/prompt
    /// path. Used by the canvas's "↻ Regenerate diagram" affordance, which
    /// is an explicit user request to redraw the blueprint from the locked
    /// upstream context.</summary>
    public async ValueTask RegenerateArchitecture(CancellationToken ct = default)
    {
        // Snapshot for Discard before the canvas mutates.
        await CaptureSnapshot(LayerKind.Architecture, ct);

        var current = await Architecture;
        var prompt  = (await Prompts).TryGetValue(LayerKind.Architecture, out var p) ? p : string.Empty;
        var lockedSummaries = BuildLockedContextSummaries(await LockedLayers, ct);

        var result = await PreviewService.GeneratePreviewAsync(
            LayerKind.Architecture, current, prompt, lockedSummaries, ct);
        _previewValues[LayerKind.Architecture] = result.ProposedValues;
        _previewSummary[LayerKind.Architecture] = result.Summary;

        await ApplyLayerValues(LayerKind.Architecture, result.ProposedValues, ct);
        await SetLayerState(LayerKind.Architecture, LayerState.Previewing, ct);
    }

    public async ValueTask AcceptAndLock(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        if (_previewValues.TryGetValue(def.Kind, out var proposed))
            await ApplyLayerValues(def.Kind, proposed, ct);

        ClearPreview(def.Kind);
        _snapshots.Remove(def.Kind);
        await SetLayerState(def.Kind, LayerState.Locked, ct);
        await ClearPrompt(def.Kind, ct);
        await AdvanceIfPossible(ct);
    }

    public async ValueTask DiscardEdits(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        if (_snapshots.TryGetValue(def.Kind, out var snap))
            await RestoreFromSnapshot(def.Kind, snap, ct);

        _snapshots.Remove(def.Kind);
        ClearPreview(def.Kind);
        await ClearPrompt(def.Kind, ct);
        await SetLayerState(def.Kind, LayerState.Clean, ct);
    }

    public async ValueTask DiscardPreview(CancellationToken ct = default)
    {
        var def = await ActiveLayer;
        // GeneratePreview overwrote canonical with the AI proposal — restore
        // the pre-edit snapshot so the user is back to where they started.
        if (_snapshots.TryGetValue(def.Kind, out var snap))
            await RestoreFromSnapshot(def.Kind, snap, ct);

        _snapshots.Remove(def.Kind);
        ClearPreview(def.Kind);
        await ClearPrompt(def.Kind, ct);
        await SetLayerState(def.Kind, LayerState.Clean, ct);
    }

    public async ValueTask Reset(CancellationToken ct = default)
    {
        _snapshots.Clear();
        _previewValues.Clear();
        _previewSummary.Clear();

        await Stack.UpdateAsync(_ => StackPreferences.Default, ct);
        await Intent.UpdateAsync(_ => IntentValues.Default, ct);
        await Architecture.UpdateAsync(_ => ArchitectureBlueprint.Default, ct);
        await UX.UpdateAsync(_ => UXFlow.Default, ct);
        await Design.UpdateAsync(_ => DesignTokens.Default, ct);
        await Interactions.UpdateAsync(_ => InteractionsMatrix.Default, ct);
        await Data.UpdateAsync(_ => DataContracts.Default, ct);
        await Implementation.UpdateAsync(_ => BuildPlan.Default, ct);

        await Prompts.UpdateAsync(_ => AllEmptyPrompts(), ct);
        // Layer state machine + active index are owned by ShellModel.
        await LayerStates.UpdateAsync(_ =>
            Layers.All.ToImmutableDictionary(l => l.Kind, _ => LayerState.Clean) as IImmutableDictionary<LayerKind, LayerState>, ct);
        await ActiveIndex.UpdateAsync(_ => 0, ct);

        await AppName.UpdateAsync(_ => string.Empty, ct);
        await AppDescription.UpdateAsync(_ => string.Empty, ct);
        await Runtime.UpdateAsync(_ => DefaultRuntime, ct);
        await IsWebSelected.UpdateAsync(_ => false, ct);
        await IsAndroidSelected.UpdateAsync(_ => false, ct);
        await IsIOSSelected.UpdateAsync(_ => false, ct);
        await IsWindowsSelected.UpdateAsync(_ => false, ct);
        await IsMacOSSelected.UpdateAsync(_ => false, ct);
        await IsDesktopSelected.UpdateAsync(_ => false, ct);
    }

    public async ValueTask DownloadBundle(CancellationToken ct = default)
    {
        // Bundle export is brief 02's responsibility (per-layer file emission
        // templates) — for now this hooks Exporter so the download plumbing
        // exists. Brief 02 wires the per-layer markdown templates.
        try
        {
            var name = await AppName;
            var files = await BuildBundleFilesPlaceholder();
            await Exporter.SaveBundleAsync(name, files, ct);
        }
        catch
        {
            // Silent fail for now — Brief 02 wires error surfaces.
        }
    }

    // Per-layer canvas setters — each updates canonical values and marks
    // the corresponding layer dirty so Generate-preview becomes available.
    public async ValueTask SetIntent(IntentValues values, CancellationToken ct = default)
    {
        if (values is null) return;
        await Intent.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.Intent, ct);
    }

    /// <summary>FilesRail "Edit" toggle handler — set or clear a per-layer
    /// override that takes precedence over the synthesised markdown when
    /// the bundle exports. Empty string is a valid override (emit nothing);
    /// null removes the override entirely. Always marks the layer dirty
    /// so Generate-preview becomes available — even on a reset.</summary>
    public async ValueTask SetOverride(LayerKind kind, string? content, CancellationToken ct = default)
    {
        await Overrides.UpdateAsync(d => (d ?? AllEmptyOverrides()).SetItem(kind, content), ct);
        await MarkDirty(kind, ct);
    }

    /// <summary>Capture the first ~80 chars of the user prompt as an
    /// acknowledgment quote. Called by GeneratePreview before transitioning
    /// to Previewing so the composer footer can render the quote above the
    /// primary action. Per v11 interaction brief §6.</summary>
    public async ValueTask CapturePreviewAck(LayerKind kind, string prompt, CancellationToken ct = default)
    {
        var firstSentence = (prompt ?? string.Empty).Split('.', 2)[0].Trim();
        if (firstSentence.Length > 80) firstSentence = firstSentence.Substring(0, 80).TrimEnd() + "…";
        await PreviewAcks.UpdateAsync(d => (d ?? AllEmptyAcks()).SetItem(kind, firstSentence), ct);
    }

    public async ValueTask SetArchitecture(ArchitectureBlueprint values, CancellationToken ct = default)
    {
        if (values is null) return;
        await Architecture.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.Architecture, ct);
    }

    public async ValueTask SetUX(UXFlow values, CancellationToken ct = default)
    {
        if (values is null) return;
        await UX.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.UX, ct);
    }

    public async ValueTask SetDesign(DesignTokens values, CancellationToken ct = default)
    {
        if (values is null) return;
        await Design.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.DesignSystem, ct);
    }

    public async ValueTask SetData(DataContracts values, CancellationToken ct = default)
    {
        if (values is null) return;
        await Data.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.Data, ct);
    }

    public async ValueTask SetImplementation(BuildPlan values, CancellationToken ct = default)
    {
        if (values is null) return;
        await Implementation.UpdateAsync(_ => values, ct);
        await MarkDirty(LayerKind.Implementation, ct);
    }

    public async ValueTask SetAppName(string name, CancellationToken ct = default)
        => await AppName.UpdateAsync(_ => name ?? string.Empty, ct);

    public async ValueTask SetAppDescription(string description, CancellationToken ct = default)
        => await AppDescription.UpdateAsync(_ => description ?? string.Empty, ct);

    public async ValueTask SetRuntime(string runtime, CancellationToken ct = default)
        => await Runtime.UpdateAsync(_ => runtime ?? DefaultRuntime, ct);

    public async ValueTask TogglePlatform(string platform, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(platform)) return;
        var state = platform switch
        {
            "Web"     => IsWebSelected,
            "Android" => IsAndroidSelected,
            "iOS"     => IsIOSSelected,
            "Windows" => IsWindowsSelected,
            "macOS"   => IsMacOSSelected,
            "Desktop" => IsDesktopSelected,
            _          => null,
        };
        if (state is null) return;
        await state.UpdateAsync(v => !v, ct);
    }

    // ────────────────────────────────────────────────────────────────────
    //  Internal helpers
    // ────────────────────────────────────────────────────────────────────

    private async ValueTask SetLayerState(LayerKind kind, LayerState s, CancellationToken ct)
        => await Shell.SetLayerStateInternal(kind, s, ct);

    private async ValueTask ClearPrompt(LayerKind kind, CancellationToken ct) =>
        await Prompts.UpdateAsync(d => d?.SetItem(kind, string.Empty) ?? AllEmptyPrompts(), ct);

    private void ClearPreview(LayerKind kind)
    {
        _previewValues.Remove(kind);
        _previewSummary.Remove(kind);
    }

    private async ValueTask AdvanceIfPossible(CancellationToken ct)
        => await Shell.AdvanceIfPossible(ct);

    private async ValueTask CaptureSnapshot(LayerKind kind, CancellationToken ct)
    {
        if (_snapshots.ContainsKey(kind)) return;
        _snapshots[kind] = kind switch
        {
            LayerKind.Intent         => new LayerSnapshot(Intent:       await Intent),
            LayerKind.Architecture   => new LayerSnapshot(Arch:         await Architecture),
            LayerKind.UX             => new LayerSnapshot(UX:           await UX),
            LayerKind.DesignSystem   => new LayerSnapshot(Design:       await Design),
            LayerKind.Interactions   => new LayerSnapshot(Interactions: await Interactions),
            LayerKind.Data           => new LayerSnapshot(Data:         await Data),
            LayerKind.Implementation => new LayerSnapshot(Implementation: await Implementation),
            _                        => new LayerSnapshot(),
        };
    }

    private async ValueTask RestoreFromSnapshot(LayerKind kind, LayerSnapshot snap, CancellationToken ct)
    {
        switch (kind)
        {
            case LayerKind.Intent         when snap.Intent         is { } v: await Intent.UpdateAsync(_ => v, ct); break;
            case LayerKind.Architecture   when snap.Arch           is { } v: await Architecture.UpdateAsync(_ => v, ct); break;
            case LayerKind.UX             when snap.UX             is { } v: await UX.UpdateAsync(_ => v, ct); break;
            case LayerKind.DesignSystem   when snap.Design         is { } v: await Design.UpdateAsync(_ => v, ct); break;
            case LayerKind.Interactions   when snap.Interactions   is { } v: await Interactions.UpdateAsync(_ => v, ct); break;
            case LayerKind.Data           when snap.Data           is { } v: await Data.UpdateAsync(_ => v, ct); break;
            case LayerKind.Implementation when snap.Implementation is { } v: await Implementation.UpdateAsync(_ => v, ct); break;
            // Scaffold has no canonical record snapshot — terminal layer.
        }
    }

    private async Task<object?> ReadLayerValues(LayerKind kind, CancellationToken ct) => kind switch
    {
        LayerKind.Intent         => await Intent,
        LayerKind.Architecture   => await Architecture,
        LayerKind.UX             => await UX,
        LayerKind.DesignSystem   => await Design,
        LayerKind.Data           => await Data,
        LayerKind.Implementation => await Implementation,
        LayerKind.Interactions   => await Interactions,
        _ => null, // Scaffold has no editable values — preview is a no-op there
    };

    private async ValueTask ApplyLayerValues(LayerKind kind, object proposed, CancellationToken ct)
    {
        switch (kind)
        {
            case LayerKind.Intent         when proposed is IntentValues v:          await Intent.UpdateAsync(_ => v, ct); break;
            case LayerKind.Architecture   when proposed is ArchitectureBlueprint v: await Architecture.UpdateAsync(_ => v, ct); break;
            case LayerKind.UX             when proposed is UXFlow v:                await UX.UpdateAsync(_ => v, ct); break;
            case LayerKind.DesignSystem   when proposed is DesignTokens v:          await Design.UpdateAsync(_ => v, ct); break;
            case LayerKind.Data           when proposed is DataContracts v:         await Data.UpdateAsync(_ => v, ct); break;
            case LayerKind.Implementation when proposed is BuildPlan v:             await Implementation.UpdateAsync(_ => v, ct); break;
            case LayerKind.Interactions   when proposed is InteractionsMatrix v:           await Interactions.UpdateAsync(_ => v, ct); break;
            case LayerKind.Interactions   when proposed is ImmutableArray<InteractionFlow> v: await Interactions.UpdateAsync(_ => new InteractionsMatrix(v), ct); break;
        }
    }

    private IReadOnlyDictionary<LayerKind, string> BuildLockedContextSummaries(
        IImmutableSet<LayerKind> locked, CancellationToken ct)
    {
        // Brief 02 derives rich per-layer summaries from canonical values.
        // Brief 01 keeps the surface minimal — just the layer hint text so
        // the AI has names to reference.
        return locked.ToDictionary(k => k, k => Layers.Get(k).Hint);
    }

    private async Task<string> CollectPlatformsString(CancellationToken ct)
    {
        var parts = new List<string>();
        if (await IsWebSelected) parts.Add("Web");
        if (await IsIOSSelected) parts.Add("iOS");
        if (await IsAndroidSelected) parts.Add("Android");
        if (await IsWindowsSelected) parts.Add("Windows");
        if (await IsMacOSSelected) parts.Add("macOS");
        if (await IsDesktopSelected) parts.Add("Desktop");
        return string.Join(", ", parts);
    }

    /// <summary>Live-derived synthesis of <c>ColorPaletteOverride.xaml</c> from
    /// the current Design tokens. Spec: brief 05 §4.</summary>
    public IFeed<string> ColorPaletteOverrideXaml =>
        Design.Select(LayerMarkdownTemplates.BuildColorPaletteOverrideXaml);

    private static ImmutableArray<string> ResolvePlatformHeads(
        bool web, bool android, bool ios, bool windows, bool macOS, bool desktop)
    {
        var heads = ImmutableArray.CreateBuilder<string>();
        if (ios || android) heads.Add("Mobile");
        if (windows || macOS || desktop) heads.Add("Desktop");
        if (web) heads.Add("Wasm");
        return heads.ToImmutable();
    }

    private async Task<string> BuildSolutionTreeAsync(CancellationToken ct)
    {
        var name    = await AppName;
        var web     = await IsWebSelected;
        var android = await IsAndroidSelected;
        var ios     = await IsIOSSelected;
        var windows = await IsWindowsSelected;
        var macOS   = await IsMacOSSelected;
        var desktop = await IsDesktopSelected;
        return LayerMarkdownTemplates.BuildSolutionTree(
            name,
            ResolvePlatformHeads(web, android, ios, windows, macOS, desktop));
    }

    /// <summary>Build the bundle file dictionary from live state — one
    /// markdown per layer plus the synthesised <c>ColorPaletteOverride.xaml</c>,
    /// <c>README.md</c>, and <c>prompt-context.md</c>.</summary>
    private async Task<IDictionary<string, string>> BuildBundleFilesPlaceholder()
    {
        var name      = await AppName;
        var platforms = await CollectPlatformsString(default);
        var runtime   = await Runtime;
        var stack     = await Stack;
        var intent    = await Intent;
        var arch      = await Architecture;
        var design    = await Design;
        var matrix    = await Interactions;
        var solTree   = await BuildSolutionTreeAsync(default);
        var paletteX  = await ColorPaletteOverrideXaml;
        var overrides = await Overrides;

        var dict = new Dictionary<string, string>();

        // Stack preferences — emitted first; every other layer downstream
        // references it. Per docs/ENGINEERING-BRIEF-01-stack-preferences.md.
        dict["stack-preferences.md"] = LayerMarkdownTemplates.BuildStackPreferences(stack);

        // Architecture, Design, Interactions: full templates per briefs 04/05/06.
        dict["architecture.md"] = LayerMarkdownTemplates.BuildArchitecture(
            arch, solTree,
            whyThis: "MVUX gives both views identical reactive feeds with offline-first state — no MVVM ceremony, no Rx plumbing. Navigation regions let Shell and TabBar animate independently.",
            agentPrompt: "When generating XAML, use uen:Region.Attached for navigation surfaces and bind ItemsSource to feeds (IFeed). Never construct ViewModels in code-behind.");

        dict["design-system.md"] = LayerMarkdownTemplates.BuildDesignSystem(
            design, paletteX,
            whyThis: "Outdoor sun + a phone in a truck mount means low chroma, high contrast. Action is the only saturated hue and is reserved for the next action — never decoration.",
            agentPrompt: "Apply tokens via ThemeResource — never hex. Reference SurfaceBrush, OnSurfaceBrush, SecondaryBrush. Light and Dark theme dictionaries let the same XAML render correctly in both themes — no per-theme branching in markup.");

        dict["interaction-spec.md"] = LayerMarkdownTemplates.BuildInteractionSpec(
            matrix,
            whyThis: "Offline-first means six states aren't optional — every flow's surface degrades to a queued / offline read at minimum. The contract is by name so generated VSM XAML can be verified.",
            agentPrompt: "Every screen must implement all six states. Use VisualStateManager groups named exactly: Default, Loading, Empty, Error, Success, Offline. Bind to a single IState<string> via utu:VisualStateManager.States — no procedural VisualStateManager.GoToState in code-behind.");

        // Color override file lives alongside the markdown.
        dict["ColorPaletteOverride.xaml"] = paletteX;

        // Other layers: thin stubs for now (UX, Data, Implementation, Scaffold
        // get full templates in a later pass).
        foreach (var l in Layers.All)
        {
            if (l.Kind is LayerKind.Intent
                       or LayerKind.Architecture
                       or LayerKind.DesignSystem
                       or LayerKind.Interactions)
                continue;
            dict[l.File] = $"# {l.Label}\n\nPlaceholder — template emission to follow.";
        }

        // Intent layer output — README.md, derived from the live Intent state.
        dict[Layers.ReadmeFileName] = LayerMarkdownTemplates.BuildIntentReadme(name, runtime, intent);

        dict[Layers.PromptContextFileName] = $"# prompt-context\n\nProject: {name}\n";

        // Per-layer overrides take precedence over synthesised content —
        // user paste in FilesRail wins. Null override = use synthesised
        // content; non-null override (even empty string) replaces it.
        if (overrides is not null)
        {
            foreach (var l in Layers.All)
            {
                if (overrides.TryGetValue(l.Kind, out var content) && content is not null)
                    dict[l.File] = content;
            }
        }

        return dict;
    }

    private static string SanitizeIdentifier(string s)
    {
        if (string.IsNullOrEmpty(s)) return "App";
        var b = new System.Text.StringBuilder(s.Length);
        foreach (var c in s)
        {
            if (char.IsLetterOrDigit(c) || c == '_') b.Append(c);
        }
        if (b.Length == 0) return "App";
        if (char.IsDigit(b[0])) b.Insert(0, '_');
        return b.ToString();
    }
}

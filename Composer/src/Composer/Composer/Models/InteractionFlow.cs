using System.Collections.Immutable;
using Windows.Foundation;

namespace Composer.Models;

/// <summary>
/// Layer 5 — Interactions. The canonical six-state set. Every flow implements
/// every state — the contract is by name, so downstream VSM XAML can reference
/// states without index dependencies.
/// Spec: <c>docs/update-context-engine-06-interactions.md</c> §1.
/// </summary>
public enum StateKind
{
    Default,
    Loading,
    Empty,
    Error,
    Success,
    Offline,
}

/// <summary>
/// One interaction flow (e.g. "create-job") — exactly six states, one per
/// <see cref="StateKind"/>, in canonical enum order.
/// </summary>
public partial record InteractionFlow(
    string Id,
    string Label,
    ImmutableArray<StateDef> States)
{
    /// <summary>Canonical six states in order. The agent's downstream VSM
    /// generation matches this order and these names exactly.</summary>
    public static readonly ImmutableArray<StateKind> StandardStates =
        ImmutableArray.Create(
            StateKind.Default,
            StateKind.Loading,
            StateKind.Empty,
            StateKind.Error,
            StateKind.Success,
            StateKind.Offline);

    /// <summary>Construct a flow with one description per canonical state.
    /// Empty defaults are allowed so existing call sites keep compiling.</summary>
    public static InteractionFlow ForFlow(
        string id, string label,
        string defaultDesc = "",
        string loadingDesc = "",
        string emptyDesc   = "",
        string errorDesc   = "",
        string successDesc = "",
        string offlineDesc = "") => new(
            id, label,
            ImmutableArray.Create(
                new StateDef(StateKind.Default, defaultDesc),
                new StateDef(StateKind.Loading, loadingDesc),
                new StateDef(StateKind.Empty,   emptyDesc),
                new StateDef(StateKind.Error,   errorDesc),
                new StateDef(StateKind.Success, successDesc),
                new StateDef(StateKind.Offline, offlineDesc)));

    /// <summary>Lookup by kind. Returns the state definition for the given
    /// <see cref="StateKind"/>, or a description-empty fallback.</summary>
    public StateDef ByKind(StateKind kind)
    {
        foreach (var s in States)
            if (s.Kind == kind) return s;
        return new StateDef(kind, "");
    }
}

/// <summary>
/// One state inside an <see cref="InteractionFlow"/>. The kind is canonical
/// (six values, fixed). Description is editable through the composer prompt.
/// Id and Label are derived from <see cref="Kind"/> for backwards-compat with
/// consumers that read those fields directly.
/// </summary>
public partial record StateDef(StateKind Kind, string Description)
{
    /// <summary>Lowercase id (e.g. "default", "loading"). Stable across renames
    /// of the enum; derived from <see cref="Kind"/>.</summary>
    public string Id => Kind switch
    {
        StateKind.Default => "default",
        StateKind.Loading => "loading",
        StateKind.Empty   => "empty",
        StateKind.Error   => "error",
        StateKind.Success => "success",
        StateKind.Offline => "offline",
        _ => "unknown",
    };

    /// <summary>Pascal-cased label (matches the VSM state name in generated
    /// XAML). Derived from <see cref="Kind"/>.</summary>
    public string Label => Kind.ToString();

    /// <summary>Top-left position on the 800×340 SVG canvas. Per the v11
    /// interactions brief §5.5, each canonical state has a fixed slot in a
    /// 3×2 grid; the position constants below match that layout.</summary>
    public Point Position => Kind switch
    {
        StateKind.Default => new Point( 60,  50),
        StateKind.Loading => new Point(280,  50),
        StateKind.Success => new Point(560,  50),
        StateKind.Empty   => new Point( 60, 200),
        StateKind.Error   => new Point(280, 200),
        StateKind.Offline => new Point(560, 200),
        _                 => new Point(  0,   0),
    };

    /// <summary>State-color key into Tokens.xaml. Per brief §1.3.</summary>
    public string ColorKey => Kind switch
    {
        StateKind.Default => "StateColorDefault",
        StateKind.Loading => "StateColorLoading",
        StateKind.Empty   => "StateColorEmpty",
        StateKind.Error   => "StateColorError",
        StateKind.Success => "StateColorSuccess",
        StateKind.Offline => "StateColorOffline",
        _                 => "InkBrush",
    };
}

/// <summary>
/// One transition between two states inside an <see cref="InteractionFlow"/>.
/// The canvas renders each as a curved arrow (cubic Bézier) from the source
/// pill's edge to the target pill's edge, labeled with the trigger.
/// Per v11 interactions brief §5.5.3.
/// </summary>
public partial record TransitionDef(
    StateKind FromKind,
    StateKind ToKind,
    string Label);

/// <summary>
/// The full interactions matrix — three flows by default, each with the six
/// canonical states. Per brief 06 §1, this is the canonical state shape and
/// the unit of preview/lock for the Interactions layer.
/// </summary>
public partial record InteractionsMatrix(
    ImmutableArray<InteractionFlow> Flows,
    ImmutableArray<TransitionDef> Transitions = default)
{
    public static InteractionsMatrix Empty { get; } =
        new(ImmutableArray<InteractionFlow>.Empty,
            ImmutableArray<TransitionDef>.Empty);

    /// <summary>The canonical 8 transitions across the six-state graph,
    /// per v11 interactions brief §5.5.3. Same set applies to every flow
    /// in v11 — the brief leaves per-flow transition overrides as future
    /// scope.</summary>
    public static readonly ImmutableArray<TransitionDef> StandardTransitions =
        ImmutableArray.Create(
            new TransitionDef(StateKind.Default, StateKind.Loading, "user acts"),
            new TransitionDef(StateKind.Loading, StateKind.Success, "ok"),
            new TransitionDef(StateKind.Loading, StateKind.Error,   "fail"),
            new TransitionDef(StateKind.Loading, StateKind.Offline, "no net"),
            new TransitionDef(StateKind.Empty,   StateKind.Default, "data arrives"),
            new TransitionDef(StateKind.Error,   StateKind.Loading, "retry"),
            new TransitionDef(StateKind.Success, StateKind.Default, "ack"),
            new TransitionDef(StateKind.Offline, StateKind.Loading, "reconnect"));

    /// <summary>Default matrix tuned to the field-service archetype the
    /// briefs use as a running example. For other archetypes, the AI layer
    /// preview returns a different default on first land on the layer.</summary>
    public static InteractionsMatrix Default { get; } = new(
        Transitions: StandardTransitions,
        Flows: ImmutableArray.Create(
        InteractionFlow.ForFlow(
            id: "create-job", label: "Create job",
            defaultDesc: "Empty calendar; primary CTA visible.",
            loadingDesc: "Skeleton rows; CTA disabled.",
            emptyDesc:   "No technicians available; suggest filters.",
            errorDesc:   "Validation conflict; inline message + retry.",
            successDesc: "Toast confirmation; new row animates in.",
            offlineDesc: "Queued locally; banner indicates sync pending."),
        InteractionFlow.ForFlow(
            id: "sign-in", label: "Sign in",
            defaultDesc: "Email + password fields; Sign in CTA visible.",
            loadingDesc: "Spinner replaces CTA; fields disabled.",
            emptyDesc:   "First-launch: invite to register instead.",
            errorDesc:   "Inline error: \"Wrong password\" below the password field.",
            successDesc: "Briefly show \"Signed in\", then route to Dashboard.",
            offlineDesc: "Show offline banner; allow biometric fallback if cached."),
        InteractionFlow.ForFlow(
            id: "sync", label: "Sync data",
            defaultDesc: "Last sync timestamp visible; pull-to-refresh available.",
            loadingDesc: "Indeterminate spinner near timestamp.",
            emptyDesc:   "No changes to sync — show \"Up to date\".",
            errorDesc:   "Server error toast; retry on next interval.",
            successDesc: "Timestamp updates; subtle checkmark animation.",
            offlineDesc: "Queue accumulates; banner shows queue depth.")));
}

using Composer.Services;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 5 — Interactions. Owns the canonical <see cref="InteractionsMatrix"/>
/// (flows × states grid). Derived <see cref="Context"/> feed enables
/// per-flow regeneration based on Intent's entity noun. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.6 / §11.
/// </summary>
public partial record InteractionsModel(
    ShellModel              Shell,
    IntentModel             IntentLayer,
    IContextDeriver         ContextDeriver)
{
    public IFeed<DerivedContext> Context =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i));

    public IState<InteractionsMatrix> Matrix =>
        State<InteractionsMatrix>.Value(this, () => InteractionsMatrix.Default);

    /// <summary>Local view-state: which flow tab is selected.</summary>
    public IState<string> ActiveFlowId =>
        State<string>.Value(this, () => "create-job");

    /// <summary>Local view-state: which state pill is the active focus.</summary>
    public IState<StateKind> ActiveStateKind =>
        State<StateKind>.Value(this, () => StateKind.Default);

    /// <summary>Local view-state: hovered state pill. Decoupled from
    /// <see cref="ActiveStateKind"/>.</summary>
    public IState<StateKind?> HoveredStateKind =>
        State<StateKind?>.Value(this, () => (StateKind?)null);
}

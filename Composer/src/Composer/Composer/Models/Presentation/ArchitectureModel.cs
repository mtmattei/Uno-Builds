using Composer.Services;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 3 — Architecture. Owns the canonical <see cref="ArchitectureBlueprint"/>
/// describing module shape + edges. Derived <see cref="Context"/> feed
/// combines Intent + Stack so the blueprint can regenerate stack-correctly
/// (MVUX vs MVVM module label, offline-first vs HTTP module presence).
/// Per <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.4 / §11.
/// </summary>
public partial record ArchitectureModel(
    ShellModel              Shell,
    IntentModel             IntentLayer,
    IContextDeriver         ContextDeriver)
{
    public IState<ArchitectureBlueprint> Blueprint =>
        State<ArchitectureBlueprint>.Value(this, () => ArchitectureBlueprint.Default);

    /// <summary>Local view-state: hovered module id on the blueprint canvas.
    /// Null when nothing is hovered. Drives the connected-set highlight.</summary>
    public IState<string?> HoveredModuleId =>
        State<string?>.Value(this, () => (string?)null);

    public IFeed<DerivedContext> Context =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i));
}

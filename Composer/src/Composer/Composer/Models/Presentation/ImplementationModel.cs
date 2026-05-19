using Composer.Services;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 7 — Implementation. Owns the canonical <see cref="BuildPlan"/>
/// (phased build plan). Derived <see cref="Context"/> feed lets the plan
/// reference Intent's entity / user noun in phase descriptions. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.8 / §11.
/// </summary>
public partial record ImplementationModel(
    ShellModel       Shell,
    IntentModel      IntentLayer,
    IContextDeriver  ContextDeriver)
{
    public IState<BuildPlan> Plan =>
        State<BuildPlan>.Value(this, () => BuildPlan.Default);

    public IFeed<DerivedContext> Context =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i));
}

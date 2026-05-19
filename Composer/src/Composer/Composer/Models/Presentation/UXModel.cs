using Composer.Services;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 2 — UX. Owns the canonical <see cref="UXFlow"/> describing the
/// primary user flow, plus a derived <see cref="Context"/> feed projecting
/// the active <see cref="DerivedContext"/> from Intent values for downstream
/// flow regeneration. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.3 / §11.
/// </summary>
public partial record UXModel(
    ShellModel       Shell,
    IntentModel      IntentLayer,
    IContextDeriver  ContextDeriver)
{
    public IState<UXFlow> Flow =>
        State<UXFlow>.Value(this, () => UXFlow.Default);

    /// <summary>Re-derives every time Intent values change. Brief §11.2.</summary>
    public IFeed<DerivedContext> Context =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i));

    /// <summary>True while the canvas is showing archetype demo content
    /// (cold-launch / field-service defaults). Brief §9.3.</summary>
    public IFeed<bool> IsDemoContent =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i).IsFieldService);
}

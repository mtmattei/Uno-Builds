using Composer.Services;
using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 6 — Data. Owns the canonical <see cref="DataContracts"/> (entity
/// records). Derived <see cref="Context"/> feed picks up Intent's entity
/// noun so generated records align with the chosen domain. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.7 / §11.
/// </summary>
public partial record DataModel(
    ShellModel       Shell,
    IntentModel      IntentLayer,
    IContextDeriver  ContextDeriver)
{
    public IState<DataContracts> Contracts =>
        State<DataContracts>.Value(this, () => DataContracts.Default);

    public IFeed<DerivedContext> Context =>
        IntentLayer.Values.Select(i => ContextDeriver.Derive(i));
}

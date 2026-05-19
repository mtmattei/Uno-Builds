using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 1 — Intent. Owns the canonical <see cref="IntentValues"/> record
/// describing what the app is for. Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.2.
/// </summary>
public partial record IntentModel(ShellModel Shell)
{
    public IState<IntentValues> Values =>
        State<IntentValues>.Value(this, () => IntentValues.Default);
}

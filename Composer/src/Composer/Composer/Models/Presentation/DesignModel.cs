using Uno.Extensions.Reactive;

namespace Composer.Models.Presentation;

/// <summary>
/// Layer 4 — Design System. Owns the canonical <see cref="DesignTokens"/>
/// (palette + type scale + spacing rhythm). Per
/// <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §9.5.
/// </summary>
public partial record DesignModel(ShellModel Shell)
{
    public IState<DesignTokens> Tokens =>
        State<DesignTokens>.Value(this, () => DesignTokens.Default);
}

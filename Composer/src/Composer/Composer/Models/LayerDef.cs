using System.Collections.Immutable;

namespace Composer.Models;

/// <summary>
/// Static metadata about a single layer — index, label, file name, hint text.
/// Each layer reads its own values from <see cref="Layers.All"/> rather than
/// duplicating the strings.
/// </summary>
public record LayerDef(
    LayerKind Kind,
    int Index,
    string Label,
    string File,
    string Title,
    string Subtitle,
    string Hint)
{
    /// <summary>Navigation route name — derived from <see cref="Kind"/>.</summary>
    public string Route => Kind switch
    {
        LayerKind.Intent         => "intent",
        LayerKind.UX             => "ux",
        LayerKind.Architecture   => "architecture",
        LayerKind.DesignSystem   => "design",
        LayerKind.Interactions   => "interactions",
        LayerKind.Data           => "data",
        LayerKind.Implementation => "implementation",
        LayerKind.Scaffold       => "scaffold",
        _                        => "intent",
    };
}

/// <summary>
/// Source of truth for the eight layer definitions in fixed order. Intent
/// is the cold-launch surface; Scaffold is the terminal layer. Matches the
/// prototype's <c>LAYERS</c> array in <c>composer-context-engine.jsx</c>
/// lines 62–71. Stack preferences live as static defaults consumed by the
/// markdown bundle emitter — not a user-facing layer.
/// </summary>
public static class Layers
{
    public static readonly ImmutableArray<LayerDef> All = ImmutableArray.Create(
        new LayerDef(
            LayerKind.Intent, 0,
            Label:    "Intent",
            File:     "README.md",
            Title:    "What are we building?",
            Subtitle: "Fill what you know. I'll infer the rest as we go.",
            Hint:     "what the app is for"),

        new LayerDef(
            LayerKind.UX, 1,
            Label:    "UX",
            File:     "ux-flows.md",
            Title:    "How do users move through it?",
            Subtitle: "A suggested primary flow as a starting point. Refine in the prompt below or accept and continue.",
            Hint:     "how users move through it"),

        new LayerDef(
            LayerKind.Architecture, 2,
            Label:    "Architecture",
            File:     "architecture.md",
            Title:    "How is this app shaped?",
            Subtitle: "Suggested baseline — modules and their connections. Refine in the prompt below or accept and continue.",
            Hint:     "how it is shaped"),

        new LayerDef(
            LayerKind.DesignSystem, 3,
            Label:    "Design System",
            File:     "design-system.md",
            Title:    "How should it feel?",
            Subtitle: "Palette, type scale, spacing rhythm — inferred from your description and any reference screenshots. Pick a different body font, or refine in the prompt to redraw.",
            Hint:     "how it feels"),

        new LayerDef(
            LayerKind.Interactions, 4,
            Label:    "Interactions",
            File:     "interaction-spec.md",
            Title:    "What about every state, every flow?",
            Subtitle: "For each flow, six states. Click a flow tab, click a state node — see what the user sees, what the system does.",
            Hint:     "every state of every flow"),

        new LayerDef(
            LayerKind.Data, 5,
            Label:    "Data",
            File:     "data-contracts.md",
            Title:    "What shapes does the data take?",
            Subtitle: "Three entities with explicit fields and nullability — the agent will scaffold C# records from these.",
            Hint:     "shapes and contracts"),

        new LayerDef(
            LayerKind.Implementation, 6,
            Label:    "Implementation",
            File:     "implementation-plan.md",
            Title:    "How does it get built?",
            Subtitle: "Six phases with explicit dependencies. The agent works through them in order, with verification per phase.",
            Hint:     "phased build plan"),

        new LayerDef(
            LayerKind.Scaffold, 7,
            Label:    "Scaffold",
            File:     "scaffold.command",
            Title:    "The bundle is ready.",
            Subtitle: "Every layer locked. The scaffold command bootstraps the project; the bundle delivers the docs into your repo.",
            Hint:     "runnable starting point"));

    /// <summary>Synthesized companion files emitted at scaffold lock alongside
    /// the seven per-layer files.</summary>
    public const string ReadmeFileName        = "README.md";
    public const string PromptContextFileName = "prompt-context.md";

    public static LayerDef Get(LayerKind kind) => All[IndexOf(kind)];

    public static int IndexOf(LayerKind kind)
    {
        for (var i = 0; i < All.Length; i++)
            if (All[i].Kind == kind) return i;
        return 0;
    }
}

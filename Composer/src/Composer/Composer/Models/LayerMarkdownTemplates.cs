using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Windows.UI;

namespace Composer.Models;

/// <summary>
/// Per-layer markdown emission. One static method per layer + a few derivation
/// helpers (longest-path on the architecture graph, solution-tree synthesis,
/// ColorPaletteOverride.xaml synthesis).
///
/// Specs:
///  - architecture.md     → docs/update-context-engine-04-architecture.md §6
///  - design-system.md    → docs/update-context-engine-05-design.md §5
///  - interaction-spec.md → docs/update-context-engine-06-interactions.md §4
///
/// The templates are deliberately self-contained — every output file makes
/// sense in isolation when an LLM reads it later (the core thesis of the
/// context engine). Agent guidance and rationale are embedded in each file
/// rather than referenced across files.
/// </summary>
internal static class LayerMarkdownTemplates
{
    // ── stack-preferences.md (Stack layer output) ──────────────────────────

    /// <summary>Build the project's Stack Preferences markdown — the
    /// canonical reference every downstream brief substitutes from. Per
    /// <c>docs/ENGINEERING-BRIEF-01-stack-preferences.md</c>.</summary>
    public static string BuildStackPreferences(StackPreferences p)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Stack Preferences");
        sb.AppendLine();
        sb.AppendLine("## Architecture");
        sb.AppendLine();
        sb.AppendLine($"- **Pattern:** {StackPreferences.Label(p.Pattern)}");
        sb.AppendLine($"- **Markup:** {StackPreferences.Label(p.Markup)}");
        sb.AppendLine($"- **Renderer:** {StackPreferences.Label(p.Renderer)}");
        sb.AppendLine($"- **HTTP client:** {StackPreferences.Label(p.Http)}");
        sb.AppendLine($"- **Navigation:** {StackPreferences.Label(p.Nav)}");
        sb.AppendLine($"- **Theme:** {StackPreferences.Label(p.Theme)}");
        sb.AppendLine();
        sb.AppendLine("## Target platforms");
        sb.AppendLine();
        foreach (var plat in p.Platforms.OrderBy(x => (int)x))
            sb.AppendLine($"- {StackPreferences.Label(plat)}");
        sb.AppendLine();
        sb.AppendLine("## What this means downstream");
        sb.AppendLine();
        if (p.IsMvux)
            sb.AppendLine("**MVUX is the chosen reactive pattern.** Models expose `IFeed<T>` and `IState<T>`; commands are public async methods invoked via the implicit `IAsyncCommand` binding. No `INotifyPropertyChanged`. No code-behind navigation calls.\n");
        else
            sb.AppendLine("**MVVM is the chosen reactive pattern.** Standard property-change notification with `INotifyPropertyChanged` and `ICommand` implementations.\n");

        sb.AppendLine(p.Markup == MarkupKind.Xaml
            ? "**XAML is the chosen markup.** All UI lives in `.xaml` files with code-behind kept thin. Bindings prefer `Mode=TwoWay` for state updates.\n"
            : "**C# Markup is the chosen markup.** All UI lives in fluent C# expressions. No `.xaml` files.\n");

        sb.AppendLine(p.Nav switch
        {
            NavigationKind.Region => "**Region-based navigation is the chosen approach.** Use `uen:Region.Attached` and `uen:Region.Name` on container elements; bind `Navigation.Request` declaratively. Never invoke `INavigator` from code-behind.\n",
            NavigationKind.Frame  => "**Frame navigation is the chosen approach.** Standard `Frame.Navigate` pattern from code-behind.\n",
            _                     => "**No navigation extensions.** Single-page app or hand-rolled routing.\n",
        });

        sb.AppendLine(p.Renderer == RendererKind.Skia
            ? "**Skia renderer is the chosen rendering backend.** All Uno Toolkit controls render consistently across iOS, Android, macOS, Linux, Windows, and WebAssembly.\n"
            : "**Native renderer is the chosen rendering backend.** Each platform renders via its native UI primitives.\n");

        sb.AppendLine(p.Theme switch
        {
            ThemeKind.Material  => "**Material theme is the chosen design system.** Resources resolve through Material brushes (`PrimaryBrush`, `SecondaryBrush`, etc.). Prefer Material if both Material and Fluent are present.\n",
            ThemeKind.Cupertino => "**Cupertino theme is the chosen design system.** iOS-style controls with Cupertino brushes.\n",
            ThemeKind.Fluent    => "**Fluent theme is the chosen design system.** WinUI-style controls with Fluent brushes.\n",
            _                   => "**Custom theme.** No pre-shipped theme resources — fully bespoke design system.\n",
        });

        sb.AppendLine(p.Http switch
        {
            HttpClientKind.Kiota => "**Kiota is the chosen HTTP client.** Typed client generation from OpenAPI definitions. Use `AddKiotaClient<T>()` in DI.\n",
            HttpClientKind.Refit => "**Refit is the chosen HTTP client.** Interface-based typed clients.\n",
            _                    => "**No HTTP client.** App is local-only / offline-first.\n",
        });

        sb.AppendLine("## Agent prompt");
        sb.AppendLine();
        sb.AppendLine("> When generating downstream code, every architectural decision must match");
        sb.AppendLine("> these preferences exactly. Reference patterns by their canonical Uno");
        sb.AppendLine("> names. If a recommended pattern is at odds with these preferences");
        sb.AppendLine("> (e.g. `MVVM` when the user chose `MVUX`), flag it and stop generating");
        sb.AppendLine("> rather than silently substituting.");
        return sb.ToString();
    }

    // ── README.md (Intent layer output) ────────────────────────────────────

    /// <summary>Build the project README from the Intent values + the
    /// hero's AppName / runtime. Spec: <c>docs/DESIGN-BRIEF.md</c> §6.</summary>
    public static string BuildIntentReadme(string appName, string runtime, IntentValues intent)
    {
        var name = string.IsNullOrWhiteSpace(appName)
            ? (string.IsNullOrWhiteSpace(intent.AppType) ? "Composer App" : intent.AppType)
            : appName;

        var sb = new StringBuilder();
        sb.AppendLine($"# {name}");
        sb.AppendLine();
        sb.AppendLine($"{intent.AppType} for {intent.PrimaryUser}.");
        sb.AppendLine();
        sb.AppendLine("## What this app does");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(intent.Workflow) ? "(no workflow described)" : intent.Workflow);
        sb.AppendLine();
        sb.AppendLine("## Platforms");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(intent.Platforms) ? "(no platforms specified)" : intent.Platforms);
        sb.AppendLine();
        sb.AppendLine("## Runtime");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(runtime) ? "(no runtime specified)" : runtime);
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(intent.Notes))
        {
            sb.AppendLine("## Additional context");
            sb.AppendLine();
            sb.AppendLine(intent.Notes);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // ── architecture.md ────────────────────────────────────────────────────

    public static string BuildArchitecture(
        ArchitectureBlueprint arch,
        string solutionTree,
        string whyThis,
        string agentPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Architecture");
        sb.AppendLine();
        sb.AppendLine("## Pattern");
        sb.AppendLine(DerivePattern(arch));
        sb.AppendLine();

        sb.AppendLine("## Modules");
        sb.AppendLine();
        foreach (var m in arch.Modules)
        {
            sb.AppendLine($"### {m.Label}");
            sb.AppendLine(string.IsNullOrWhiteSpace(m.Description) ? "(no description)" : m.Description);
            sb.AppendLine();
            sb.AppendLine("Connects to:");
            var outbound = arch.Edges.Where(e => e.FromId == m.Id).ToList();
            if (outbound.Count == 0)
            {
                sb.AppendLine("- (no outbound edges)");
            }
            else
            {
                foreach (var e in outbound)
                {
                    var to = arch.Modules.FirstOrDefault(x => x.Id == e.ToId);
                    sb.AppendLine($"- {(to.Label ?? e.ToId)} — {e.Label}");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Connections summary");
        sb.AppendLine();
        sb.AppendLine("| From | To | Relationship |");
        sb.AppendLine("|------|----|--------------|");
        foreach (var e in arch.Edges)
        {
            var from = arch.Modules.FirstOrDefault(m => m.Id == e.FromId).Label ?? e.FromId;
            var to   = arch.Modules.FirstOrDefault(m => m.Id == e.ToId).Label   ?? e.ToId;
            sb.AppendLine($"| {from} | {to} | {e.Label} |");
        }
        sb.AppendLine();

        sb.AppendLine("## Solution structure");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(solutionTree.TrimEnd());
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Why this fits");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(whyThis) ? "(no rationale provided)" : whyThis);
        sb.AppendLine();

        sb.AppendLine("## Agent guidance");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(agentPrompt) ? "(no agent guidance provided)" : agentPrompt);
        return sb.ToString();
    }

    /// <summary>One-sentence summary derived by walking the longest path
    /// through the module graph and naming the first hop. Used by the locked
    /// context card and as the architecture.md opener.</summary>
    public static string DeriveArchitectureSummary(ArchitectureBlueprint arch)
    {
        if (arch.Modules.IsDefaultOrEmpty || arch.Edges.IsDefaultOrEmpty)
            return "Empty blueprint.";

        var spineIds = ComputeLongestPath(arch);
        if (spineIds.Count < 2) return $"{arch.Modules.Length} modules with no outbound chain.";

        var labels = spineIds
            .Select(id => arch.Modules.FirstOrDefault(m => m.Id == id).Label ?? id)
            .ToList();

        // "Pages bind State (MVUX); Services consume HTTP and Storage."
        // Soften the verb at the second-to-last hop based on its outbound count.
        var first = labels[0];
        var rest = string.Join(" → ", labels.Skip(1));
        return $"{first} flows through {rest}.";
    }

    private static string DerivePattern(ArchitectureBlueprint arch)
    {
        if (arch.Modules.Any(m => m.Id == "mvux")) return "MVUX";
        if (arch.Modules.Any(m => m.Id == "mvvm")) return "MVVM";
        return "Custom";
    }

    /// <summary>Longest path through the directed edge graph. Tie-broken by
    /// insertion order so the result is deterministic across runs.</summary>
    private static List<string> ComputeLongestPath(ArchitectureBlueprint arch)
    {
        var byFrom = arch.Edges.GroupBy(e => e.FromId).ToDictionary(g => g.Key, g => g.ToList());
        var visited = new HashSet<string>();
        var best = new List<string>();

        foreach (var m in arch.Modules)
        {
            visited.Clear();
            var path = new List<string>();
            Walk(m.Id);
            if (path.Count > best.Count) best = new List<string>(path);

            void Walk(string id)
            {
                if (!visited.Add(id)) return;
                path.Add(id);
                if (byFrom.TryGetValue(id, out var outs))
                {
                    foreach (var e in outs) Walk(e.ToId);
                }
            }
        }

        return best;
    }

    // ── design-system.md ───────────────────────────────────────────────────

    public static string BuildDesignSystem(
        DesignTokens tokens,
        string colorPaletteOverrideXaml,
        string whyThis,
        string agentPrompt)
    {
        string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        var sb = new StringBuilder();
        sb.AppendLine("# Design system");
        sb.AppendLine();
        sb.AppendLine("## Color tokens");
        sb.AppendLine();
        sb.AppendLine("| Token   | Hex       | Material slot       | Themes      |");
        sb.AppendLine("|---------|-----------|---------------------|-------------|");
        sb.AppendLine($"| Surface | {Hex(tokens.Surface)} | BackgroundColor     | Dark        |");
        sb.AppendLine($"| Action  | {Hex(tokens.Action)}  | SecondaryColor      | Light, Dark |");
        sb.AppendLine($"| Info    | {Hex(tokens.Info)}    | (semantic)          | —           |");
        sb.AppendLine($"| Success | {Hex(tokens.Success)} | (semantic)          | —           |");
        sb.AppendLine($"| Warn    | {Hex(tokens.Warn)}    | ErrorColor          | Light, Dark |");
        sb.AppendLine($"| Panel   | {Hex(tokens.Panel)}   | SurfaceColor        | Dark        |");
        sb.AppendLine($"| Tag     | {Hex(tokens.Tag)}     | (semantic)          | —           |");
        sb.AppendLine($"| Locked  | {Hex(tokens.Locked)}  | (semantic)          | —           |");
        sb.AppendLine();

        sb.AppendLine("## Typography");
        sb.AppendLine();
        sb.AppendLine($"**Body font:** {tokens.BodyFont}");
        sb.AppendLine("**Mono font:** JetBrains Mono (variable)");
        sb.AppendLine();
        sb.AppendLine("| Level   | Size | Line height | Use                                    |");
        sb.AppendLine("|---------|------|-------------|----------------------------------------|");
        sb.AppendLine("| Display | 32   | 38          | Page titles, section headers           |");
        sb.AppendLine("| Heading | 24   | 30          | Subsection headers, item titles        |");
        sb.AppendLine("| Body    | 16   | 24          | Long-form prose, descriptions          |");
        sb.AppendLine("| Caption | 12   | 14          | Mono caps labels, metadata, timestamps |");
        sb.AppendLine();

        sb.AppendLine("## Spacing rhythm");
        sb.AppendLine();
        sb.AppendLine("4-point grid: 4, 8, 12, 16, 24, 32, 48px");
        sb.AppendLine();
        sb.AppendLine("Most spacing decisions land on the 4 / 8 / 16 / 24 marks. The 12 mark");
        sb.AppendLine("is reserved for in-between cases. 32 is the standard section gap; 48 is the marquee.");
        sb.AppendLine();

        sb.AppendLine("## Color override XAML");
        sb.AppendLine();
        sb.AppendLine("```xml");
        sb.AppendLine(colorPaletteOverrideXaml.TrimEnd());
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## Why this fits");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(whyThis) ? "(no rationale provided)" : whyThis);
        sb.AppendLine();

        sb.AppendLine("## Agent guidance");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(agentPrompt) ? "(no agent guidance provided)" : agentPrompt);
        return sb.ToString();
    }

    /// <summary>Synthesise the canonical Material color override file from
    /// the live design tokens. Spec: brief 05 §4.</summary>
    public static string BuildColorPaletteOverrideXaml(DesignTokens t)
    {
        string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        var sb = new StringBuilder();
        sb.AppendLine("<ResourceDictionary");
        sb.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        sb.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
        sb.AppendLine();
        sb.AppendLine("  <ResourceDictionary.ThemeDictionaries>");
        sb.AppendLine();
        sb.AppendLine("    <ResourceDictionary x:Key=\"Light\">");
        sb.AppendLine("      <Color x:Key=\"PrimaryColor\">#18181B</Color>");
        sb.AppendLine("      <Color x:Key=\"OnPrimaryColor\">#FAFAFA</Color>");
        sb.AppendLine($"      <Color x:Key=\"SecondaryColor\">{Hex(t.Action)}</Color>");
        sb.AppendLine("      <Color x:Key=\"OnSecondaryColor\">#18181B</Color>");
        sb.AppendLine("      <Color x:Key=\"SurfaceColor\">#FAFAFA</Color>");
        sb.AppendLine("      <Color x:Key=\"OnSurfaceColor\">#18181B</Color>");
        sb.AppendLine("      <Color x:Key=\"BackgroundColor\">#FFFFFF</Color>");
        sb.AppendLine($"      <Color x:Key=\"ErrorColor\">{Hex(t.Warn)}</Color>");
        sb.AppendLine("    </ResourceDictionary>");
        sb.AppendLine();
        sb.AppendLine("    <ResourceDictionary x:Key=\"Dark\">");
        sb.AppendLine("      <Color x:Key=\"PrimaryColor\">#FAFAFA</Color>");
        sb.AppendLine($"      <Color x:Key=\"OnPrimaryColor\">{Hex(t.Surface)}</Color>");
        sb.AppendLine($"      <Color x:Key=\"SecondaryColor\">{Hex(t.Action)}</Color>");
        sb.AppendLine("      <Color x:Key=\"OnSecondaryColor\">#18181B</Color>");
        sb.AppendLine($"      <Color x:Key=\"SurfaceColor\">{Hex(t.Panel)}</Color>");
        sb.AppendLine("      <Color x:Key=\"OnSurfaceColor\">#FAFAFA</Color>");
        sb.AppendLine($"      <Color x:Key=\"BackgroundColor\">{Hex(t.Surface)}</Color>");
        sb.AppendLine($"      <Color x:Key=\"ErrorColor\">{Hex(t.Warn)}</Color>");
        sb.AppendLine("    </ResourceDictionary>");
        sb.AppendLine();
        sb.AppendLine("  </ResourceDictionary.ThemeDictionaries>");
        sb.AppendLine("</ResourceDictionary>");
        return sb.ToString();
    }

    // ── interaction-spec.md ────────────────────────────────────────────────

    public static string BuildInteractionSpec(
        InteractionsMatrix matrix,
        string whyThis,
        string agentPrompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Interaction spec");
        sb.AppendLine();

        var stateCount = matrix.Flows.Sum(f => f.States.Length);
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine($"{matrix.Flows.Length} flows · 6 states per flow · {stateCount} state definitions total.");
        sb.AppendLine();
        sb.AppendLine("The agent must implement all 6 states per flow. Each state corresponds");
        sb.AppendLine("exactly to a VisualStateManager state name in the generated XAML.");
        sb.AppendLine();
        sb.AppendLine("State naming is fixed:");
        foreach (var k in InteractionFlow.StandardStates)
            sb.AppendLine($"- {k}");
        sb.AppendLine();

        foreach (var flow in matrix.Flows)
        {
            sb.AppendLine($"## Flow: {flow.Label}");
            sb.AppendLine();
            sb.AppendLine($"**VSM group:** {ToPascal(flow.Id)}StateGroup");
            sb.AppendLine();

            foreach (var state in flow.States)
            {
                sb.AppendLine($"### {state.Label}");
                sb.AppendLine();
                sb.AppendLine($"**Description:** {(string.IsNullOrWhiteSpace(state.Description) ? "(no description)" : state.Description)}");
                sb.AppendLine($"**VSM state name:** {state.Label}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Cross-flow rules");
        sb.AppendLine();
        sb.AppendLine("- Offline state must always be implementable — even if the flow technically requires");
        sb.AppendLine("  connectivity, the state describes how the UI degrades.");
        sb.AppendLine("- Loading and Default are both required even if the transition is instant; the agent");
        sb.AppendLine("  renders both states in VSM, sets `Default` as the initial state, and goes to `Loading`");
        sb.AppendLine("  for any non-zero async work.");
        sb.AppendLine("- Error states must be recoverable. If the error is fatal, the description must say so.");
        sb.AppendLine();

        sb.AppendLine("## Why this fits");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(whyThis) ? "(no rationale provided)" : whyThis);
        sb.AppendLine();

        sb.AppendLine("## Agent guidance");
        sb.AppendLine();
        sb.AppendLine(string.IsNullOrWhiteSpace(agentPrompt) ? "(no agent guidance provided)" : agentPrompt);
        return sb.ToString();
    }

    // ── helpers ────────────────────────────────────────────────────────────

    /// <summary>Synthesise the solution tree from the architecture blueprint
    /// and the platform list. Spec: brief 04 §5.</summary>
    public static string BuildSolutionTree(string appName, ImmutableArray<string> platformHeads)
    {
        var name = SanitizeName(appName);
        var sb = new StringBuilder();
        sb.AppendLine($"{name}/");
        sb.AppendLine($"├── {name}/                   # shared");
        sb.AppendLine($"│   ├── Models/");
        sb.AppendLine($"│   ├── Presentation/                # MVUX feeds");
        sb.AppendLine($"│   ├── Services/");
        sb.AppendLine($"│   ├── Views/");
        sb.AppendLine($"│   └── App.xaml");
        if (!platformHeads.IsDefaultOrEmpty)
        {
            for (var i = 0; i < platformHeads.Length; i++)
            {
                var head = platformHeads[i];
                var prefix = i == platformHeads.Length - 1 ? "└──" : "├──";
                sb.AppendLine($"{prefix} {name}.{head}/");
            }
        }
        sb.AppendLine($"└── {name}.Tests/");
        return sb.ToString();
    }

    private static string SanitizeName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "ComposerApp";
        var b = new StringBuilder(s.Length);
        foreach (var c in s)
            if (char.IsLetterOrDigit(c)) b.Append(c);
        return b.Length == 0 ? "ComposerApp" : b.ToString();
    }

    private static string ToPascal(string id)
    {
        if (string.IsNullOrEmpty(id)) return "";
        var sb = new StringBuilder(id.Length);
        var capitalize = true;
        foreach (var c in id)
        {
            if (c == '-' || c == '_' || c == ' ') { capitalize = true; continue; }
            sb.Append(capitalize ? char.ToUpperInvariant(c) : c);
            capitalize = false;
        }
        return sb.ToString();
    }
}

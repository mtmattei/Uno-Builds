using System.Collections.Immutable;
using Windows.Foundation;

namespace Composer.Models;

/// <summary>
/// Layer 2 — Architecture. A blueprint of named module nodes connected by
/// labeled edges. Brief 01 includes only the data shape; the SVG-style
/// canvas rendering lives in brief 02.
/// </summary>
public record ArchitectureBlueprint(
    ImmutableArray<ModuleDef> Modules,
    ImmutableArray<EdgeDef> Edges)
{
    public static ArchitectureBlueprint Empty { get; } =
        new(ImmutableArray<ModuleDef>.Empty, ImmutableArray<EdgeDef>.Empty);

    /// <summary>Default blueprint reflecting the recommended Uno baseline —
    /// MVUX state, region-based navigation, Kiota HTTP, offline-first storage.
    /// Module positions sized so the rightmost column ends well within the
    /// 780×340 canvas (module width 180 → rightmost end at X=740).</summary>
    public static ArchitectureBlueprint Default { get; } = new(
        Modules: ImmutableArray.Create(
            new ModuleDef("pages",    "Pages",        new Point( 20,  70), "ink",    "Route surfaces", Files: 5),
            new ModuleDef("nav",      "Navigation",   new Point( 20, 220), "ink",    "Region-based routes", Files: 2),
            new ModuleDef("mvux",     "State (MVUX)", new Point(290,  70), "indigo", "Feeds, States, Selection", Files: 6),
            new ModuleDef("services", "Services",     new Point(290, 220), "ink2",   "Business logic", Files: 3),
            new ModuleDef("http",     "HTTP",         new Point(560,  70), "ink2",   "Typed clients", Files: 2),
            new ModuleDef("storage",  "Storage",      new Point(560, 220), "ink2",   "Local cache, offline-first", Files: 2)),
        Edges: ImmutableArray.Create(
            new EdgeDef("pages",    "mvux",     "binds"),
            new EdgeDef("pages",    "nav",      "requests"),
            new EdgeDef("mvux",     "services", "consumes"),
            new EdgeDef("services", "http",     "calls"),
            new EdgeDef("services", "storage",  "persists")));
}

public partial record ModuleDef(
    string Id,
    string Label,
    Point Position,
    string ColorKey,    // "ink" | "ink2" | "indigo" | "amber" — looked up at render time
    string Description,
    int Files = 0);     // count badge shown top-right on hover (v11 brief §5.3)

public partial record EdgeDef(string FromId, string ToId, string Label);

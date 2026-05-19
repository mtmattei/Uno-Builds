using System.Collections.Immutable;

namespace Composer.Models;

/// <summary>
/// Layer 3 — UX. A primary flow consisting of an ordered set of screens.
/// Brief 02 expands the canvas to multiple flows with tabs; brief 01 keeps
/// the shape simple.
/// </summary>
public record UXFlow(string Name, ImmutableArray<ScreenDef> Screens)
{
    public static UXFlow Empty { get; } = new("", ImmutableArray<ScreenDef>.Empty);

    public static UXFlow Default { get; } = new(
        Name: "Dispatch a job",
        Screens: ImmutableArray.Create(
            new ScreenDef("Dashboard", "Today's jobs",   ImmutableArray.Create(UIBlock.Full, UIBlock.Half, UIBlock.Full)),
            new ScreenDef("Job detail", "Status, location", ImmutableArray.Create(UIBlock.Half, UIBlock.Full, UIBlock.Half)),
            new ScreenDef("Schedule",   "Drag-to-reorder",  ImmutableArray.Create(UIBlock.Full, UIBlock.Half, UIBlock.Full)),
            new ScreenDef("Dispatch",   "Confirm + assign", ImmutableArray.Create(UIBlock.Quarter, UIBlock.Half, UIBlock.Full))));
}

public record ScreenDef(
    string Name,
    string Note,
    ImmutableArray<UIBlock> MockBlocks);

public enum UIBlock { Full, ThreeQuarters, Half, Quarter }

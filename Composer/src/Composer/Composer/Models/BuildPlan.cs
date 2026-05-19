using System.Collections.Immutable;

namespace Composer.Models;

/// <summary>
/// Layer 7 — Implementation. A linear list of build phases, each with files
/// and a string dependency. Future briefs may enforce a real DAG.
/// </summary>
public record BuildPlan(ImmutableArray<PhaseDef> Phases)
{
    public static BuildPlan Empty { get; } = new(ImmutableArray<PhaseDef>.Empty);

    public static BuildPlan Default { get; } = new(
        ImmutableArray.Create(
            new PhaseDef(1, "Scaffold", ImmutableArray.Create("*.csproj", "App.xaml", ".mcp.json"), "—"),
            new PhaseDef(2, "Shell",    ImmutableArray.Create("MainPage", "Navigation", "Themes/"),  "Scaffold"),
            new PhaseDef(3, "Domain",   ImmutableArray.Create("Models/Job.cs", "Services/JobSvc"),   "Shell"),
            new PhaseDef(4, "Screens",  ImmutableArray.Create("Calendar.xaml", "JobDetail.xaml"),    "Domain"),
            new PhaseDef(5, "States",   ImmutableArray.Create("Loading, empty, error variants"),    "Screens"),
            new PhaseDef(6, "Polish",   ImmutableArray.Create("Tests, a11y, perf"),                 "States")));
}

public record PhaseDef(
    int Number,
    string Label,
    ImmutableArray<string> Files,
    string After);

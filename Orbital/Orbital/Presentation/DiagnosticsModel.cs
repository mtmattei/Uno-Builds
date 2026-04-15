using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record DiagnosticsModel(IDiagnosticsService Diagnostics, IProjectContext Ctx)
{
    public IFeed<ImmutableList<DiagnosticsCheck>> Checks => Feed.Async(Diagnostics.GetChecksAsync);
    public IFeed<ImmutableList<DependencyInfo>> Dependencies => Feed.Async(Diagnostics.GetDependenciesAsync);
    public IFeed<ImmutableList<PlatformTarget>> Platforms => Feed.Async(Diagnostics.GetPlatformTargetsAsync);
    public IFeed<ImmutableList<RuntimeTool>> Tools => Feed.Async(Diagnostics.GetRuntimeToolsAsync);

    public IFeed<string> HeaderSubtitle => Feed.Async(ct =>
    {
        var active = Ctx.ActiveProject;
        return ValueTask.FromResult(active is not null
            ? $"{active.Name} \u00B7 Environment verification and dependency checks"
            : "Environment verification, dependency checks, and runtime tools");
    });

    public IFeed<string> CheckSummary => Checks.Select(checks =>
    {
        var warn = checks.Count(c => c.Status == HealthStatus.Warn);
        var err = checks.Count(c => c.Status == HealthStatus.Error);
        if (err > 0) return $"{err} error{(err > 1 ? "s" : "")}";
        if (warn > 0) return $"{warn} warning{(warn > 1 ? "s" : "")}";
        return "all passed";
    });
}

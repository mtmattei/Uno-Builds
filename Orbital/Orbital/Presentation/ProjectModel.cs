using Orbital.Helpers;
using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record ProjectModel(IBuildService Build, IEnvironmentService Env, IProjectContext Ctx)
{
    public IFeed<ProjectMeta> Meta => Feed.Async(Env.GetCurrentProjectAsync);
    public IFeed<ImmutableList<Controls.ConsoleLine>> BuildOutput => Feed.Async(Build.GetLastBuildOutputAsync);
    public IFeed<ImmutableList<Artifact>> Artifacts => Feed.Async(Build.GetArtifactsAsync);

    // ── Meta strip feeds ──────────────────────────────────────────────
    public IFeed<string> SolutionName => Feed.Async(ct =>
    {
        var root = Ctx.ActiveProject?.RootDirectory ?? HostHelper.FindProjectRoot() ?? AppContext.BaseDirectory;
        var sln = Directory.GetFiles(root, "*.sln").FirstOrDefault();
        if (sln is not null) return new ValueTask<string>(Path.GetFileName(sln));
        var csproj = Directory.GetFiles(root, "*.csproj").FirstOrDefault();
        return new ValueTask<string>(csproj is not null ? Path.GetFileName(csproj) : (Ctx.ActiveProject?.Name ?? "Orbital"));
    });

    public IFeed<string> SolutionDetail => Feed.Async(ct =>
    {
        var root = Ctx.ActiveProject?.RootDirectory ?? HostHelper.FindProjectRoot() ?? AppContext.BaseDirectory;
        var hasSln = Directory.GetFiles(root, "*.sln").Length > 0;
        if (!hasSln)
        {
            var hasCsproj = Directory.GetFiles(root, "*.csproj").Length > 0;
            return new ValueTask<string>(hasCsproj ? "Single project" : "No projects");
        }
        var projCount = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories).Length;
        return new ValueTask<string>($"{projCount} project{(projCount != 1 ? "s" : "")}");
    });

    public IFeed<string> TargetsDisplay => Feed.Async(ct =>
    {
        var csproj = HostHelper.FindCsproj(Ctx.ActiveProject?.RootDirectory);
        if (csproj is null) return new ValueTask<string>("Desktop");
        var (shortNames, _) = HostHelper.ReadTargetFrameworks(csproj);
        var display = string.Join(", ", shortNames.Select(n => char.ToUpperInvariant(n[0]) + n[1..]));
        return new ValueTask<string>(display);
    });

    public IFeed<string> TargetsDetail => Feed.Async(ct =>
    {
        var csproj = HostHelper.FindCsproj(Ctx.ActiveProject?.RootDirectory);
        if (csproj is null) return new ValueTask<string>("Single target");
        var (_, fullTfms) = HostHelper.ReadTargetFrameworks(csproj);
        return new ValueTask<string>($"{fullTfms.Length} target{(fullTfms.Length != 1 ? "s" : "")}");
    });

    public IFeed<string> BuildConfig => Feed.Async(ct =>
    {
#if DEBUG
        return new ValueTask<string>("Debug");
#else
        return new ValueTask<string>("Release");
#endif
    });

    public IFeed<string> BuildDetail => Feed.Async(ct =>
        new ValueTask<string>($".NET {Environment.Version.ToString(2)}"));

    public IFeed<string> BranchName => Feed.Async(ct =>
        new ValueTask<string>(Ctx.ActiveProject?.Branch ?? "main"));

    public IFeed<string> BranchDetail => Feed.Async(ct =>
    {
        var root = Ctx.ActiveProject?.RootDirectory;
        return new ValueTask<string>(root is not null ? Path.GetFileName(root) : "Local");
    });

    public IFeed<string> HotReloadStatus => Feed.Async(ct =>
    {
        var hrEnv = Environment.GetEnvironmentVariable("DOTNET_MODIFIABLE_ASSEMBLIES");
        return new ValueTask<string>(hrEnv is not null ? "Active" : "Standby");
    });

    public IFeed<string> HotReloadDetail => Feed.Async(ct =>
    {
        var hrEnv = Environment.GetEnvironmentVariable("DOTNET_MODIFIABLE_ASSEMBLIES");
        return new ValueTask<string>(hrEnv is not null ? "XAML + C#" : "Set DOTNET_MODIFIABLE_ASSEMBLIES");
    });

    // ── Header subtitle feed ──────────────────────────────────────────
    public IFeed<string> HeaderSubtitle => Feed.Async(ct =>
    {
        var active = Ctx.ActiveProject;
        if (active is not null)
            return new ValueTask<string>($"{active.Name} \u00B7 {active.RootDirectory} \u00B7 {active.Branch ?? "main"}");
        var asm = System.Reflection.Assembly.GetEntryAssembly();
        var name = asm?.GetName().Name ?? "Orbital";
        var rootDir = HostHelper.FindProjectRoot() ?? AppContext.BaseDirectory.TrimEnd('\\', '/');
        return new ValueTask<string>($"{name} \u00B7 {rootDir}");
    });

    // ── Console title feeds ───────────────────────────────────────────
    public IFeed<string> BuildConsoleTitle => Feed.Async(ct =>
    {
        var tfm = GetCurrentTfm();
        return new ValueTask<string>($"build \u2192 {tfm}");
    });

    public IFeed<string> RunConsoleTitle => Feed.Async(ct =>
    {
        var tfm = GetCurrentTfm();
        return new ValueTask<string>($"run \u2192 {tfm}");
    });

    private string GetCurrentTfm()
    {
        var csproj = HostHelper.FindCsproj(Ctx.ActiveProject?.RootDirectory);
        if (csproj is not null)
        {
            var (_, fullTfms) = HostHelper.ReadTargetFrameworks(csproj);
            var desktop = fullTfms.FirstOrDefault(t => t.Contains("desktop")) ?? fullTfms[0];
            return desktop;
        }
        return $"net{Environment.Version.ToString(2)}-desktop";
    }
}

using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record SettingsModel(IEnvironmentService Env)
{
    public IFeed<VersionInfo> Versions => Feed.Async(Env.GetVersionInfoAsync);

    public IFeed<EnvironmentStatus> EnvStatus => Feed.Async(Env.GetStatusAsync);

    public IFeed<string> VersionDisplay => Feed.Async(async ct =>
        $"v{(await Env.GetVersionInfoAsync(ct)).OrbitalVersion}");

    public IFeed<string> DotNetDisplay => Feed.Async(async ct =>
        $".NET {(await Env.GetStatusAsync(ct)).DotNetVersion}");

    public IFeed<string> PlatformInfo => Feed.Async(ct =>
        ValueTask.FromResult(System.Runtime.InteropServices.RuntimeInformation.OSDescription));

    public IFeed<string> ProjectRoot => Feed.Async(ct =>
        ValueTask.FromResult(Helpers.HostHelper.FindProjectRoot() ?? AppContext.BaseDirectory));

    public IFeed<string> RecentsPath => Feed.Async(ct =>
        ValueTask.FromResult(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Orbital", "recent-projects.json")));

    public IFeed<string> SkillsPath => Feed.Async(ct =>
        ValueTask.FromResult(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude", "skills")));
}

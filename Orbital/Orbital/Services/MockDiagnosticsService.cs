namespace Orbital.Services;

public class MockDiagnosticsService : IDiagnosticsService
{
    public ValueTask<ImmutableList<DiagnosticsCheck>> GetChecksAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new DiagnosticsCheck(".NET SDK", "10.0.3 installed", HealthStatus.Ok),
            new DiagnosticsCheck("Uno.Sdk", "6.5.31 installed", HealthStatus.Ok),
            new DiagnosticsCheck("Android SDK", "API 35 (Build-tools 35.0.0)", HealthStatus.Ok),
            new DiagnosticsCheck("Xcode", "Not installed", HealthStatus.Warn),
            new DiagnosticsCheck("OpenJDK", "21.0.2 installed", HealthStatus.Ok),
            new DiagnosticsCheck("GTK", "3.24.41 installed", HealthStatus.Ok)));

    public ValueTask<ImmutableList<DependencyInfo>> GetDependenciesAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new DependencyInfo("Uno.WinUI", "6.5.31", "6.5.31", HealthStatus.Ok),
            new DependencyInfo("Uno.Extensions.Navigation", "5.2.170", "5.2.170", HealthStatus.Ok),
            new DependencyInfo("Uno.Extensions.Reactive", "5.2.170", "5.2.170", HealthStatus.Ok),
            new DependencyInfo("Uno.Toolkit.UI", "7.1.12", "7.1.12", HealthStatus.Ok),
            new DependencyInfo("Uno.Toolkit.UI.Material", "7.1.12", "7.1.12", HealthStatus.Ok),
            new DependencyInfo("Microsoft.Extensions.Hosting", "10.0.0", "10.0.0", HealthStatus.Ok),
            new DependencyInfo("Uno.Resizetizer", "6.5.31", "6.5.31", HealthStatus.Ok),
            new DependencyInfo("SkiaSharp", "3.119.0", "3.119.0", HealthStatus.Ok)));

    public ValueTask<ImmutableList<PlatformTarget>> GetPlatformTargetsAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new PlatformTarget("Desktop (Skia)", ".NET 10.0", true, HealthStatus.Ok),
            new PlatformTarget("WebAssembly", ".NET 10.0", true, HealthStatus.Ok),
            new PlatformTarget("Android", "API 35", true, HealthStatus.Ok),
            new PlatformTarget("iOS", "Xcode 16", false, HealthStatus.Warn),
            new PlatformTarget("macOS (Catalyst)", "Xcode 16", false, HealthStatus.Warn)));

    public ValueTask<ImmutableList<RuntimeTool>> GetRuntimeToolsAsync(CancellationToken ct) =>
        ValueTask.FromResult(ImmutableList.Create(
            new RuntimeTool("Hot Reload", "Live XAML and C# updates", "Active", HealthStatus.Ok, "emerald"),
            new RuntimeTool("XAML Inspector", "Runtime visual tree browser", "Ready", HealthStatus.Ok, "blue"),
            new RuntimeTool("Screenshot", "Capture app screenshots via MCP", "Ready", HealthStatus.Ok, "violet"),
            new RuntimeTool(".NET SDK", "Build and runtime tools", "10.0.3", HealthStatus.Ok, "blue"),
            new RuntimeTool("Hot Design", "Visual designer integration", "Active", HealthStatus.Ok, "emerald"),
            new RuntimeTool("Performance Profiler", "Frame timing analysis", "Idle", HealthStatus.Idle, "amber")));
}

namespace Orbital.Models;

// Enums
public enum SessionStatus { Active, Paused, Done }
public enum ActionStatus { Ok, Warn, Error, Idle }
public enum HealthStatus { Ok, Warn, Error, Idle }

// Home
public record EnvironmentStatus(string UnoSdkVersion, string DotNetVersion, int WorkloadCount, string Renderer);
public record StudioStatus(string Version, string Tier, string AccountName, string AccountEmail, DateOnly Expiry, bool IsValid);
public record McpStatus(bool Connected, int ServerCount, int ToolCount, ImmutableList<McpServer> Servers);
public record McpServer(string Name, string Url, bool Healthy, int ToolCount);
public record VersionInfo(string OrbitalVersion, string UnoSdkVersion, string DotNetVersion, string LlmModel, int McpServerCount);
public record RecentProject(string Name, string Path, string Branch, string LastBuild, HealthStatus Status);

// Project
public record ProjectMeta(string Name, string Path, string Branch, string TargetFramework, string LastBuildTime, HealthStatus BuildStatus);
public record ProjectCreateResult(bool Success, string ProjectPath, string Output);
public record Artifact(string FileName, string Type, string Size, string Path, DateTime Created);

// Agents
public partial record AgentSession(
    string Id, string Name, string Repo, string Branch, string Goal,
    SessionStatus Status, int ActionCount, int ArtifactCount,
    ImmutableList<AgentAction> Actions, DateTime StartTime);
public record AgentAction(DateTime Time, string Title, string Detail, ActionStatus Status);

// Diagnostics
public record DiagnosticsCheck(string Name, string Detail, HealthStatus Status);
public record DependencyInfo(string Package, string CurrentVersion, string LatestVersion, HealthStatus Status);
public record PlatformTarget(string Name, string Sdk, bool IsInstalled, HealthStatus Status);
public record RuntimeTool(string Name, string Description, string Version, HealthStatus Status, string AccentColor);

// Studio
public record StudioFeature(string Name, string Description, bool IsEnabled, string Tier);
public record McpConnector(string Name, string Url, bool Connected, int ToolCount, string Status);

// Skills
public partial record SkillInfo(
    string Id, string Name, string Description, string Category,
    bool IsActive, int Invocations, double Accuracy, string Path);

// Project Context
public record OrbitalProject(
    string Name,
    string SolutionPath,
    string RootDirectory,
    string? Branch,
    DateTime LastOpened,
    HealthStatus Status);

// Search
public record SearchResult(string Name, string Description, string Category, string Route, string Icon);

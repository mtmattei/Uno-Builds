namespace Sanctum.Models;

/// <summary>
/// Application modes for context-based UI switching
/// </summary>
public enum AppMode
{
    Explore,
    Focus,
    Recover
}

/// <summary>
/// View modes for app navigation state
/// </summary>
public enum ViewMode
{
    Onboarding,
    Dashboard
}

/// <summary>
/// Status for source control items
/// </summary>
public enum SourceStatus
{
    Allowed,
    Batched,
    Muted
}

/// <summary>
/// User preferences collected during onboarding
/// </summary>
public class UserPreferences
{
    public List<string> Goals { get; set; } = [];
    public List<string> Sources { get; set; } = [];
}

/// <summary>
/// AI-generated sanity plan
/// </summary>
public class SanityPlan
{
    public string Manifesto { get; set; } = string.Empty;
    public List<string> Rules { get; set; } = [];
}

/// <summary>
/// Feed item for Signal Feed
/// </summary>
public class FeedItem
{
    public string Source { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Digest item for Daily Digest
/// </summary>
public class DigestItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
}

/// <summary>
/// Source control item
/// </summary>
public class SourceControlItem
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public SourceStatus Status { get; set; } = SourceStatus.Allowed;
}

/// <summary>
/// Goal option for onboarding
/// </summary>
public class GoalOption
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

/// <summary>
/// Source option for onboarding
/// </summary>
public class SourceOption
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

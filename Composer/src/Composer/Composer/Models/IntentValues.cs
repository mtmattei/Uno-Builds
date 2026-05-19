namespace Composer.Models;

/// <summary>
/// Layer 1 — Intent. Captures what the app is for in four named fields plus
/// an optional notes textarea.
/// Spec: <c>docs/DESIGN-BRIEF.md</c> §6.
///
/// Defaults pre-populate the field-service-scheduling archetype (the running
/// example in the brief). On AI-seeded preview, these are replaced with
/// archetype-appropriate values inferred from the hero's AppName /
/// AppDescription / Platforms.
/// </summary>
public record IntentValues(
    string AppType,
    string PrimaryUser,
    string Workflow,
    string Platforms,
    string Notes)
{
    public static IntentValues Default { get; } = new(
        AppType:     "Field-service scheduling",
        PrimaryUser: "Mobile-first technicians",
        Workflow:    "Receive jobs, schedule, dispatch",
        Platforms:   "Web, iOS, Android",
        Notes:       string.Empty);
}

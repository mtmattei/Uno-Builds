namespace Composer.Models;

/// <summary>
/// Domain context distilled from <see cref="IntentValues"/>. Used across
/// downstream layer models (UX, Architecture, Interactions, Data,
/// Implementation, Scaffold) to keep generated content stack- and
/// domain-correct without depending on AI augmentation.
///
/// Per <c>docs/ARCHITECTURE-BRIEF-from-scratch.md</c> §10.2.
/// </summary>
public record DerivedContext(
    string AppName,
    string EntityNoun,
    string EntityTitle,
    string EntityPlural,
    string UserNoun,
    bool   IsOfflineFirst,
    bool   IsMobileFirst,
    bool   IsFieldService);

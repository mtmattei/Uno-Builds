namespace Composer.Models;

/// <summary>
/// Per-layer rollback snapshot. Captured when a layer transitions
/// Clean → Dirty so DiscardEdits / DiscardPreview can restore the
/// pre-edit values. Only the relevant slice is populated for any one
/// layer's snapshot — the rest are null.
/// </summary>
public record LayerSnapshot(
    StackPreferences? Stack           = null,
    IntentValues? Intent              = null,
    ArchitectureBlueprint? Arch       = null,
    UXFlow? UX                        = null,
    DesignTokens? Design              = null,
    InteractionsMatrix? Interactions  = null,
    DataContracts? Data               = null,
    BuildPlan? Implementation         = null);

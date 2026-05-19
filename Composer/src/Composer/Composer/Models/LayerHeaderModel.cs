namespace Composer.Models;

/// <summary>
/// Bound to <see cref="Composer.Views.Controls.ActiveLayerHeader"/>. Combines
/// the active layer's static metadata with its current run-time state for the
/// "Edited — preview pending" / "Preview — review and accept" badge.
/// </summary>
public record LayerHeaderModel(
    string LayerLabel,   // "02 · Architecture"
    string Title,
    string Subtitle,
    LayerState State);

namespace Composer.Models;

/// <summary>
/// Result of <see cref="Composer.Services.ILayerPreviewService.GeneratePreviewAsync"/>.
/// <see cref="ProposedValues"/> is typed per-layer (the canvas casts at the
/// consumption site). <see cref="Summary"/> is a one-line italic-serif headline
/// shown in the composer footer.
/// </summary>
public record LayerPreviewResult(object ProposedValues, string Summary);

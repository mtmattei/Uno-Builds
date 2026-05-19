using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Composer.Models;

namespace Composer.Services;

/// <summary>
/// Two AI entry points per layer:
///   1. <see cref="GenerateInitialAsync"/> — kicked off when the user submits
///      the hero so each layer's canvas has app-aware initial content rather
///      than the static <c>Default</c> values.
///   2. <see cref="GeneratePreviewAsync"/> — runs when the user types in the
///      composer prompt and clicks "Generate preview →". Refines the current
///      layer's state with the user's edit + locked context summaries.
/// </summary>
public interface ILayerPreviewService
{
    /// <summary>Returns null when the layer isn't AI-seeded or the call
    /// fails — caller falls back to the layer's hardcoded <c>Default</c>.
    /// When <paramref name="screenshotPaths"/> is non-empty, the UX and
    /// DesignSystem layers route through Anthropic's vision API so the AI
    /// can see the reference images.</summary>
    Task<object?> GenerateInitialAsync(
        LayerKind kind,
        string appName,
        string description,
        string platforms,
        string runtime,
        ImmutableArray<string> screenshotPaths,
        CancellationToken ct = default);

    Task<LayerPreviewResult> GeneratePreviewAsync(
        LayerKind kind,
        object currentValues,
        string userPrompt,
        IReadOnlyDictionary<LayerKind, string> lockedContextSummaries,
        CancellationToken ct = default);
}

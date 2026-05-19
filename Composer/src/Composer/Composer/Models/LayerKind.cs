namespace Composer.Models;

/// <summary>
/// The eight layers of the context-engine composition. Order is fixed and
/// load-bearing — each layer's locked context becomes input for subsequent
/// layers. Underlying integer value matches position in the composition
/// stack so <c>(int)LayerKind</c> ordering is safe.
///
/// Stack preferences are *not* a layer — they ship as static defaults
/// baked into <c>LayerMarkdownTemplates.BuildStackPreferences</c> bundle
/// output. The architecture brief's Stack-at-0 design is superseded by
/// the prototype, which puts Intent at the cold-launch surface.
/// </summary>
public enum LayerKind
{
    Intent         = 0,
    UX             = 1,
    Architecture   = 2,
    DesignSystem   = 3,
    Interactions   = 4,
    Data           = 5,
    Implementation = 6,
    Scaffold       = 7,
}

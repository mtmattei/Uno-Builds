namespace Composer.Models;

/// <summary>
/// Composer-footer eyebrow status strings. Per the v11 interaction brief,
/// the eyebrow next to <c>COMPOSER ·</c> reads "REFINING" / "LISTENING" /
/// "PROPOSING" depending on the active layer's state.
/// </summary>
public static class ComposerStatus
{
    public const string Clean      = "REFINING";
    public const string Dirty      = "LISTENING";
    public const string Previewing = "PROPOSING";

    public static string ForLayerState(LayerState s) => s switch
    {
        LayerState.Dirty       => Dirty,
        LayerState.Previewing  => Previewing,
        _                      => Clean,
    };
}

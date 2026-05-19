namespace Composer.Models;

/// <summary>
/// Per-layer state machine. Each layer carries one of these independently —
/// editing one layer never affects another's state. See brief 01 §2.
/// </summary>
public enum LayerState
{
    /// <summary>No edits since lock. Lock-and-continue is the action.</summary>
    Clean,

    /// <summary>User edited canvas or typed in prompt. Generate-preview is the action.</summary>
    Dirty,

    /// <summary>AI rendered proposed redraw. Accept-or-discard.</summary>
    Previewing,

    /// <summary>Terminal — layer settled, file drafted, advanced past.</summary>
    Locked,
}

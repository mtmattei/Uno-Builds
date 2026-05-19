namespace Composer.Models;

/// <summary>
/// Per-file emission state shown in the right Files Rail. See brief 01 §5.
/// </summary>
public enum FileStatus
{
    /// <summary>Hollow ring; layer hasn't been active yet.</summary>
    Planned,

    /// <summary>Indigo dot, pulsing — the layer is the currently-active one.</summary>
    Writing,

    /// <summary>Amber dot — layer locked, file content settled.</summary>
    Drafted,
}

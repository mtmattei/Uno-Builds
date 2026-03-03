namespace UnoVox.Configuration;

/// <summary>
/// Configuration for voxel editor UI and 3D navigation.
/// </summary>
public class VoxelEditorConfig
{
    /// <summary>Voxel grid size (NxNxN cubic grid)</summary>
    public int GridSize { get; set; } = 32;

    /// <summary>Default voxel rendering size in pixels</summary>
    public int DefaultVoxelSize { get; set; } = 8;

    /// <summary>Minimum allowed voxel size</summary>
    public int MinVoxelSize { get; set; } = 4;

    /// <summary>Maximum allowed voxel size</summary>
    public int MaxVoxelSize { get; set; } = 24;

    /// <summary>
    /// AR plane depth for hand-to-voxel mapping.
    /// Represents the Z-coordinate of the virtual drawing plane in grid space.
    /// </summary>
    public float ArPlaneDepth { get; set; } = 16f;

    // Camera Navigation Settings
    /// <summary>Sensitivity multiplier for two-hand pan gesture</summary>
    public float TwoHandPanSensitivity { get; set; } = 800f;

    /// <summary>Sensitivity multiplier for two-hand pinch-to-zoom gesture</summary>
    public float TwoHandZoomSensitivity { get; set; } = 200f;

    /// <summary>Minimum hand distance change to trigger zoom (normalized 0-1)</summary>
    public float TwoHandMinDistanceDelta { get; set; } = 0.005f;
}

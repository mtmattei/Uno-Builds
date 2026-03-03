namespace UnoVox.Models;

/// <summary>
/// Represents the data structure for saving/loading voxel projects
/// </summary>
public class VoxelProject
{
    public string Version { get; set; } = "1.0";
    public int GridSize { get; set; } = 32;
    public List<VoxelData> Voxels { get; set; } = new();
    public List<string> Palette { get; set; } = new();
    public CameraData? Camera { get; set; }
    public ProjectMetadata? Metadata { get; set; }
}

/// <summary>
/// Voxel data for serialization
/// </summary>
public class VoxelData
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public string Color { get; set; } = "#FFFFFF";
}

/// <summary>
/// Camera state for serialization
/// </summary>
public class CameraData
{
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float Zoom { get; set; }
    public float PanX { get; set; }
    public float PanY { get; set; }
}

/// <summary>
/// Project metadata
/// </summary>
public class ProjectMetadata
{
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public string Author { get; set; } = "Unknown";
}

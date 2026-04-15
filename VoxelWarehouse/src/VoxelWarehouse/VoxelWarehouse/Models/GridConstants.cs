namespace VoxelWarehouse.Models;

public static class GridConstants
{
    public const int GridSize = 14;
    public const int MaxHeight = 6;
    public const int HalfWidth = 22;
    public const int HalfHeight = 11;
    public const int VoxelDepth = 18;

    /// <summary>Weight in tons per asset type.</summary>
    public static double GetWeight(AssetType asset) => asset switch
    {
        AssetType.Pallet => 1.2,
        AssetType.Rack => 0.4,
        AssetType.Container => 2.5,
        AssetType.Equipment => 3.0,
        AssetType.Aisle => 0.0,
        _ => 0.0
    };

    /// <summary>Single-char label stamp for voxel top face.</summary>
    public static string GetLabel(AssetType asset) => asset switch
    {
        AssetType.Pallet => "P",
        AssetType.Rack => "R",
        AssetType.Container => "C",
        AssetType.Equipment => "E",
        _ => ""
    };
}

namespace VoxelWarehouse.Models;

public readonly record struct Landmark3D(float X, float Y, float Z);

public record VoxelCell(int X, int Y, int Z, AssetType Asset);

public record VoxelWorldState(
    IImmutableDictionary<(int X, int Y, int Z), AssetType> Cells,
    IImmutableDictionary<(int X, int Z), ZoneType> Zones,
    int ActiveLayer,
    AssetType ActiveAsset,
    ToolMode ActiveTool,
    EditorMode ActiveMode)
{
    public static VoxelWorldState Empty() =>
        new(
            ImmutableDictionary<(int X, int Y, int Z), AssetType>.Empty,
            ImmutableDictionary<(int X, int Z), ZoneType>.Empty,
            ActiveLayer: 0,
            ActiveAsset: AssetType.Pallet,
            ActiveTool: ToolMode.Place,
            ActiveMode: EditorMode.Build);
}

public record HandTrackingResult(
    bool HandDetected,
    float Confidence,
    Landmark3D[] Landmarks,
    GestureType Gesture,
    float CursorX,
    float CursorY,
    bool IsLeftHand)
{
    public static HandTrackingResult None() =>
        new(false, 0f, [], GestureType.None, 0, 0, false);
}

public record UtilizationMetrics(
    double FloorUtilPercent,
    double VolumeUtilPercent,
    double TotalWeightTons,
    int PeakHeight,
    int TotalUnits,
    IImmutableDictionary<AssetType, int> AssetCounts,
    IImmutableDictionary<ZoneType, int> ZoneCellCounts);

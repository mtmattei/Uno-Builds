namespace GridForm.Models;

public record WarehouseState
{
	public const int GridWidth = 14;
	public const int GridDepth = 14;
	public const int MaxLayers = 6;

	public Dictionary<VoxelKey, AssetType> Voxels { get; init; } = new();
	public Dictionary<(int X, int Z), ZoneType> Zones { get; init; } = new();
	public int CurrentLayer { get; init; }
	public AssetType CurrentAsset { get; init; } = AssetType.Pallet;
	public WarehouseTool CurrentTool { get; init; } = WarehouseTool.Place;
	public WarehouseMode CurrentMode { get; init; } = WarehouseMode.Build;
	public (int X, int Z)? CursorPosition { get; init; }

	public WarehouseMetrics ComputeMetrics()
	{
		var totalFloorCells = GridWidth * GridDepth;
		var occupiedFloor = Voxels.Keys
			.Select(v => (v.X, v.Z))
			.Distinct()
			.Count();
		var totalVolume = GridWidth * GridDepth * MaxLayers;
		var usedVolume = Voxels.Count;

		var assetCounts = Voxels.Values
			.GroupBy(a => a)
			.ToImmutableDictionary(g => g.Key, g => g.Count());

		return new WarehouseMetrics(
			FloorUtilization: totalFloorCells > 0 ? (double)occupiedFloor / totalFloorCells : 0,
			VolumeUtilization: totalVolume > 0 ? (double)usedVolume / totalVolume : 0,
			TotalUnits: Voxels.Count,
			PeakHeight: Voxels.Count > 0 ? Voxels.Keys.Max(v => v.Y) + 1 : 0,
			AssetCounts: assetCounts,
			ZonedCells: Zones.Count);
	}
}

public enum WarehouseTool
{
	Place,
	Erase
}

public enum WarehouseMode
{
	Build,
	Zone
}

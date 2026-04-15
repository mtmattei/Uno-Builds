namespace GridForm.Models;

public record WarehouseMetrics(
	double FloorUtilization,
	double VolumeUtilization,
	int TotalUnits,
	int PeakHeight,
	ImmutableDictionary<AssetType, int> AssetCounts,
	int ZonedCells);

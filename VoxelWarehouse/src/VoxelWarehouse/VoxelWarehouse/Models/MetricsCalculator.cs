namespace VoxelWarehouse.Models;

public static class MetricsCalculator
{
    public static UtilizationMetrics Compute(VoxelWorldState world)
    {
        var cells = world.Cells;
        var totalCells = cells.Count;

        // Single pass over cells to compute floor, peak height, weight, and asset counts
        var floorSet = new HashSet<(int X, int Z)>();
        int peakY = 0;
        double totalWeight = 0;
        var assetCounts = new Dictionary<AssetType, int>();

        foreach (var (key, asset) in cells)
        {
            floorSet.Add((key.X, key.Z));
            if (key.Y > peakY) peakY = key.Y;
            totalWeight += GridConstants.GetWeight(asset);

            if (assetCounts.TryGetValue(asset, out int count))
                assetCounts[asset] = count + 1;
            else
                assetCounts[asset] = 1;
        }

        var maxFloor = GridConstants.GridSize * GridConstants.GridSize;
        var floorUtil = maxFloor > 0 ? (double)floorSet.Count / maxFloor * 100.0 : 0.0;

        var maxVolume = maxFloor * GridConstants.MaxHeight;
        var volumeUtil = maxVolume > 0 ? (double)totalCells / maxVolume * 100.0 : 0.0;

        var peakHeight = totalCells > 0 ? peakY + 1 : 0;

        // Zone counts
        var zoneCounts = new Dictionary<ZoneType, int>();
        foreach (var zone in world.Zones.Values)
        {
            if (zone == ZoneType.None) continue;
            if (zoneCounts.TryGetValue(zone, out int zc))
                zoneCounts[zone] = zc + 1;
            else
                zoneCounts[zone] = 1;
        }

        return new UtilizationMetrics(
            FloorUtilPercent: Math.Round(floorUtil, 1),
            VolumeUtilPercent: Math.Round(volumeUtil, 1),
            TotalWeightTons: Math.Round(totalWeight, 1),
            PeakHeight: peakHeight,
            TotalUnits: totalCells,
            AssetCounts: assetCounts.ToImmutableDictionary(),
            ZoneCellCounts: zoneCounts.ToImmutableDictionary());
    }
}

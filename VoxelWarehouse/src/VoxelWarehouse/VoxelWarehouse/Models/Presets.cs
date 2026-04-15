namespace VoxelWarehouse.Models;

public static class Presets
{
    public static VoxelWorldState Get(string name) => name switch
    {
        "A" => WarehouseA(),
        "B" => WarehouseB(),
        "C" => WarehouseC(),
        _ => VoxelWorldState.Empty()
    };

    /// <summary>Small warehouse with pallets and racks in organized rows.</summary>
    public static VoxelWorldState WarehouseA()
    {
        var cells = ImmutableDictionary.CreateBuilder<(int X, int Y, int Z), AssetType>();
        var zones = ImmutableDictionary.CreateBuilder<(int X, int Z), ZoneType>();

        // Receiving zone (left strip)
        for (int z = 2; z <= 11; z++)
        {
            zones[(0, z)] = ZoneType.Receiving;
            zones[(1, z)] = ZoneType.Receiving;
        }

        // Shipping zone (right strip)
        for (int z = 2; z <= 11; z++)
        {
            zones[(12, z)] = ZoneType.Shipping;
            zones[(13, z)] = ZoneType.Shipping;
        }

        // Storage zone (center)
        for (int x = 3; x <= 10; x++)
            for (int z = 2; z <= 11; z++)
                zones[(x, z)] = ZoneType.Storage;

        // Aisle down the middle
        for (int z = 0; z < GridConstants.GridSize; z++)
        {
            cells[(2, 0, z)] = AssetType.Aisle;
            cells[(6, 0, z)] = AssetType.Aisle;
            cells[(11, 0, z)] = AssetType.Aisle;
        }

        // Rack rows
        for (int z = 2; z <= 11; z += 3)
        {
            for (int x = 3; x <= 5; x++)
            {
                cells[(x, 0, z)] = AssetType.Rack;
                cells[(x, 1, z)] = AssetType.Pallet;
            }
            for (int x = 7; x <= 10; x++)
            {
                cells[(x, 0, z)] = AssetType.Rack;
                cells[(x, 1, z)] = AssetType.Pallet;
            }
        }

        // Some containers near receiving
        cells[(1, 0, 3)] = AssetType.Container;
        cells[(1, 0, 6)] = AssetType.Container;
        cells[(1, 0, 9)] = AssetType.Container;

        // Equipment
        cells[(12, 0, 5)] = AssetType.Equipment;
        cells[(12, 0, 8)] = AssetType.Equipment;

        return VoxelWorldState.Empty() with
        {
            Cells = cells.ToImmutable(),
            Zones = zones.ToImmutable()
        };
    }

    /// <summary>Dense multi-level storage facility.</summary>
    public static VoxelWorldState WarehouseB()
    {
        var cells = ImmutableDictionary.CreateBuilder<(int X, int Y, int Z), AssetType>();

        // Dense rack grid, 3 layers high
        for (int x = 1; x <= 12; x += 2)
        {
            for (int z = 1; z <= 12; z += 2)
            {
                for (int y = 0; y < 3; y++)
                {
                    cells[(x, y, z)] = AssetType.Rack;
                    cells[(x, y, z + 1)] = AssetType.Pallet;
                }
            }
        }

        // Aisles
        for (int z = 0; z < GridConstants.GridSize; z++)
        {
            for (int x = 0; x < GridConstants.GridSize; x += 2)
                cells[(x, 0, z)] = AssetType.Aisle;
        }

        return VoxelWorldState.Empty() with { Cells = cells.ToImmutable() };
    }

    /// <summary>Empty warehouse floor with zone markers.</summary>
    public static VoxelWorldState WarehouseC()
    {
        var zones = ImmutableDictionary.CreateBuilder<(int X, int Z), ZoneType>();

        // Quadrant zones
        for (int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
                zones[(x, z)] = ZoneType.Receiving;
            for (int z = 7; z < 14; z++)
                zones[(x, z)] = ZoneType.Storage;
        }
        for (int x = 7; x < 14; x++)
        {
            for (int z = 0; z < 7; z++)
                zones[(x, z)] = ZoneType.Staging;
            for (int z = 7; z < 14; z++)
                zones[(x, z)] = ZoneType.Shipping;
        }

        return VoxelWorldState.Empty() with { Zones = zones.ToImmutable() };
    }
}

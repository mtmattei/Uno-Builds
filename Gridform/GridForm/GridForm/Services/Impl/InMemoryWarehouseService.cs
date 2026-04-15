namespace GridForm.Services.Impl;

public class InMemoryWarehouseService : IWarehouseService
{
	public ValueTask<WarehouseState> GetInitialState(CancellationToken ct)
		=> ValueTask.FromResult(BuildPresetA());

	public ValueTask<WarehouseState> LoadPreset(string presetName, CancellationToken ct)
	{
		return presetName switch
		{
			"A" => ValueTask.FromResult(BuildPresetA()),
			"B" => ValueTask.FromResult(BuildPresetB()),
			_ => ValueTask.FromResult(new WarehouseState())
		};
	}

	private static WarehouseState BuildPresetA()
	{
		var voxels = new Dictionary<VoxelKey, AssetType>();
		var zones = new Dictionary<(int X, int Z), ZoneType>();

		// Receiving zone (left strip x=0..3)
		for (var x = 0; x < 4; x++)
			for (var z = 0; z < 14; z++)
				zones[(x, z)] = ZoneType.Receiving;

		// Pallets in receiving
		for (var x = 0; x < 3; x++)
			for (var z = 1; z < 13; z += 3)
			{
				voxels[new VoxelKey(x, 0, z)] = AssetType.Pallet;
				voxels[new VoxelKey(x, 0, z + 1)] = AssetType.Pallet;
			}

		// Storage zone (center x=5..10)
		for (var x = 5; x < 11; x++)
			for (var z = 0; z < 14; z++)
				zones[(x, z)] = ZoneType.Storage;

		// Rack columns in storage (every other x)
		for (var x = 5; x < 11; x += 2)
			for (var z = 0; z < 14; z++)
				for (var y = 0; y < 4; y++)
					voxels[new VoxelKey(x, y, z)] = AssetType.Rack;

		// Aisle lanes
		for (var z = 0; z < 14; z++)
		{
			voxels[new VoxelKey(6, 0, z)] = AssetType.Aisle;
			voxels[new VoxelKey(8, 0, z)] = AssetType.Aisle;
		}

		// Shipping zone (right strip x=12..13)
		for (var x = 12; x < 14; x++)
			for (var z = 0; z < 14; z++)
				zones[(x, z)] = ZoneType.Shipping;

		// Containers in shipping
		for (var z = 2; z < 12; z += 4)
		{
			voxels[new VoxelKey(12, 0, z)] = AssetType.Container;
			voxels[new VoxelKey(12, 0, z + 1)] = AssetType.Container;
			voxels[new VoxelKey(12, 1, z)] = AssetType.Container;
		}

		return new WarehouseState
		{
			Voxels = voxels,
			Zones = zones
		};
	}

	private static WarehouseState BuildPresetB()
	{
		var voxels = new Dictionary<VoxelKey, AssetType>();
		var zones = new Dictionary<(int X, int Z), ZoneType>();

		// Staging zone (center)
		for (var x = 3; x < 11; x++)
			for (var z = 3; z < 11; z++)
				zones[(x, z)] = ZoneType.Staging;

		// Pallets in staging
		for (var x = 4; x < 10; x += 2)
			for (var z = 4; z < 10; z += 2)
				for (var y = 0; y < 3; y++)
					voxels[new VoxelKey(x, y, z)] = AssetType.Pallet;

		// Aisle lanes
		for (var z = 0; z < 14; z++)
		{
			voxels[new VoxelKey(3, 0, z)] = AssetType.Aisle;
			voxels[new VoxelKey(10, 0, z)] = AssetType.Aisle;
		}

		return new WarehouseState
		{
			Voxels = voxels,
			Zones = zones
		};
	}
}

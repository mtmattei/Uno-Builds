using System.Text.Json;

namespace VoxelWarehouse.Services;

/// <summary>
/// Serializable DTO for voxel world state — ImmutableDictionary with tuple keys
/// cannot be directly serialized by System.Text.Json.
/// </summary>
internal record VoxelWorldDto(
    List<VoxelCellDto> Cells,
    List<ZoneCellDto> Zones,
    int ActiveLayer,
    string ActiveAsset,
    string ActiveTool,
    string ActiveMode);

internal record VoxelCellDto(int X, int Y, int Z, string Asset);
internal record ZoneCellDto(int X, int Z, string Zone);

public sealed class JsonVoxelStorageService : IVoxelStorageService
{
    private readonly string _basePath;

    public JsonVoxelStorageService()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoxelWarehouse", "Layouts");
        Directory.CreateDirectory(_basePath);
    }

    public async Task SaveAsync(string name, VoxelWorldState world, CancellationToken ct = default)
    {
        var dto = new VoxelWorldDto(
            Cells: world.Cells.Select(kv => new VoxelCellDto(kv.Key.X, kv.Key.Y, kv.Key.Z, kv.Value.ToString())).ToList(),
            Zones: world.Zones.Select(kv => new ZoneCellDto(kv.Key.X, kv.Key.Z, kv.Value.ToString())).ToList(),
            ActiveLayer: world.ActiveLayer,
            ActiveAsset: world.ActiveAsset.ToString(),
            ActiveTool: world.ActiveTool.ToString(),
            ActiveMode: world.ActiveMode.ToString());

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        var path = Path.Combine(_basePath, $"{name}.json");
        await File.WriteAllTextAsync(path, json, ct);
    }

    public async Task<VoxelWorldState?> LoadAsync(string name, CancellationToken ct = default)
    {
        var path = Path.Combine(_basePath, $"{name}.json");
        if (!File.Exists(path)) return null;

        var json = await File.ReadAllTextAsync(path, ct);
        var dto = JsonSerializer.Deserialize<VoxelWorldDto>(json);
        if (dto is null) return null;

        var cells = dto.Cells
            .Where(c => Enum.TryParse<AssetType>(c.Asset, out _))
            .ToImmutableDictionary(
                c => (c.X, c.Y, c.Z),
                c => Enum.Parse<AssetType>(c.Asset));

        var zones = dto.Zones
            .Where(z => Enum.TryParse<ZoneType>(z.Zone, out _))
            .ToImmutableDictionary(
                z => (z.X, z.Z),
                z => Enum.Parse<ZoneType>(z.Zone));

        return new VoxelWorldState(
            cells, zones,
            dto.ActiveLayer,
            Enum.TryParse<AssetType>(dto.ActiveAsset, out var asset) ? asset : AssetType.Pallet,
            Enum.TryParse<ToolMode>(dto.ActiveTool, out var tool) ? tool : ToolMode.Place,
            Enum.TryParse<EditorMode>(dto.ActiveMode, out var mode) ? mode : EditorMode.Build);
    }
}

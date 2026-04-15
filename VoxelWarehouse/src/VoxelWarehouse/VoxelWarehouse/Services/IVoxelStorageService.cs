namespace VoxelWarehouse.Services;

public interface IVoxelStorageService
{
    Task SaveAsync(string name, VoxelWorldState world, CancellationToken ct = default);
    Task<VoxelWorldState?> LoadAsync(string name, CancellationToken ct = default);
}

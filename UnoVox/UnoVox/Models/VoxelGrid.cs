namespace UnoVox.Models;

/// <summary>
/// Manages a 3D grid of voxels
/// </summary>
public class VoxelGrid
{
    private readonly Dictionary<(int x, int y, int z), Voxel> _voxels;
    private readonly List<Voxel> _sortedVoxels;
    private readonly IComparer<Voxel> _depthComparer;

    public int Size { get; }
    public IReadOnlyList<Voxel> ActiveVoxels => _sortedVoxels;

    public VoxelGrid(int size = 32)
    {
        Size = size;
        _voxels = new Dictionary<(int x, int y, int z), Voxel>();
        _sortedVoxels = new List<Voxel>();
        _depthComparer = Comparer<Voxel>.Create((a, b) =>
            (a.X + a.Y + a.Z).CompareTo(b.X + b.Y + b.Z));
    }

    /// <summary>
    /// Places a voxel at the specified position
    /// </summary>
    public bool PlaceVoxel(int x, int y, int z, string color)
    {
        if (!IsValidPosition(x, y, z))
            return false;

        var key = (x, y, z);
        var voxel = new Voxel(x, y, z, color, true);
        _voxels[key] = voxel;

        // Insert into sorted list maintaining order
        int index = _sortedVoxels.BinarySearch(voxel, _depthComparer);
        if (index < 0) index = ~index;
        _sortedVoxels.Insert(index, voxel);

        return true;
    }

    /// <summary>
    /// Removes a voxel at the specified position
    /// </summary>
    public bool RemoveVoxel(int x, int y, int z)
    {
        if (!IsValidPosition(x, y, z))
            return false;

        var key = (x, y, z);
        if (_voxels.TryGetValue(key, out var voxel))
        {
            _voxels.Remove(key);
            _sortedVoxels.Remove(voxel);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a voxel at the specified position
    /// </summary>
    public Voxel? GetVoxel(int x, int y, int z)
    {
        var key = (x, y, z);
        return _voxels.TryGetValue(key, out var voxel) ? voxel : null;
    }

    /// <summary>
    /// Checks if a voxel exists at the specified position
    /// </summary>
    public bool HasVoxel(int x, int y, int z)
    {
        return _voxels.ContainsKey((x, y, z));
    }

    /// <summary>
    /// Clears all voxels from the grid
    /// </summary>
    public void Clear()
    {
        _voxels.Clear();
        _sortedVoxels.Clear();
    }

    /// <summary>
    /// Validates if a position is within grid bounds
    /// </summary>
    public bool IsValidPosition(int x, int y, int z)
    {
        return x >= 0 && x < Size &&
               y >= 0 && y < Size &&
               z >= 0 && z < Size;
    }
}

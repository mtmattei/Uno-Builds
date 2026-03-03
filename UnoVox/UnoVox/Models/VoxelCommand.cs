namespace UnoVox.Models;

/// <summary>
/// Base interface for undo/redo commands
/// </summary>
public interface IVoxelCommand
{
    void Execute(VoxelGrid grid);
    void Undo(VoxelGrid grid);
}

/// <summary>
/// Command to place a voxel
/// </summary>
public class PlaceVoxelCommand : IVoxelCommand
{
    private readonly int _x, _y, _z;
    private readonly string _color;

    public PlaceVoxelCommand(int x, int y, int z, string color)
    {
        _x = x;
        _y = y;
        _z = z;
        _color = color;
    }

    public void Execute(VoxelGrid grid)
    {
        grid.PlaceVoxel(_x, _y, _z, _color);
    }

    public void Undo(VoxelGrid grid)
    {
        grid.RemoveVoxel(_x, _y, _z);
    }
}

/// <summary>
/// Command to remove a voxel
/// </summary>
public class RemoveVoxelCommand : IVoxelCommand
{
    private readonly int _x, _y, _z;
    private Voxel? _previousVoxel;

    public RemoveVoxelCommand(int x, int y, int z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public void Execute(VoxelGrid grid)
    {
        _previousVoxel = grid.GetVoxel(_x, _y, _z);
        grid.RemoveVoxel(_x, _y, _z);
    }

    public void Undo(VoxelGrid grid)
    {
        if (_previousVoxel.HasValue)
        {
            var v = _previousVoxel.Value;
            grid.PlaceVoxel(v.X, v.Y, v.Z, v.Color);
        }
    }
}

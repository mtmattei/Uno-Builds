namespace UnoVox.Models;

/// <summary>
/// Represents a single voxel in 3D space
/// </summary>
public struct Voxel
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public string Color { get; set; } // Hex color string
    public bool IsActive { get; set; }

    public Voxel(int x, int y, int z, string color, bool isActive = true)
    {
        X = x;
        Y = y;
        Z = z;
        Color = color;
        IsActive = isActive;
    }

    public override bool Equals(object? obj)
    {
        return obj is Voxel voxel &&
               X == voxel.X &&
               Y == voxel.Y &&
               Z == voxel.Z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}

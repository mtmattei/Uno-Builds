namespace VoxelWarehouse.Controls;

/// <summary>
/// Isometric coordinate conversion with rotation support.
/// Rotation 0-3 rotates the view in 90° CW increments.
/// </summary>
public static class IsoMath
{
    private const int HW = GridConstants.HalfWidth;
    private const int HH = GridConstants.HalfHeight;
    private const int Depth = GridConstants.VoxelDepth;
    private const int G = GridConstants.GridSize - 1; // max index

    /// <summary>
    /// Rotate grid coords (gx, gz) by the given rotation (0-3).
    /// 0=NE (default), 1=SE, 2=SW, 3=NW — each step is 90° CW.
    /// </summary>
    public static (int RX, int RZ) RotateGrid(int gx, int gz, int rotation) => (rotation % 4) switch
    {
        0 => (gx, gz),
        1 => (gz, G - gx),
        2 => (G - gx, G - gz),
        3 => (G - gz, gx),
        _ => (gx, gz)
    };

    /// <summary>Inverse of RotateGrid — from rotated coords back to world coords.</summary>
    public static (int GX, int GZ) UnrotateGrid(int rx, int rz, int rotation) => (rotation % 4) switch
    {
        0 => (rx, rz),
        1 => (G - rz, rx),
        2 => (G - rx, G - rz),
        3 => (rz, G - rx),
        _ => (rx, rz)
    };

    /// <summary>
    /// Convert grid coordinates (gx, gz) at a given layer to screen pixel coordinates.
    /// Applies rotation before projection. Returns the apex (top center) of the isometric diamond.
    /// </summary>
    public static (float SX, float SY) GridToScreen(int gx, int gz, int layer, float originX, float originY, int rotation = 0)
    {
        var (rx, rz) = RotateGrid(gx, gz, rotation);
        float sx = originX + (rx - rz) * HW;
        float sy = originY + (rx + rz) * HH - layer * Depth;
        return (sx, sy);
    }

    /// <summary>
    /// Convert screen pixel coordinates back to world grid (gx, gz) at a given layer.
    /// Applies inverse rotation after inverse projection.
    /// </summary>
    public static (int GX, int GZ) ScreenToGrid(float sx, float sy, int layer, float originX, float originY, int rotation = 0)
    {
        float adjY = sy + layer * Depth;
        float dx = sx - originX;
        // Subtract HH so the hit-test maps to diamond centers, not apexes.
        // GridToScreen returns the apex; the visible center is HH pixels below.
        float dy = adjY - originY - HH;

        float frx = (dx / HW + dy / HH) / 2f;
        float frz = (dy / HH - dx / HW) / 2f;

        int rx = (int)MathF.Floor(frx + 0.5f);
        int rz = (int)MathF.Floor(frz + 0.5f);

        rx = Math.Clamp(rx, 0, G);
        rz = Math.Clamp(rz, 0, G);

        return UnrotateGrid(rx, rz, rotation);
    }

    /// <summary>
    /// Compute the depth fog factor for a voxel at (gx, gz) with rotation.
    /// Range: 1.0 (front) to 0.6 (back).
    /// </summary>
    public static float FogFactor(int gx, int gz, int rotation = 0)
    {
        var (rx, rz) = RotateGrid(gx, gz, rotation);
        float maxDepth = G * 2f;
        return maxDepth > 0 ? 1f - ((rx + rz) / maxDepth) * 0.40f : 1f;
    }
}

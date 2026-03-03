using System.Numerics;
using UnoVox.Models;

namespace UnoVox.Services;

/// <summary>
/// Handles ray casting from screen space to voxel grid
/// </summary>
public class RayCaster
{
    /// <summary>
    /// Converts 2D screen position to 3D ray in world space
    /// </summary>
    public (Vector3 origin, Vector3 direction) ScreenToWorldRay(
        float screenX, float screenY,
        CameraController camera,
        float screenWidth, float screenHeight)
    {
        // Convert screen to normalized device coordinates (-1 to 1)
        float ndcX = (screenX - screenWidth / 2f) / (screenWidth / 2f);
        float ndcY = (screenY - screenHeight / 2f) / (screenHeight / 2f);

        // Apply inverse zoom
        ndcX /= camera.Zoom;
        ndcY /= camera.Zoom;

        // Create rotation matrices (inverse of rendering rotation)
        float pitchRad = -camera.RotationX * MathF.PI / 180f;
        float yawRad = -camera.RotationY * MathF.PI / 180f;

        // Ray direction in camera space (pointing into screen)
        Vector3 rayDir = new Vector3(ndcX, ndcY, -1f);

        // Apply inverse pitch rotation (around X-axis)
        float cosP = MathF.Cos(pitchRad);
        float sinP = MathF.Sin(pitchRad);
        float y1 = rayDir.Y * cosP - rayDir.Z * sinP;
        float z1 = rayDir.Y * sinP + rayDir.Z * cosP;
        rayDir = new Vector3(rayDir.X, y1, z1);

        // Apply inverse yaw rotation (around Y-axis)
        float cosY = MathF.Cos(yawRad);
        float sinY = MathF.Sin(yawRad);
        float x1 = rayDir.X * cosY - rayDir.Z * sinY;
        float z2 = rayDir.X * sinY + rayDir.Z * cosY;
        rayDir = new Vector3(x1, rayDir.Y, z2);

        // Normalize direction
        rayDir = Vector3.Normalize(rayDir);

        // Ray origin is at camera position (accounting for pan)
        Vector3 rayOrigin = new Vector3(
            -camera.PanX / camera.Zoom,
            camera.PanY / camera.Zoom,
            camera.Zoom / 10f
        );

        return (rayOrigin, rayDir);
    }

    /// <summary>
    /// Finds the first voxel position the ray hits or would hit
    /// Uses DDA (Digital Differential Analyzer) algorithm for voxel traversal
    /// </summary>
    public (int x, int y, int z)? RayGridIntersection(
        Vector3 origin, Vector3 direction, VoxelGrid grid,
        bool findEmpty = false)
    {
        // Offset to center grid at origin
        int halfSize = grid.Size / 2;
        
        // Starting voxel
        float startX = origin.X + halfSize;
        float startY = origin.Y + halfSize;
        float startZ = origin.Z + halfSize;

        int x = (int)MathF.Floor(startX);
        int y = (int)MathF.Floor(startY);
        int z = (int)MathF.Floor(startZ);

        // Step direction
        int stepX = direction.X > 0 ? 1 : -1;
        int stepY = direction.Y > 0 ? 1 : -1;
        int stepZ = direction.Z > 0 ? 1 : -1;

        // Avoid division by zero
        if (MathF.Abs(direction.X) < 0.0001f) direction.X = 0.0001f;
        if (MathF.Abs(direction.Y) < 0.0001f) direction.Y = 0.0001f;
        if (MathF.Abs(direction.Z) < 0.0001f) direction.Z = 0.0001f;

        // t values for next voxel boundary
        float tMaxX = ((x + (stepX > 0 ? 1 : 0)) - startX) / direction.X;
        float tMaxY = ((y + (stepY > 0 ? 1 : 0)) - startY) / direction.Y;
        float tMaxZ = ((z + (stepZ > 0 ? 1 : 0)) - startZ) / direction.Z;

        // t delta per voxel
        float tDeltaX = MathF.Abs(1f / direction.X);
        float tDeltaY = MathF.Abs(1f / direction.Y);
        float tDeltaZ = MathF.Abs(1f / direction.Z);

        // Track previous position for empty finding
        int prevX = x, prevY = y, prevZ = z;

        // Traverse voxels
        int maxSteps = grid.Size * 3;
        for (int i = 0; i < maxSteps; i++)
        {
            // Check if current voxel is valid
            if (grid.IsValidPosition(x, y, z))
            {
                bool hasVoxel = grid.HasVoxel(x, y, z);
                
                if (findEmpty)
                {
                    // Return the empty space before a solid voxel
                    if (hasVoxel)
                        return (prevX, prevY, prevZ);
                }
                else
                {
                    // Return the first solid voxel hit
                    if (hasVoxel)
                        return (x, y, z);
                }
            }
            else
            {
                // Out of bounds - return last valid position if finding empty
                if (findEmpty && grid.IsValidPosition(prevX, prevY, prevZ))
                    return (prevX, prevY, prevZ);
                break;
            }

            // Store previous position
            prevX = x;
            prevY = y;
            prevZ = z;

            // Step to next voxel boundary
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                x += stepX;
                tMaxX += tDeltaX;
            }
            else if (tMaxY < tMaxZ)
            {
                y += stepY;
                tMaxY += tDeltaY;
            }
            else
            {
                z += stepZ;
                tMaxZ += tDeltaZ;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds an empty voxel position where a new voxel can be placed
    /// </summary>
    public (int x, int y, int z)? FindPlacementPosition(
        Vector3 origin, Vector3 direction, VoxelGrid grid)
    {
        return RayGridIntersection(origin, direction, grid, findEmpty: true);
    }
}

using Microsoft.Extensions.Options;
using UnoVox.Configuration;
using UnoVox.Models;

namespace UnoVox.Services;

/// <summary>
/// Maps hand positions from 2D camera space to 3D voxel grid coordinates
/// </summary>
public class HandToVoxelMapper
{
    private readonly int _gridSize;
    private (int x, int y, int z)? _lastVoxelPosition;
    private readonly KalmanFilter3D _kalmanFilter; // Kalman filtering for smoother, more responsive tracking
    private readonly VoxelEditorConfig _voxelConfig;
    private readonly HandTrackingConfig _handConfig;
    private int _logCounter = 0; // Log every 30 frames to avoid spam

    public HandToVoxelMapper(
        IOptions<VoxelEditorConfig> voxelConfig,
        IOptions<HandTrackingConfig> handConfig)
    {
        _voxelConfig = voxelConfig.Value;
        _handConfig = handConfig.Value;
        _gridSize = _voxelConfig.GridSize;

        // Use config values for Kalman filter
        _kalmanFilter = new KalmanFilter3D(
            processNoise: _handConfig.KalmanProcessNoise,
            measurementNoise: _handConfig.KalmanMeasurementNoise);

        Console.WriteLine($"[HandToVoxelMapper] Initialized with grid size {_gridSize}, " +
                         $"Kalman(process={_handConfig.KalmanProcessNoise:F3}, measure={_handConfig.KalmanMeasurementNoise:F3})");
    }

    /// <summary>
    /// Maps hand position to flat AR plane in front of camera (like drawing on invisible canvas)
    /// Uses the pinch point (midpoint between thumb and index finger) for direct manipulation feel
    /// </summary>
    /// <param name="detection">Hand detection with landmarks</param>
    /// <param name="fixedDepth">Fixed depth plane in front of camera (in grid units, default from config)</param>
    /// <returns>Voxel grid coordinates (x, y, z) or null if invalid</returns>
    public (int x, int y, int z)? MapToArPlane(HandDetection detection, float? fixedDepth = null)
    {
        if (detection?.Landmarks == null || detection.Landmarks.Count < 21)
            return null;

        // Use pinch point (midpoint between thumb tip and index tip) instead of palm center
        // This makes voxels appear exactly where fingers meet - direct manipulation!
        var thumbTip = detection.Landmarks[4];   // Thumb tip
        var indexTip = detection.Landmarks[8];   // Index finger tip

        // Calculate midpoint between thumb and index (the "pinch point")
        var pinchPoint = new
        {
            X = (thumbTip.X + indexTip.X) / 2f,
            Y = (thumbTip.Y + indexTip.Y) / 2f,
            Z = (thumbTip.Z + indexTip.Z) / 2f
        };

        var depth = fixedDepth ?? _voxelConfig.ArPlaneDepth;

        // Map screen coordinates directly to grid plane
        // Screen X [0,1] → Grid X centered [-16 to 16] → Index [0-31]
        // Screen Y [0,1] → Grid Z centered [-16 to 16] → Index [0-31] (vertical)
        // Depth Y fixed at plane depth

        float normalizedX = (pinchPoint.X * 2.0f) - 1.0f;  // 0-1 → -1 to 1
        float normalizedZ = -((pinchPoint.Y * 2.0f) - 1.0f); // 0-1 → -1 to 1, Y-flipped

        // Map to grid coordinates (centered at origin)
        float worldX = normalizedX * (_gridSize / 2f);  // -16 to 16
        float worldZ = normalizedZ * (_gridSize / 2f);  // -16 to 16
        float worldY = depth;                            // Fixed depth plane

        // Convert to grid indices [0-31]
        var rawX = worldX + (_gridSize / 2f);
        var rawY = worldY;
        var rawZ = worldZ + (_gridSize / 2f);

        // Apply Kalman filtering
        var (filteredX, filteredY, filteredZ) = _kalmanFilter.Update(rawX, rawY, rawZ);

        var voxelX = (int)Math.Clamp(filteredX, 0, _gridSize - 1);
        var voxelY = (int)Math.Clamp(filteredY, 0, _gridSize - 1);
        var voxelZ = (int)Math.Clamp(filteredZ, 0, _gridSize - 1);

        var result = (voxelX, voxelY, voxelZ);
        _lastVoxelPosition = result;

        // Log every 30 frames to track coordinate mapping
        if (_logCounter++ % 30 == 0)
        {
            Console.WriteLine($"[HandToVoxelMapper] Pinch({pinchPoint.X:F3},{pinchPoint.Y:F3}) → " +
                            $"World({worldX:F1},{worldY:F1},{worldZ:F1}) → " +
                            $"Voxel({voxelX},{voxelY},{voxelZ})");
        }

        return result;
    }

    /// <summary>
    /// Maps hand position with depth control using hand size estimation
    /// Transforms from normalized screen coordinates [0,1] to world space in front of camera
    /// </summary>
    /// <param name="detection">Hand detection with landmarks</param>
    /// <param name="useDepthEstimation">If true, estimate depth from hand size</param>
    /// <returns>Voxel grid coordinates (x, y, z) or null if invalid</returns>
    public (int x, int y, int z)? MapToVoxelGridWithDepth(HandDetection detection, bool useDepthEstimation = true)
    {
        if (detection?.Landmarks == null || detection.Landmarks.Count < 21)
            return null;

        var palmCenter = detection.Landmarks[9];
        var wrist = detection.Landmarks[0];
        var middleTip = detection.Landmarks[12];
        
        // Calculate hand size (wrist to middle finger tip distance)
        var handSize = Math.Sqrt(
            Math.Pow(middleTip.X - wrist.X, 2) +
            Math.Pow(middleTip.Y - wrist.Y, 2)
        );
        
        // Transform from screen space [0,1] to centered world space [-1,1]
        // This matches how the camera views the scene
        float normalizedX = (palmCenter.X * 2.0f) - 1.0f;  // 0-1 → -1 to 1
        float normalizedY = -((palmCenter.Y * 2.0f) - 1.0f); // 0-1 → -1 to 1, flipped for Y-up
        
        // Define depth plane in front of camera (in world units)
        float depthInFront = _voxelConfig.ArPlaneDepth; // Center of grid (grid is 32 units, centered at origin)

        // Map Y (depth) based on hand size if enabled
        // Larger hand = closer to camera = less depth in front
        // Smaller hand = farther from camera = more depth in front
        float worldDepth;
        if (useDepthEstimation)
        {
            // Normalize hand size (typical range 0.1 to 0.4)
            var normalizedSize = Math.Clamp((handSize - 0.1) / 0.3, 0.0, 1.0);
            // Map to depth range: close hand (0.2-0.6), far hand (0.6-0.9)
            worldDepth = (float)(depthInFront * (0.2 + normalizedSize * 0.7));
        }
        else
        {
            worldDepth = depthInFront; // Middle of grid
        }
        
        // Convert normalized screen coords to world space
        // Spread across grid width/height proportionally
        float worldX = normalizedX * (_gridSize / 2f);  // -16 to 16
        float worldZ = normalizedY * (_gridSize / 2f);  // -16 to 16 (vertical)
        float worldY = worldDepth;                      // Depth in front of camera
        
        // Convert back to grid indices [0-31]
        // Renderer subtracts 16 to center, so we add it back for indexing
        var rawX = worldX + (_gridSize / 2f);  // -16 to 16 → 0 to 32
        var rawY = worldY;                      // Depth stays as-is
        var rawZ = worldZ + (_gridSize / 2f);  // -16 to 16 → 0 to 32
        
        // Apply Kalman filtering for smooth, responsive tracking
        var (filteredX, filteredY, filteredZ) = _kalmanFilter.Update(rawX, rawY, rawZ);
        
        var voxelX = (int)Math.Clamp(filteredX, 0, _gridSize - 1);
        var voxelY = (int)Math.Clamp(filteredY, 0, _gridSize - 1);
        var voxelZ = (int)Math.Clamp(filteredZ, 0, _gridSize - 1);
        
        var result = (voxelX, voxelY, voxelZ);
        _lastVoxelPosition = result;

        return result;
    }

    /// <summary>
    /// Resets the smoothing history
    /// </summary>
    public void Reset()
    {
        _lastVoxelPosition = null;
        _kalmanFilter.Reset();
    }
}

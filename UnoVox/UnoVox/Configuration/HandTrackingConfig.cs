namespace UnoVox.Configuration;

/// <summary>
/// Configuration for hand tracking and gesture detection.
/// All magic numbers centralized here for easy tuning.
/// </summary>
public class HandTrackingConfig
{
    // Performance Settings
    /// <summary>Target frames per second for hand tracking processing</summary>
    public int TargetFps { get; set; } = 30;

    /// <summary>Minimum interval between hand processing operations (ms). ~60 FPS = 16ms</summary>
    public int HandProcessingIntervalMs { get; set; } = 16;

    /// <summary>Minimum interval between canvas invalidations (ms). Max 30 FPS = 33ms</summary>
    public int MinInvalidateIntervalMs { get; set; } = 33;

    // ONNX Palm Detection Settings
    /// <summary>Run full palm detection every N frames (use region tracking in between)</summary>
    public int PalmDetectionInterval { get; set; } = 10;

    /// <summary>Minimum confidence for palm detection (0.0-1.0)</summary>
    public float PalmConfidenceThreshold { get; set; } = 0.3f;

    /// <summary>Non-maximum suppression threshold for palm detection</summary>
    public float PalmNmsThreshold { get; set; } = 0.25f;

    /// <summary>Padding around detected palm region for landmark extraction</summary>
    public float PalmBoxPadding { get; set; } = 0.3f;

    /// <summary>Input size for palm detection model (typically 192x192)</summary>
    public int PalmInputSize { get; set; } = 192;

    // ONNX Landmark Detection Settings
    /// <summary>Input size for hand landmark model (typically 224x224)</summary>
    public int LandmarkInputSize { get; set; } = 224;

    // Color Detection Fallback Settings
    /// <summary>Minimum contour area in pixels to consider as hand candidate</summary>
    public int MinHandAreaPixels { get; set; } = 3500;

    /// <summary>Maximum contour area in pixels to consider as hand candidate</summary>
    public int MaxHandAreaPixels { get; set; } = 80000;

    /// <summary>Minimum motion pixels required for background subtraction</summary>
    public int MinMotionPixels { get; set; } = 10;

    /// <summary>Number of consecutive frames required to confirm hand detection</summary>
    public int HandConfirmationFrames { get; set; } = 3;

    /// <summary>Maximum distance in pixels to track same hand across frames</summary>
    public int HandTrackingDistancePixels { get; set; } = 100;

    // Gesture Detection Settings
    /// <summary>Base pinch threshold (scaled by hand size)</summary>
    public float BasePinchThreshold { get; set; } = 0.45f;

    /// <summary>Finger extension ratio (tip must be this factor farther than base from palm)</summary>
    public float FingerExtensionRatio { get; set; } = 1.3f;

    /// <summary>Winner must be this factor better than second place to be recognized</summary>
    public float MinGestureConfidenceThreshold { get; set; } = 1.25f;

    /// <summary>Minimum confidence to enter a gesture state (used by state machine)</summary>
    public float GestureConfidenceThreshold { get; set; } = 0.60f;

    /// <summary>Minimum confidence to stay in current gesture state (hysteresis)</summary>
    public float GestureHysteresisThreshold { get; set; } = 0.45f;

    /// <summary>Minimum confidence for weak continuation (during drag operations)</summary>
    public float GestureWeakContinuationThreshold { get; set; } = 0.30f;

    /// <summary>Number of consecutive frames required to confirm gesture</summary>
    public int GestureConfirmationFrames { get; set; } = 1;

    // Kalman Filtering / Smoothing Settings
    /// <summary>Process noise for landmark smoothing (higher = more responsive, less smooth)</summary>
    public float LandmarkProcessNoise { get; set; } = 0.03f;

    /// <summary>Measurement noise for landmark smoothing</summary>
    public float LandmarkMeasurementNoise { get; set; } = 0.06f;

    /// <summary>Process noise for hand-to-voxel mapper smoothing</summary>
    public float MapperProcessNoise { get; set; } = 0.03f;

    /// <summary>Measurement noise for hand-to-voxel mapper smoothing</summary>
    public float MapperMeasurementNoise { get; set; } = 0.08f;

    // Accessor properties for consistent naming
    /// <summary>Alias for LandmarkProcessNoise (used by LandmarkSmoother)</summary>
    public float LandmarkSmootherProcessNoise => LandmarkProcessNoise;

    /// <summary>Alias for LandmarkMeasurementNoise (used by LandmarkSmoother)</summary>
    public float LandmarkSmootherMeasurementNoise => LandmarkMeasurementNoise;

    /// <summary>Alias for MapperProcessNoise (used by HandToVoxelMapper)</summary>
    public float KalmanProcessNoise => MapperProcessNoise;

    /// <summary>Alias for MapperMeasurementNoise (used by HandToVoxelMapper)</summary>
    public float KalmanMeasurementNoise => MapperMeasurementNoise;
}

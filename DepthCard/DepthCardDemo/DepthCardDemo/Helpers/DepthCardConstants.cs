namespace DepthCardDemo.Helpers;

/// <summary>
/// Constants used throughout the DepthCard component system.
/// Centralizes magic numbers for maintainability and clarity.
/// </summary>
public static class DepthCardConstants
{
    #region DepthCard Transform Constants

    /// <summary>
    /// Multiplier for subtle scale effect during tilt (0.001 = 0.1% per degree).
    /// </summary>
    public const double SCALE_FACTOR_MULTIPLIER = 0.0005;

    /// <summary>
    /// Multiplier for SkewX transform to simulate 3D rotation around Y axis.
    /// </summary>
    public const double SKEW_X_MULTIPLIER = 0.08;

    /// <summary>
    /// Multiplier for SkewY transform to simulate 3D rotation around X axis.
    /// </summary>
    public const double SKEW_Y_MULTIPLIER = 0.08;

    /// <summary>
    /// Offset distance for glare gradient edges (0.3 = 30% of card dimensions).
    /// </summary>
    public const double GLARE_OFFSET = 0.3;

    #endregion

    #region Animation Constants

    /// <summary>
    /// Duration in milliseconds for card reset animation when hover ends.
    /// </summary>
    public const int RESET_ANIMATION_DURATION_MS = 500;

    /// <summary>
    /// Number of oscillations in the elastic spring animation.
    /// </summary>
    public const int ELASTIC_OSCILLATIONS = 1;

    /// <summary>
    /// Springiness factor for elastic easing (higher = more bounce).
    /// </summary>
    public const int ELASTIC_SPRINGINESS = 4;

    /// <summary>
    /// Minimum time in milliseconds between pointer move event processing (~60fps).
    /// </summary>
    public const int POINTER_THROTTLE_MS = 16;

    #endregion

    #region DepthLayer Constants

    /// <summary>
    /// Divisor for parallax translation calculation (higher = less movement).
    /// </summary>
    public const double PARALLAX_SCALE_DIVISOR = 10.0;

    /// <summary>
    /// Scale factor applied per unit of depth (0.03 = 3% scale per depth unit).
    /// </summary>
    public const double DEPTH_SCALE_FACTOR = 0.03;

    /// <summary>
    /// Duration in milliseconds for layer reset animation.
    /// </summary>
    public const int LAYER_RESET_DURATION_MS = 500;

    /// <summary>
    /// Springiness for layer reset animation.
    /// </summary>
    public const int LAYER_ELASTIC_SPRINGINESS = 5;

    #endregion

    #region Liquid Inertia Constants

    /// <summary>
    /// Stiffness constant for spring physics (higher = faster return to target).
    /// </summary>
    public const double INERTIA_STIFFNESS = 0.2;

    /// <summary>
    /// Dampening constant for spring physics (higher = less oscillation).
    /// </summary>
    public const double INERTIA_DAMPENING = 0.8;

    /// <summary>
    /// Minimum velocity threshold below which motion is considered stopped.
    /// </summary>
    public const double INERTIA_VELOCITY_THRESHOLD = 0.01;

    #endregion

    #region Magnetic Elements Constants

    /// <summary>
    /// Default magnetic attraction strength (0-1 range).
    /// </summary>
    public const double DEFAULT_MAGNETIC_STRENGTH = 0.3;

    /// <summary>
    /// Default magnetic attraction range in pixels.
    /// </summary>
    public const double DEFAULT_MAGNETIC_RANGE = 100.0;

    /// <summary>
    /// Duration in milliseconds for magnetic attraction animation.
    /// </summary>
    public const int MAGNETIC_ANIMATION_DURATION_MS = 100;

    #endregion

    #region Edge Lighting Constants

    /// <summary>
    /// Default edge light intensity (0-1 range).
    /// </summary>
    public const double DEFAULT_EDGE_LIGHT_INTENSITY = 0.3;

    /// <summary>
    /// Border thickness for edge lighting overlay in pixels.
    /// </summary>
    public const double EDGE_LIGHT_BORDER_THICKNESS = 2.0;

    #endregion

    #region Living Presence Constants

    /// <summary>
    /// Default breathing cycle duration in milliseconds.
    /// </summary>
    public const double DEFAULT_BREATHING_CYCLE_MS = 4000.0;

    /// <summary>
    /// Default breathing intensity (0.008 = 0.8% scale change).
    /// </summary>
    public const double DEFAULT_BREATHING_INTENSITY = 0.008;

    #endregion

    #region Depth Shadow Constants

    /// <summary>
    /// Multiplier for shadow Z translation based on depth value.
    /// </summary>
    public const double SHADOW_Z_MULTIPLIER = 32.0;

    /// <summary>
    /// Multiplier for blur amount based on depth value.
    /// </summary>
    public const double BLUR_DEPTH_MULTIPLIER = 3.0;

    /// <summary>
    /// Exponent for non-linear parallax scaling (higher = more dramatic separation).
    /// </summary>
    public const double PARALLAX_NONLINEAR_EXPONENT = 1.2;

    #endregion
}

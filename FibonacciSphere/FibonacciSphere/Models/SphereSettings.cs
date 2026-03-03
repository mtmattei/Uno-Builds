using FibonacciSphere.Math;

namespace FibonacciSphere.Models;

/// <summary>
/// Configuration settings for the Fibonacci sphere visualization.
/// </summary>
public record SphereSettings
{
    // Shape and point generation
    public ShapeType Shape { get; init; } = ShapeType.Sphere;
    public int PointCount { get; init; } = 200;

    // Rotation
    public float RotationSpeed { get; init; } = 0.5f;
    public bool IsRotating { get; init; } = true;
    public bool RotateClockwise { get; init; } = true;
    public Easing.EasingType EasingType { get; init; } = Easing.EasingType.Linear;

    // Wobble effect
    public float WobbleAmplitude { get; init; } = 0.1f;
    public float WobbleFrequency { get; init; } = 2.0f;
    public WobbleAxis WobbleAxis { get; init; } = WobbleAxis.Radial;

    // Point size
    public float BasePointSize { get; init; } = 8f;
    public float SizeVariation { get; init; } = 0f;
    public float PulseSpeed { get; init; } = 0f;
    public float PulseAmount { get; init; } = 0f;
    public bool DepthScaling { get; init; } = true;

    // Trails
    public int TrailLength { get; init; } = 20;
    public float TrailOpacity { get; init; } = 0.5f;
    public TrailStyle TrailStyle { get; init; } = TrailStyle.Line;

    // Camera
    public float CameraDistance { get; init; } = 3.5f;

    // Colors
    public uint PrimaryColor { get; init; } = 0xFF404040; // Dark Grey
    public uint SecondaryColor { get; init; } = 0xFFFFFFFF; // White
    public bool UseGradientColors { get; init; } = true;
}

/// <summary>
/// Defines how the wobble effect is applied to points.
/// </summary>
public enum WobbleAxis
{
    /// <summary>Points move in/out from the sphere center.</summary>
    Radial,
    /// <summary>Points move tangent to the sphere surface.</summary>
    Tangential,
    /// <summary>Points move in random directions.</summary>
    Random
}

/// <summary>
/// Defines how trails are rendered behind points.
/// </summary>
public enum TrailStyle
{
    /// <summary>Connected line segments.</summary>
    Line,
    /// <summary>Individual dots at each position.</summary>
    Dots,
    /// <summary>Gradient ribbon effect.</summary>
    Ribbon
}

/// <summary>
/// Defines the 3D shape to render.
/// </summary>
public enum ShapeType
{
    /// <summary>Classic Fibonacci sphere distribution.</summary>
    Sphere,
    /// <summary>Uno Platform "U" logo shape.</summary>
    UnoLogo
}

using SkiaSharp;
using UnoVox.Models;

namespace UnoVox.Services;

/// <summary>
/// Interface for Roboflow-based gesture classification.
/// Provides cloud-based gesture recognition as an alternative/supplement to local detection.
/// </summary>
public interface IRoboflowGestureClassifier
{
    /// <summary>
    /// Gets whether the classifier is properly configured with an API key.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Gets whether the classifier is currently available (configured and online).
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Initializes the classifier and validates the API configuration.
    /// </summary>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Classifies a gesture from an image frame.
    /// </summary>
    /// <param name="frame">The full camera frame as SKBitmap.</param>
    /// <param name="handRegion">Optional region of interest containing the hand (improves accuracy).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The detected gesture with confidence score.</returns>
    Task<RoboflowGestureResult> ClassifyAsync(SKBitmap frame, SKRectI? handRegion = null, CancellationToken ct = default);
}

/// <summary>
/// Result from Roboflow gesture classification.
/// </summary>
public record RoboflowGestureResult
{
    /// <summary>
    /// The detected gesture type mapped to the app's GestureType enum.
    /// </summary>
    public GestureType Gesture { get; init; } = GestureType.None;

    /// <summary>
    /// The raw class name returned by Roboflow.
    /// </summary>
    public string RawClassName { get; init; } = string.Empty;

    /// <summary>
    /// Confidence score from Roboflow (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; init; }

    /// <summary>
    /// Time taken for the API call in milliseconds.
    /// </summary>
    public long LatencyMs { get; init; }

    /// <summary>
    /// Whether the classification was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if classification failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Static result for no detection or failure.
    /// </summary>
    public static RoboflowGestureResult None => new() { Success = true, Gesture = GestureType.None };

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    public static RoboflowGestureResult Failure(string message) => new() { Success = false, ErrorMessage = message };
}

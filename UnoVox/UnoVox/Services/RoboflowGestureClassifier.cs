using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using UnoVox.Models;

namespace UnoVox.Services;

/// <summary>
/// Roboflow cloud-based gesture classifier.
/// Calls the Roboflow Hosted API to classify hand gestures from images.
///
/// Features:
/// - Throttling to conserve API credits (configurable interval)
/// - Automatic fallback when offline or API unavailable
/// - Gesture mapping from Roboflow classes to app GestureType
/// - Caching of recent results to reduce API calls
/// </summary>
public class RoboflowGestureClassifier : IRoboflowGestureClassifier, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoboflowGestureClassifier> _logger;
    private readonly RoboflowConfig _config;

    private bool _isInitialized;
    private DateTime _lastClassificationTime = DateTime.MinValue;
    private RoboflowGestureResult _cachedResult = RoboflowGestureResult.None;

    // Gesture mapping from Roboflow class names to app GestureType
    private static readonly Dictionary<string, GestureType> GestureMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common gesture names from various Roboflow models
        ["palm"] = GestureType.OpenPalm,
        ["open_palm"] = GestureType.OpenPalm,
        ["open_hand"] = GestureType.OpenPalm,
        ["five"] = GestureType.OpenPalm,
        ["stop"] = GestureType.OpenPalm,

        ["fist"] = GestureType.ClosedFist,
        ["closed_fist"] = GestureType.ClosedFist,
        ["rock"] = GestureType.ClosedFist,

        ["pinch"] = GestureType.Pinch,
        ["ok"] = GestureType.Pinch,
        ["ok_sign"] = GestureType.Pinch,
        ["okay"] = GestureType.Pinch,

        ["point"] = GestureType.Point,
        ["pointing"] = GestureType.Point,
        ["one"] = GestureType.Point,
        ["index"] = GestureType.Point,

        ["thumbs_up"] = GestureType.ThumbsUp,
        ["thumbsup"] = GestureType.ThumbsUp,
        ["like"] = GestureType.ThumbsUp,
        ["thumb_up"] = GestureType.ThumbsUp,

        // Peace/victory often maps to Point in voxel editing context
        ["peace"] = GestureType.Point,
        ["victory"] = GestureType.Point,
        ["two"] = GestureType.Point,
    };

    public RoboflowGestureClassifier(
        IOptions<RoboflowConfig> config,
        ILogger<RoboflowGestureClassifier> logger)
    {
        _config = config.Value;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(_config.TimeoutMs)
        };
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_config.ApiKey) &&
                                 _config.ApiKey != "YOUR_API_KEY_HERE";

    public bool IsAvailable => IsConfigured && _isInitialized;

    public async Task<bool> InitializeAsync()
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Roboflow API key not configured. Set 'Roboflow:ApiKey' in appsettings.json or ROBOFLOW_API_KEY environment variable.");
            return false;
        }

        try
        {
            // Validate API key with a simple test (optional - can be skipped for faster startup)
            if (_config.ValidateOnStartup)
            {
                _logger.LogInformation("Validating Roboflow API configuration...");

                // Create a tiny test image
                using var testBitmap = new SKBitmap(64, 64);
                using var canvas = new SKCanvas(testBitmap);
                canvas.Clear(SKColors.Gray);

                var result = await ClassifyInternalAsync(testBitmap, null, CancellationToken.None);

                if (!result.Success && result.ErrorMessage?.Contains("401") == true)
                {
                    _logger.LogError("Roboflow API key is invalid.");
                    return false;
                }
            }

            _isInitialized = true;
            _logger.LogInformation("Roboflow gesture classifier initialized. Model: {Model}", _config.ModelEndpoint);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Roboflow gesture classifier.");
            return false;
        }
    }

    public async Task<RoboflowGestureResult> ClassifyAsync(SKBitmap frame, SKRectI? handRegion = null, CancellationToken ct = default)
    {
        if (!IsAvailable)
        {
            return RoboflowGestureResult.Failure("Roboflow classifier not available");
        }

        // Throttling: Return cached result if called too frequently
        var timeSinceLastCall = DateTime.UtcNow - _lastClassificationTime;
        if (timeSinceLastCall.TotalMilliseconds < _config.ThrottleIntervalMs)
        {
            return _cachedResult;
        }

        var result = await ClassifyInternalAsync(frame, handRegion, ct);

        _lastClassificationTime = DateTime.UtcNow;
        _cachedResult = result;

        return result;
    }

    private async Task<RoboflowGestureResult> ClassifyInternalAsync(SKBitmap frame, SKRectI? handRegion, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Crop to hand region if provided (improves accuracy and reduces data transfer)
            SKBitmap imageToSend;
            bool needsDispose = false;

            if (handRegion.HasValue && handRegion.Value.Width > 0 && handRegion.Value.Height > 0)
            {
                var region = handRegion.Value;
                // Ensure region is within bounds
                region = SKRectI.Intersect(region, new SKRectI(0, 0, frame.Width, frame.Height));

                if (region.Width > 10 && region.Height > 10)
                {
                    imageToSend = new SKBitmap(region.Width, region.Height);
                    needsDispose = true;

                    using var canvas = new SKCanvas(imageToSend);
                    canvas.DrawBitmap(frame, region, new SKRect(0, 0, region.Width, region.Height));
                }
                else
                {
                    imageToSend = frame;
                }
            }
            else
            {
                imageToSend = frame;
            }

            try
            {
                // Resize if image is too large (saves bandwidth and API credits)
                var resizedImage = ResizeIfNeeded(imageToSend, _config.MaxImageSize);
                var shouldDisposeResized = resizedImage != imageToSend;

                try
                {
                    // Encode to JPEG
                    using var stream = new MemoryStream();
                    resizedImage.Encode(stream, SKEncodedImageFormat.Jpeg, _config.JpegQuality);
                    var base64Image = Convert.ToBase64String(stream.ToArray());

                    // Build request URL
                    var url = $"{_config.BaseUrl}/{_config.ModelEndpoint}?api_key={_config.ApiKey}&confidence={_config.MinConfidence}";

                    // Send request
                    var content = new StringContent(base64Image, Encoding.ASCII, "application/x-www-form-urlencoded");
                    var response = await _httpClient.PostAsync(url, content, ct);

                    stopwatch.Stop();

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync(ct);
                        _logger.LogWarning("Roboflow API error: {StatusCode} - {Error}", response.StatusCode, errorBody);
                        return RoboflowGestureResult.Failure($"API error: {response.StatusCode}");
                    }

                    var json = await response.Content.ReadAsStringAsync(ct);
                    return ParseResponse(json, stopwatch.ElapsedMilliseconds);
                }
                finally
                {
                    if (shouldDisposeResized)
                    {
                        resizedImage.Dispose();
                    }
                }
            }
            finally
            {
                if (needsDispose)
                {
                    imageToSend.Dispose();
                }
            }
        }
        catch (TaskCanceledException)
        {
            return RoboflowGestureResult.Failure("Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Roboflow API request failed (network error)");
            return RoboflowGestureResult.Failure($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Roboflow classification");
            return RoboflowGestureResult.Failure($"Error: {ex.Message}");
        }
    }

    private SKBitmap ResizeIfNeeded(SKBitmap image, int maxSize)
    {
        if (image.Width <= maxSize && image.Height <= maxSize)
        {
            return image;
        }

        var scale = Math.Min((float)maxSize / image.Width, (float)maxSize / image.Height);
        var newWidth = (int)(image.Width * scale);
        var newHeight = (int)(image.Height * scale);

        var resized = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(resized);
        canvas.DrawBitmap(image, new SKRect(0, 0, newWidth, newHeight));

        return resized;
    }

    private RoboflowGestureResult ParseResponse(string json, long latencyMs)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Roboflow response format: { "predictions": [...], "time": ... }
            if (!root.TryGetProperty("predictions", out var predictions))
            {
                return new RoboflowGestureResult
                {
                    Success = true,
                    Gesture = GestureType.None,
                    LatencyMs = latencyMs
                };
            }

            // Find the prediction with highest confidence
            string? bestClass = null;
            float bestConfidence = 0;

            foreach (var prediction in predictions.EnumerateArray())
            {
                // Object detection format: { "class": "...", "confidence": 0.9, "x": ..., "y": ..., ... }
                if (prediction.TryGetProperty("class", out var classElement) &&
                    prediction.TryGetProperty("confidence", out var confElement))
                {
                    var className = classElement.GetString();
                    var confidence = confElement.GetSingle();

                    if (confidence > bestConfidence && !string.IsNullOrEmpty(className))
                    {
                        bestClass = className;
                        bestConfidence = confidence;
                    }
                }
                // Classification format: { "predictions": [{ "class": "...", "confidence": 0.9 }] }
                // or nested predictions
            }

            if (string.IsNullOrEmpty(bestClass))
            {
                return new RoboflowGestureResult
                {
                    Success = true,
                    Gesture = GestureType.None,
                    LatencyMs = latencyMs
                };
            }

            // Map Roboflow class to app GestureType
            var gesture = MapToGestureType(bestClass);

            _logger.LogDebug("Roboflow: '{RawClass}' -> {Gesture} ({Confidence:P1}) in {Latency}ms",
                bestClass, gesture, bestConfidence, latencyMs);

            return new RoboflowGestureResult
            {
                Success = true,
                Gesture = gesture,
                RawClassName = bestClass,
                Confidence = bestConfidence,
                LatencyMs = latencyMs
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Roboflow response: {Json}", json);
            return RoboflowGestureResult.Failure("Failed to parse response");
        }
    }

    private GestureType MapToGestureType(string roboflowClass)
    {
        // Normalize: remove underscores, spaces, convert to lowercase
        var normalized = roboflowClass.ToLowerInvariant().Replace(" ", "_").Replace("-", "_");

        if (GestureMap.TryGetValue(normalized, out var gesture))
        {
            return gesture;
        }

        // Try partial match
        foreach (var kvp in GestureMap)
        {
            if (normalized.Contains(kvp.Key) || kvp.Key.Contains(normalized))
            {
                return kvp.Value;
            }
        }

        _logger.LogDebug("Unknown Roboflow gesture class: '{Class}'", roboflowClass);
        return GestureType.None;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

/// <summary>
/// Configuration options for Roboflow gesture classifier.
/// </summary>
public class RoboflowConfig
{
    /// <summary>
    /// Roboflow API key. Required for cloud inference.
    /// Can be set via appsettings.json or ROBOFLOW_API_KEY environment variable.
    /// </summary>
    public string ApiKey { get; set; } = "YOUR_API_KEY_HERE";

    /// <summary>
    /// Roboflow model endpoint in format "project-name/version".
    /// Default uses a public hand gesture recognition model.
    /// </summary>
    public string ModelEndpoint { get; set; } = "hand-gesture-recognition-9ldly/1";

    /// <summary>
    /// Base URL for Roboflow inference API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://detect.roboflow.com";

    /// <summary>
    /// Minimum confidence threshold for detections (0.0 to 1.0).
    /// </summary>
    public float MinConfidence { get; set; } = 0.4f;

    /// <summary>
    /// Throttle interval in milliseconds. API will not be called more frequently than this.
    /// Helps conserve API credits. Default: 300ms (max ~3 calls/second).
    /// </summary>
    public int ThrottleIntervalMs { get; set; } = 300;

    /// <summary>
    /// HTTP request timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Maximum image dimension (width or height). Images larger than this will be resized.
    /// Reduces bandwidth and improves response time.
    /// </summary>
    public int MaxImageSize { get; set; } = 640;

    /// <summary>
    /// JPEG quality for image encoding (1-100). Lower = smaller file size.
    /// </summary>
    public int JpegQuality { get; set; } = 75;

    /// <summary>
    /// Whether to validate API key on startup. Set to false for faster startup.
    /// </summary>
    public bool ValidateOnStartup { get; set; } = false;
}

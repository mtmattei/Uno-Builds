using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace UnoVox.Services;

/// <summary>
/// Validates ONNX models to ensure they match expected formats.
/// Provides diagnostic information for troubleshooting model issues.
/// </summary>
public class OnnxModelValidator
{
    /// <summary>
    /// Validates MediaPipe hand detection models and returns diagnostic information.
    /// </summary>
    /// <param name="palmPath">Path to palm detection model</param>
    /// <param name="landmarkPath">Path to hand landmark model</param>
    /// <param name="facePath">Optional path to face detection model</param>
    /// <returns>Validation result with diagnostics</returns>
    public static ModelValidationResult ValidateModels(string palmPath, string landmarkPath, string? facePath = null)
    {
        var result = new ModelValidationResult();

        // Validate palm detection model
        result.PalmModelValid = ValidatePalmModel(palmPath, result);

        // Validate landmark model
        result.LandmarkModelValid = ValidateLandmarkModel(landmarkPath, result);

        // Validate face model if provided
        if (!string.IsNullOrEmpty(facePath))
        {
            result.FaceModelValid = ValidateFaceModel(facePath, result);
        }

        result.OverallValid = result.PalmModelValid && result.LandmarkModelValid;

        return result;
    }

    private static bool ValidatePalmModel(string path, ModelValidationResult result)
    {
        try
        {
            if (!File.Exists(path))
            {
                result.PalmModelError = $"Model file not found: {path}";
                return false;
            }

            using var session = new InferenceSession(path);

            // Get input metadata
            var inputMeta = session.InputMetadata.FirstOrDefault();
            if (inputMeta.Value == null)
            {
                result.PalmModelError = "No input metadata found";
                return false;
            }

            result.PalmInputShape = inputMeta.Value.Dimensions.ToArray();
            result.PalmInputName = inputMeta.Key;

            // Validate expected input shape [1, 192, 192, 3] or [1, 3, 192, 192]
            var dims = result.PalmInputShape;
            if (dims.Length != 4 || dims[0] != 1)
            {
                result.PalmModelWarning = $"Unexpected input shape: [{string.Join(",", dims)}]. Expected [1,192,192,3] or [1,3,192,192]";
            }

            int expectedSize = 192;
            bool hasExpectedSize = (dims[1] == expectedSize && dims[2] == expectedSize) ||
                                   (dims[2] == expectedSize && dims[3] == expectedSize);

            if (!hasExpectedSize)
            {
                result.PalmModelWarning = $"Input size {dims[1]}x{dims[2]} may not match expected 192x192";
            }

            // Get output metadata
            var outputMeta = session.OutputMetadata;
            result.PalmOutputCount = outputMeta.Count;
            result.PalmOutputNames = outputMeta.Keys.ToArray();

            // MediaPipe palm detection typically has 2 outputs:
            // - Boxes with keypoints: [1, 2016, 18]
            // - Scores: [1, 2016, 1]
            if (outputMeta.Count != 2)
            {
                result.PalmModelWarning = $"Expected 2 outputs, got {outputMeta.Count}. Model may use different architecture.";
            }

            // Try to get output shapes
            result.PalmOutputShapes = new int[outputMeta.Count][];
            int i = 0;
            foreach (var output in outputMeta.Values)
            {
                result.PalmOutputShapes[i] = output.Dimensions.ToArray();
                i++;
            }

            // Check for expected anchor count (2016 for standard MediaPipe)
            bool hasExpectedAnchorCount = false;
            foreach (var shape in result.PalmOutputShapes)
            {
                if (shape.Length >= 2 && shape[1] == 2016)
                {
                    hasExpectedAnchorCount = true;
                    break;
                }
            }

            if (!hasExpectedAnchorCount)
            {
                result.PalmModelWarning += " Output shapes don't match expected [1,2016,18] format.";
            }

            return true;
        }
        catch (Exception ex)
        {
            result.PalmModelError = $"Failed to load palm model: {ex.Message}";
            return false;
        }
    }

    private static bool ValidateLandmarkModel(string path, ModelValidationResult result)
    {
        try
        {
            if (!File.Exists(path))
            {
                result.LandmarkModelError = $"Model file not found: {path}";
                return false;
            }

            using var session = new InferenceSession(path);

            // Get input metadata
            var inputMeta = session.InputMetadata.FirstOrDefault();
            if (inputMeta.Value == null)
            {
                result.LandmarkModelError = "No input metadata found";
                return false;
            }

            result.LandmarkInputShape = inputMeta.Value.Dimensions.ToArray();
            result.LandmarkInputName = inputMeta.Key;

            // Validate expected input shape [1, 224, 224, 3] or [1, 3, 224, 224]
            var dims = result.LandmarkInputShape;
            if (dims.Length != 4 || dims[0] != 1)
            {
                result.LandmarkModelWarning = $"Unexpected input shape: [{string.Join(",", dims)}]. Expected [1,224,224,3] or [1,3,224,224]";
            }

            int expectedSize = 224;
            bool hasExpectedSize = (dims[1] == expectedSize && dims[2] == expectedSize) ||
                                   (dims[2] == expectedSize && dims[3] == expectedSize);

            if (!hasExpectedSize)
            {
                result.LandmarkModelWarning = $"Input size may not match expected 224x224";
            }

            // Get output metadata
            var outputMeta = session.OutputMetadata;
            result.LandmarkOutputCount = outputMeta.Count;
            result.LandmarkOutputNames = outputMeta.Keys.ToArray();

            // Try to get output shapes
            result.LandmarkOutputShapes = new int[outputMeta.Count][];
            int i = 0;
            foreach (var output in outputMeta.Values)
            {
                result.LandmarkOutputShapes[i] = output.Dimensions.ToArray();
                i++;
            }

            // MediaPipe hand landmarks output: 21 points × 3 coordinates = 63 values
            // Common formats: [1,63], [1,21,3], [1,3,21], or [1,42] (x,y only)
            bool hasValidFormat = false;
            foreach (var shape in result.LandmarkOutputShapes)
            {
                int totalElements = 1;
                foreach (var dim in shape)
                {
                    if (dim > 0) totalElements *= dim;
                }

                if (totalElements == 63 || totalElements == 42 || totalElements == 21)
                {
                    hasValidFormat = true;
                    break;
                }
            }

            if (!hasValidFormat)
            {
                result.LandmarkModelWarning = "Output shape doesn't match expected landmark format (21×3 or similar)";
            }

            return true;
        }
        catch (Exception ex)
        {
            result.LandmarkModelError = $"Failed to load landmark model: {ex.Message}";
            return false;
        }
    }

    private static bool ValidateFaceModel(string path, ModelValidationResult result)
    {
        try
        {
            if (!File.Exists(path))
            {
                result.FaceModelError = $"Model file not found: {path}";
                return false;
            }

            using var session = new InferenceSession(path);

            // Get input metadata
            var inputMeta = session.InputMetadata.FirstOrDefault();
            if (inputMeta.Value == null)
            {
                result.FaceModelError = "No input metadata found";
                return false;
            }

            result.FaceInputShape = inputMeta.Value.Dimensions.ToArray();

            // Get output count
            result.FaceOutputCount = session.OutputMetadata.Count;

            return true;
        }
        catch (Exception ex)
        {
            result.FaceModelError = $"Failed to load face model: {ex.Message}";
            return false;
        }
    }
}

/// <summary>
/// Result of ONNX model validation with detailed diagnostics.
/// </summary>
public class ModelValidationResult
{
    // Overall status
    public bool OverallValid { get; set; }

    // Palm detection model
    public bool PalmModelValid { get; set; }
    public string? PalmModelError { get; set; }
    public string? PalmModelWarning { get; set; }
    public string? PalmInputName { get; set; }
    public int[]? PalmInputShape { get; set; }
    public int PalmOutputCount { get; set; }
    public string[]? PalmOutputNames { get; set; }
    public int[][]? PalmOutputShapes { get; set; }

    // Landmark model
    public bool LandmarkModelValid { get; set; }
    public string? LandmarkModelError { get; set; }
    public string? LandmarkModelWarning { get; set; }
    public string? LandmarkInputName { get; set; }
    public int[]? LandmarkInputShape { get; set; }
    public int LandmarkOutputCount { get; set; }
    public string[]? LandmarkOutputNames { get; set; }
    public int[][]? LandmarkOutputShapes { get; set; }

    // Face detection model (optional)
    public bool FaceModelValid { get; set; }
    public string? FaceModelError { get; set; }
    public int[]? FaceInputShape { get; set; }
    public int FaceOutputCount { get; set; }

    /// <summary>
    /// Gets a formatted diagnostic report.
    /// </summary>
    public string GetDiagnosticReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== ONNX Model Validation Report ===");
        report.AppendLine();

        // Palm model
        report.AppendLine("Palm Detection Model:");
        report.AppendLine($"  Status: {(PalmModelValid ? "✓ Valid" : "✗ Invalid")}");
        if (PalmModelError != null)
            report.AppendLine($"  Error: {PalmModelError}");
        if (PalmModelWarning != null)
            report.AppendLine($"  Warning: {PalmModelWarning}");
        if (PalmInputShape != null)
            report.AppendLine($"  Input: {PalmInputName} [{string.Join(",", PalmInputShape)}]");
        if (PalmOutputShapes != null)
        {
            report.AppendLine($"  Outputs: {PalmOutputCount}");
            for (int i = 0; i < PalmOutputShapes.Length; i++)
            {
                var name = PalmOutputNames != null && i < PalmOutputNames.Length ? PalmOutputNames[i] : $"output_{i}";
                report.AppendLine($"    - {name}: [{string.Join(",", PalmOutputShapes[i])}]");
            }
        }
        report.AppendLine();

        // Landmark model
        report.AppendLine("Landmark Model:");
        report.AppendLine($"  Status: {(LandmarkModelValid ? "✓ Valid" : "✗ Invalid")}");
        if (LandmarkModelError != null)
            report.AppendLine($"  Error: {LandmarkModelError}");
        if (LandmarkModelWarning != null)
            report.AppendLine($"  Warning: {LandmarkModelWarning}");
        if (LandmarkInputShape != null)
            report.AppendLine($"  Input: {LandmarkInputName} [{string.Join(",", LandmarkInputShape)}]");
        if (LandmarkOutputShapes != null)
        {
            report.AppendLine($"  Outputs: {LandmarkOutputCount}");
            for (int i = 0; i < LandmarkOutputShapes.Length; i++)
            {
                var name = LandmarkOutputNames != null && i < LandmarkOutputNames.Length ? LandmarkOutputNames[i] : $"output_{i}";
                report.AppendLine($"    - {name}: [{string.Join(",", LandmarkOutputShapes[i])}]");
            }
        }
        report.AppendLine();

        report.AppendLine($"Overall: {(OverallValid ? "✓ Models are valid and ready" : "✗ Model validation failed")}");

        return report.ToString();
    }
}

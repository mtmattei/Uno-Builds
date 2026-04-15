using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using VoxelWarehouse.Models;

namespace VoxelWarehouse.ML;

public sealed class OnnxHandTracker : IDisposable
{
    private const int PalmInputSize = 192;
    private const int LandmarkInputSize = 224;
    private const float PalmRoiScale = 2.6f;

    private readonly InferenceSession _palmSession;
    private readonly InferenceSession _landmarkSession;
    private readonly float[] _anchors;
    private readonly string _palmInputName;
    private readonly string _landmarkInputName;
    private bool _disposed;

    private PalmDetection? _previousPalm;
    private int _framesSinceDetection;
    private const int RedetectionInterval = 10;

    // Reusable buffer for frame conversion (avoids per-frame allocation)
    private float[]? _frameFloatBuffer;

    public OnnxHandTracker(string modelDirectory)
    {
        var palmModelPath = Path.Combine(modelDirectory, "palm_detection_mediapipe_2023feb.onnx");
        var landmarkModelPath = Path.Combine(modelDirectory, "handpose_estimation_mediapipe_2023feb.onnx");
        var anchorsPath = Path.Combine(modelDirectory, "anchors.bin");

        var sessionOptions = CreateSessionOptions();

        _palmSession = new InferenceSession(palmModelPath, sessionOptions);
        _landmarkSession = new InferenceSession(landmarkModelPath, sessionOptions);

        // Auto-detect input tensor names from model metadata
        _palmInputName = _palmSession.InputMetadata.Keys.First();
        _landmarkInputName = _landmarkSession.InputMetadata.Keys.First();

        Debug.WriteLine($"[OnnxHandTracker] Palm input: '{_palmInputName}' {string.Join(",", _palmSession.InputMetadata[_palmInputName].Dimensions)}");
        Debug.WriteLine($"[OnnxHandTracker] Palm outputs: {string.Join(", ", _palmSession.OutputMetadata.Keys)}");
        Debug.WriteLine($"[OnnxHandTracker] Landmark input: '{_landmarkInputName}' {string.Join(",", _landmarkSession.InputMetadata[_landmarkInputName].Dimensions)}");
        Debug.WriteLine($"[OnnxHandTracker] Landmark outputs: {string.Join(", ", _landmarkSession.OutputMetadata.Keys)}");

        _anchors = File.Exists(anchorsPath)
            ? PalmDecoder.LoadAnchors(anchorsPath)
            : PalmDecoder.GenerateAnchors(PalmInputSize);
    }

    public HandTrackingResult ProcessFrame(byte[] rgbPixels, int width, int height)
    {
        // Normalize RGB bytes to [0,1] float — reuse buffer
        var frameTensor = ImagePreprocessor.RgbToFloat(rgbPixels, width, height, _frameFloatBuffer);
        _frameFloatBuffer = frameTensor;

        PalmDetection? palm = null;

        if (_previousPalm is null || _framesSinceDetection >= RedetectionInterval)
        {
            palm = RunPalmDetection(frameTensor, width, height);
            _framesSinceDetection = 0;
        }
        else
        {
            palm = _previousPalm;
            _framesSinceDetection++;
        }

        if (palm is null)
        {
            _previousPalm = null;
            return HandTrackingResult.None();
        }

        var landmarkResult = RunLandmarkDetection(frameTensor, width, height, palm.Value);

        if (landmarkResult is null)
        {
            _previousPalm = null;
            return HandTrackingResult.None();
        }

        var result = landmarkResult.Value;
        _previousPalm = palm;

        var gesture = GestureClassifier.Classify(result.Landmarks);
        var (cursorX, cursorY) = LandmarkDecoder.ComputeCursorPosition(result.Landmarks);

        return new HandTrackingResult(
            HandDetected: true,
            Confidence: result.Confidence,
            Landmarks: result.Landmarks,
            Gesture: gesture,
            CursorX: cursorX,
            CursorY: cursorY,
            IsLeftHand: result.Handedness < 0.5f);
    }

    private PalmDetection? RunPalmDetection(float[] frameTensor, int frameWidth, int frameHeight)
    {
        // Resize to palm model input (192×192)
        var resized = ImagePreprocessor.ResizePlanar(frameTensor, frameWidth, frameHeight, PalmInputSize, PalmInputSize);

        // Check if model expects NHWC or NCHW from metadata
        var dims = _palmSession.InputMetadata[_palmInputName].Dimensions;
        bool isNHWC = dims.Length == 4 && dims[3] == 3;

        float[] inputData;
        int[] shape;

        if (isNHWC)
        {
            inputData = ImagePreprocessor.PlanarToInterleaved(resized, PalmInputSize, PalmInputSize);
            shape = [1, PalmInputSize, PalmInputSize, 3];
        }
        else
        {
            inputData = resized;
            shape = [1, 3, PalmInputSize, PalmInputSize];
        }

        var inputTensor = new DenseTensor<float>(inputData, shape);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_palmInputName, inputTensor)
        };

        using var results = _palmSession.Run(inputs);
        var outputList = results.ToList();

        // The OpenCV palm model outputs: classificators [1, 2016, 1] and regressors [1, 2016, 18]
        // Order may vary — identify by shape
        float[] rawBoxes, rawScores;
        var out0 = ExtractFloatOutput(outputList[0]);
        var out1 = ExtractFloatOutput(outputList[1]);
        var dim0 = outputList[0].AsTensor<float>().Dimensions.ToArray();
        var dim1 = outputList[1].AsTensor<float>().Dimensions.ToArray();

        if (dim0.Length >= 3 && dim0[^1] == 18)
        {
            rawBoxes = out0;
            rawScores = out1;
        }
        else
        {
            rawBoxes = out1;
            rawScores = out0;
        }

        var detections = PalmDecoder.Decode(rawBoxes, rawScores, _anchors, PalmInputSize);
        return detections.Count > 0 ? detections[0] : null;
    }

    private LandmarkResult? RunLandmarkDetection(
        float[] frameTensor, int frameWidth, int frameHeight, PalmDetection palm)
    {
        float roiCx = palm.CenterX * frameWidth;
        float roiCy = palm.CenterY * frameHeight;
        float roiW = palm.Width * frameWidth * PalmRoiScale;
        float roiH = palm.Height * frameHeight * PalmRoiScale;

        var (forward, inverse) = ImagePreprocessor.ComputeRotatedRectWarp(
            roiCx, roiCy, roiW, roiH, palm.RotationRadians, LandmarkInputSize, LandmarkInputSize);

        var cropped = ImagePreprocessor.AffineCrop(
            frameTensor, frameWidth, frameHeight,
            LandmarkInputSize, LandmarkInputSize, inverse);

        // Check if model expects NHWC or NCHW
        var dims = _landmarkSession.InputMetadata[_landmarkInputName].Dimensions;
        bool isNHWC = dims.Length == 4 && dims[3] == 3;

        float[] inputData;
        int[] shape;

        if (isNHWC)
        {
            inputData = ImagePreprocessor.PlanarToInterleaved(cropped, LandmarkInputSize, LandmarkInputSize);
            shape = [1, LandmarkInputSize, LandmarkInputSize, 3];
        }
        else
        {
            inputData = cropped;
            shape = [1, 3, LandmarkInputSize, LandmarkInputSize];
        }

        var inputTensor = new DenseTensor<float>(inputData, shape);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_landmarkInputName, inputTensor)
        };

        using var results = _landmarkSession.Run(inputs);
        var outputList = results.ToList();

        // Landmark model outputs (OpenCV variant):
        // landmarks [1, 63], confidence [1, 1], handedness [1, 1], world_landmarks [1, 63]
        // Identify outputs by size
        float[]? rawLandmarks = null, rawConfidence = null, rawHandedness = null;

        foreach (var output in outputList)
        {
            var tensor = output.AsTensor<float>();
            int len = (int)tensor.Length;

            if (len == 63 && rawLandmarks is null)
                rawLandmarks = ExtractFloatOutput(output);
            else if (len == 63)
                continue; // world landmarks — skip
            else if (len == 1 && rawConfidence is null)
                rawConfidence = ExtractFloatOutput(output);
            else if (len == 1)
                rawHandedness = ExtractFloatOutput(output);
        }

        if (rawLandmarks is null || rawConfidence is null)
            return null;

        rawHandedness ??= [0.5f];

        return LandmarkDecoder.Decode(
            rawLandmarks, rawConfidence, rawHandedness,
            inverse, LandmarkInputSize, frameWidth, frameHeight);
    }

    private static float[] ExtractFloatOutput(DisposableNamedOnnxValue output)
    {
        var tensor = output.AsTensor<float>();
        if (tensor is DenseTensor<float> dense)
            return dense.Buffer.ToArray();

        var result = new float[tensor.Length];
        int i = 0;
        foreach (var value in tensor)
            result[i++] = value;
        return result;
    }

    private static SessionOptions CreateSessionOptions()
    {
        var options = new SessionOptions();
        options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        options.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;

        try
        {
            if (OperatingSystem.IsWindows())
                options.AppendExecutionProvider_DML(0);
        }
        catch
        {
            // DirectML not available — CPU fallback
        }

        return options;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _palmSession.Dispose();
        _landmarkSession.Dispose();
    }
}

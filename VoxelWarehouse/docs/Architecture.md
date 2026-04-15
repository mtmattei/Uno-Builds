# GRIDFORM — Voxel Spatial Editor
## Uno Platform Implementation Brief v2

---

## 1. Project Summary

**GRIDFORM** is an isometric voxel editor built with Uno Platform (C# + WinUI 3 + XAML) targeting Windows, macOS, Linux, WebAssembly, iOS, and Android. It features two input modes: standard pointer/touch interaction and a camera-overlay gesture mode where ML-powered hand landmark detection drives the cursor and gesture classification triggers placement/erasure.

The app uses **SkiaSharp** for the isometric voxel renderer (AO, edge highlights, ground shadows, depth fog), **MediaCapture** / platform camera APIs for live video feed, **ONNX Runtime** with **MediaPipe hand landmark models** for gesture recognition, and **MVUX** for reactive state management.

---

## 2. Architecture Overview

```
┌──────────────────────────────────────────────────────────┐
│                      GRIDFORM App                         │
├──────────┬──────────┬────────────┬───────────────────────┤
│  Views   │  Models  │  Services  │   Custom Controls      │
│  (XAML)  │  (MVUX)  │  (DI)     │   (SkiaSharp + ONNX)   │
├──────────┴──────────┴────────────┴───────────────────────┤
│                Uno.Extensions Host                        │
│     Navigation · DI · Configuration · Logging             │
├──────────────────────────────────────────────────────────┤
│             Uno Platform (Skia Renderer)                   │
│     Windows · macOS · Linux · WASM · iOS · Android         │
└──────────────────────────────────────────────────────────┘
```

### Key Layers

| Layer | Responsibility | Key Types |
|-------|---------------|-----------|
| **View** | XAML pages, layout, overlays | `EditorPage.xaml`, `ToolbarPanel.xaml`, `MetricsPanel.xaml` |
| **Model** | MVUX reactive state, commands | `EditorModel`, `VoxelWorldState`, `HandTrackingState` |
| **Services** | Camera, hand tracking, persistence | `ICameraService`, `OnnxHandTracker`, `IVoxelStorageService` |
| **Controls** | SkiaSharp custom renderers | `IsometricCanvasControl`, `CameraPreviewControl` |
| **ML** | ONNX inference pipeline | `PalmDetector`, `HandLandmarkDetector`, `GestureClassifier` |

---

## 3. .csproj Configuration

### UnoFeatures

```xml
<UnoFeatures>
    Extensions;
    ExtensionsCore;
    Hosting;
    Toolkit;
    Material;
    MVUX;
    Logging;
    Skia;
    SkiaRenderer;
    Configuration;
    Serialization;
    Storage;
</UnoFeatures>
```

### NuGet Packages

```xml
<!-- Rendering -->
<PackageReference Include="SkiaSharp.Views.Uno.WinUI" Version="3.*" />
<PackageReference Include="Uno.WinUI.Graphics2DSK" Version="5.*" />

<!-- ML Inference -->
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.*" />
<!-- Windows GPU acceleration (optional, Windows-only) -->
<PackageReference Include="Microsoft.ML.OnnxRuntime.DirectML" Version="1.*"
                  Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'" />

<!-- Image preprocessing -->
<PackageReference Include="SixLabors.ImageSharp" Version="3.*" />
```

`Uno.WinUI.Graphics2DSK` provides `SKCanvasElement` — hardware-accelerated SkiaSharp canvas for Skia targets, avoids buffer-copy overhead vs `SKXamlCanvas`.

---

## 4. Hand Tracking: ONNX Runtime + MediaPipe

### 4.1 Why This Approach

The MediaPipe hand pipeline is a two-stage ML model:

1. **Palm Detector** (`palm_detection.onnx`, ~2MB) — runs on full frame, outputs oriented bounding boxes
2. **Hand Landmark Model** (`hand_landmark.onnx`, ~5MB) — runs on cropped palm region, outputs 21 3D keypoints

Both models are available as ONNX exports (converted from TFLite via `tf2onnx`). ONNX Runtime is Microsoft's cross-platform inference engine with C# bindings, DirectML GPU acceleration on Windows, and CPU execution on all other platforms. This gives us:

- **Fully offline** — no API calls, no network dependency
- **Single implementation** — `Microsoft.ML.OnnxRuntime` runs on every Uno target
- **~5-10ms inference** on GPU, ~15-25ms on CPU — well within real-time budget
- **21 landmark points** — enough to classify pinch, point, open hand, fist without an additional model

### 4.2 Model Assets

Ship as embedded resources in the app package:

```
Assets/
├── Models/
│   ├── palm_detection_lite.onnx       # 2.1 MB — palm detector
│   ├── hand_landmark_lite.onnx        # 5.3 MB — 21-point landmark regressor
│   └── anchors.bin                    # pre-computed anchor boxes for palm decoder
```

Source: OpenCV's HuggingFace repo (`opencv/palm_detection_mediapipe`) provides pre-converted ONNX models. The "lite" variants are optimized for real-time mobile inference.

### 4.3 Inference Pipeline

```
Camera Frame (e.g. 640×480 BGRA)
│
├── 1. PREPROCESSING
│   ├── Convert BGRA → RGB
│   ├── Resize to 192×192 (palm detector input)
│   └── Normalize to [0, 1] float32
│
├── 2. PALM DETECTION
│   ├── InferenceSession.Run(palm_detection.onnx)
│   ├── Decode raw outputs against pre-computed anchors
│   ├── Apply sigmoid to confidence scores
│   ├── Filter by confidence threshold (> 0.5)
│   └── Non-Maximum Suppression → best palm bbox + 7 keypoints
│
├── 3. CROP + WARP
│   ├── Compute rotation angle from palm keypoints (wrist → middle finger)
│   ├── Expand bbox by 2.6× (hand extends beyond palm)
│   ├── Affine warp to axis-aligned 224×224 crop
│   └── Normalize to [0, 1] float32
│
├── 4. HAND LANDMARK DETECTION
│   ├── InferenceSession.Run(hand_landmark.onnx)
│   ├── Output: 21 landmarks × 3 (x, y, z) normalized
│   ├── Output: hand presence confidence
│   ├── Output: handedness (left/right)
│   └── Inverse-warp landmarks back to camera coordinates
│
└── 5. GESTURE CLASSIFICATION (pure geometry, no ML)
    ├── Compute thumb tip (4) ↔ index tip (8) distance → pinch
    ├── Check finger extension states → point / open / fist
    ├── Map landmark centroid to screen coordinates → cursor position
    └── Emit HandTrackingResult
```

### 4.4 Core Types

```csharp
/// The 21 MediaPipe hand landmarks
public enum HandLandmarkId
{
    Wrist = 0,
    ThumbCmc = 1, ThumbMcp = 2, ThumbIp = 3, ThumbTip = 4,
    IndexMcp = 5, IndexPip = 6, IndexDip = 7, IndexTip = 8,
    MiddleMcp = 9, MiddlePip = 10, MiddleDip = 11, MiddleTip = 12,
    RingMcp = 13, RingPip = 14, RingDip = 15, RingTip = 16,
    PinkyMcp = 17, PinkyPip = 18, PinkyDip = 19, PinkyTip = 20,
}

public readonly record struct Landmark3D(float X, float Y, float Z);

public record HandTrackingResult(
    bool HandDetected,
    float Confidence,
    Landmark3D[] Landmarks,       // 21 points, normalized 0..1
    GestureType Gesture,
    float CursorX,                // mapped to screen space
    float CursorY,
    bool IsLeftHand
);

public enum GestureType
{
    None,       // no hand detected
    Open,       // all fingers extended — hover/move
    Pinch,      // thumb + index touching — place voxel
    Point,      // index extended, others curled — select
    Fist,       // all fingers curled — erase
}
```

### 4.5 OnnxHandTracker Implementation

```csharp
public sealed class OnnxHandTracker : IDisposable
{
    private readonly InferenceSession _palmSession;
    private readonly InferenceSession _landmarkSession;
    private readonly float[] _anchors;
    private Landmark3D[]? _prevLandmarks;   // for tracking between frames

    public OnnxHandTracker(string modelDirectory)
    {
        var sessionOptions = new SessionOptions();

        // Use DirectML on Windows for GPU acceleration, CPU everywhere else
        if (OperatingSystem.IsWindows())
        {
            try { sessionOptions.AppendExecutionProvider_DML(0); }
            catch { /* fall through to CPU */ }
        }

        sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        sessionOptions.InterOpNumThreads = 2;
        sessionOptions.IntraOpNumThreads = 2;

        _palmSession = new InferenceSession(
            Path.Combine(modelDirectory, "palm_detection_lite.onnx"), sessionOptions);
        _landmarkSession = new InferenceSession(
            Path.Combine(modelDirectory, "hand_landmark_lite.onnx"), sessionOptions);
        _anchors = LoadAnchors(Path.Combine(modelDirectory, "anchors.bin"));
    }

    public HandTrackingResult ProcessFrame(ReadOnlySpan<byte> rgbPixels, int width, int height)
    {
        // 1. Preprocess for palm detection
        var palmInput = PreprocessForPalm(rgbPixels, width, height);

        // 2. Run palm detection
        using var palmResults = _palmSession.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor("input", palmInput)
        });

        var palmBoxes = DecodePalmDetections(palmResults, _anchors);
        if (palmBoxes.Length == 0)
            return new(false, 0f, Array.Empty<Landmark3D>(), GestureType.None, 0, 0, false);

        var bestPalm = palmBoxes[0]; // highest confidence after NMS

        // 3. Crop and warp for landmark detection
        var (landmarkInput, warpMatrix) = CropAndWarp(rgbPixels, width, height, bestPalm);

        // 4. Run landmark detection
        using var lmResults = _landmarkSession.Run(new[]
        {
            NamedOnnxValue.CreateFromTensor("input", landmarkInput)
        });

        var landmarks = DecodeLandmarks(lmResults, warpMatrix);
        var confidence = ExtractConfidence(lmResults);

        if (confidence < 0.5f)
            return new(false, confidence, Array.Empty<Landmark3D>(), GestureType.None, 0, 0, false);

        // 5. Classify gesture from geometry
        var gesture = GestureClassifier.Classify(landmarks);

        // 6. Map index finger tip to screen cursor (or centroid for open hand)
        var (cx, cy) = gesture == GestureType.Point
            ? (landmarks[(int)HandLandmarkId.IndexTip].X, landmarks[(int)HandLandmarkId.IndexTip].Y)
            : ComputeCentroid(landmarks);

        var isLeft = DetermineHandedness(lmResults);

        _prevLandmarks = landmarks;

        return new(true, confidence, landmarks, gesture, cx, cy, isLeft);
    }

    public void Dispose()
    {
        _palmSession.Dispose();
        _landmarkSession.Dispose();
    }
}
```

### 4.6 Gesture Classification (Pure Geometry)

No additional ML model — just distance and angle checks on the 21 landmarks:

```csharp
public static class GestureClassifier
{
    private const float PinchThreshold = 0.06f;

    public static GestureType Classify(Landmark3D[] lm)
    {
        var pinchDist = Distance(lm[(int)HandLandmarkId.ThumbTip],
                                  lm[(int)HandLandmarkId.IndexTip]);

        if (pinchDist < PinchThreshold)
            return GestureType.Pinch;

        bool indexUp   = IsFingerExtended(lm, HandLandmarkId.IndexMcp,  HandLandmarkId.IndexTip);
        bool middleUp  = IsFingerExtended(lm, HandLandmarkId.MiddleMcp, HandLandmarkId.MiddleTip);
        bool ringUp    = IsFingerExtended(lm, HandLandmarkId.RingMcp,   HandLandmarkId.RingTip);
        bool pinkyUp   = IsFingerExtended(lm, HandLandmarkId.PinkyMcp,  HandLandmarkId.PinkyTip);

        if (indexUp && !middleUp && !ringUp && !pinkyUp)
            return GestureType.Point;

        if (!indexUp && !middleUp && !ringUp && !pinkyUp)
            return GestureType.Fist;

        if (indexUp && middleUp && ringUp && pinkyUp)
            return GestureType.Open;

        return GestureType.Open; // default to tracking mode
    }

    private static bool IsFingerExtended(Landmark3D[] lm, HandLandmarkId mcp, HandLandmarkId tip)
    {
        // Tip is further from wrist than MCP → extended
        var wrist = lm[(int)HandLandmarkId.Wrist];
        return Distance(lm[(int)tip], wrist) > Distance(lm[(int)mcp], wrist) * 1.3f;
    }

    private static float Distance(Landmark3D a, Landmark3D b) =>
        MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
}
```

### 4.7 Gesture → Voxel Action Mapping

| Gesture | Detected By | Editor Action |
|---------|-------------|---------------|
| **Open hand** | All fingers extended | **Move cursor** — landmark centroid maps to grid position |
| **Pinch** | Thumb-index distance < threshold | **Place voxel** at cursor position |
| **Fist** | All fingers curled | **Erase voxel** at cursor position |
| **Point** | Index extended, rest curled | **Select / inspect** — shows cell info tooltip |

### 4.8 Frame Processing Loop

Runs on a dedicated background thread to avoid blocking the UI:

```csharp
public sealed class HandTrackingLoop : IDisposable
{
    private readonly OnnxHandTracker _tracker;
    private readonly ICameraService _camera;
    private readonly CancellationTokenSource _cts = new();
    private Task? _processingTask;

    public IState<HandTrackingResult> LatestResult { get; }

    public void Start()
    {
        _processingTask = Task.Run(async () =>
        {
            await foreach (var frame in _camera.GetFrames(_cts.Token))
            {
                var result = _tracker.ProcessFrame(frame.RgbPixels, frame.Width, frame.Height);
                await LatestResult.Set(result);
            }
        }, _cts.Token);
    }

    // Camera delivers frames at ~30fps
    // Palm detection: ~8ms (GPU) / ~20ms (CPU)
    // Landmark detection: ~5ms (GPU) / ~12ms (CPU)
    // Total: ~13ms (GPU) / ~32ms (CPU) — comfortably real-time
}
```

### 4.9 Platform Execution Providers

| Platform | ONNX Execution Provider | Expected Latency |
|----------|------------------------|-------------------|
| **Windows** | DirectML (GPU/NPU) | ~8-13ms total |
| **macOS** | CoreML EP (if available) or CPU | ~15-25ms |
| **Linux** | CPU (OpenMP threads) | ~20-32ms |
| **WASM** | WASM SIMD (ort-web) | ~30-50ms |
| **iOS** | CoreML EP | ~10-18ms |
| **Android** | NNAPI EP | ~12-20ms |

All latencies are per-frame for the full palm + landmark pipeline on typical hardware. Even the worst case (WASM CPU) stays under 50ms, which is 20fps for the gesture layer — acceptable since the pointer input remains 60fps regardless.

---

## 5. Data Model

### 5.1 Voxel World

```csharp
public record VoxelCell(int X, int Y, int Z, AssetType Asset);

public enum AssetType
{
    Pallet,     // 1.2t, warm gray
    Rack,       // 0.4t, cool gray
    Container,  // 2.5t, neutral
    Equipment,  // 3.0t, dark
    Aisle       // 0.0t, floor marker
}

public record VoxelWorldState(
    IImmutableDictionary<(int X, int Y, int Z), AssetType> Cells,
    IImmutableDictionary<(int X, int Z), ZoneType> Zones,
    int ActiveLayer,
    AssetType ActiveAsset,
    ToolMode ActiveTool,
    EditorMode ActiveMode
);

public enum ZoneType { None, Receiving, Storage, Staging, Shipping }
public enum ToolMode { Place, Erase }
public enum EditorMode { Build, Zone }
```

### 5.2 Grid Constants

```csharp
public static class GridConstants
{
    public const int GridSize = 14;
    public const int MaxHeight = 6;
    public const int HalfWidth = 22;
    public const int HalfHeight = 11;
    public const int VoxelDepth = 18;
}
```

### 5.3 Metrics (Computed)

```csharp
public record UtilizationMetrics(
    double FloorUtilPercent,
    double VolumeUtilPercent,
    double TotalWeightTons,
    int PeakHeight,
    int TotalUnits,
    IImmutableDictionary<AssetType, int> AssetCounts,
    IImmutableDictionary<ZoneType, int> ZoneCellCounts
);
```

---

## 6. MVUX Model

```csharp
public partial record EditorModel
{
    public IState<VoxelWorldState> World => State.Value(this, () => Presets.WarehouseA());
    public IState<HandTrackingResult> HandState => State.Value(this, () =>
        new HandTrackingResult(false, 0, Array.Empty<Landmark3D>(), GestureType.None, 0, 0, false));
    public IState<(int GX, int GZ)> CursorPosition => State.Value(this, () => (6, 6));
    public IState<bool> ShowMetrics => State.Value(this, () => true);
    public IState<bool> CameraEnabled => State.Value(this, () => false);

    public IFeed<UtilizationMetrics> Metrics => World.Select(w => MetricsCalculator.Compute(w));

    public async ValueTask PlaceVoxel(int gx, int gz)
    {
        var w = await World;
        var key = (gx, w.ActiveLayer, gz);
        await World.Update(s => s.ActiveTool == ToolMode.Place
            ? s with { Cells = s.Cells.SetItem(key, s.ActiveAsset) }
            : s with { Cells = s.Cells.Remove(key) });
    }

    public async ValueTask HandleGestureAction(HandTrackingResult hand)
    {
        if (!hand.HandDetected) return;
        var (gx, gz) = IsoMath.FromScreenToGrid(hand.CursorX, hand.CursorY, ...);
        await CursorPosition.Set((gx, gz));

        switch (hand.Gesture)
        {
            case GestureType.Pinch:
                await World.Update(s => s with {
                    Cells = s.Cells.SetItem((gx, s.ActiveLayer, gz), s.ActiveAsset) });
                break;
            case GestureType.Fist:
                await World.Update(s => s with {
                    Cells = s.Cells.Remove((gx, s.ActiveLayer, gz)) });
                break;
        }
    }

    public async ValueTask SetLayer(int layer) =>
        await World.Update(s => s with { ActiveLayer = Math.Clamp(layer, 0, GridConstants.MaxHeight) });

    public async ValueTask SetTool(ToolMode tool) =>
        await World.Update(s => s with { ActiveTool = tool });

    public async ValueTask SetAsset(AssetType asset) =>
        await World.Update(s => s with { ActiveAsset = asset });

    public async ValueTask SetMode(EditorMode mode) =>
        await World.Update(s => s with { ActiveMode = mode });

    public async ValueTask LoadPreset(string name) =>
        await World.Set(Presets.Get(name));

    public async ValueTask ToggleCamera() =>
        await CameraEnabled.Update(v => !v);
}
```

---

## 7. Isometric Renderer — `IsometricCanvasControl`

### 7.1 Base Class Selection

| Target | Base Class | Why |
|--------|-----------|------|
| Desktop (Skia) | `SKCanvasElement` | HW accelerated, no buffer copy |
| Mobile + WASM | `SKXamlCanvas` | Broader compat |

### 7.2 Render Pipeline (per `RenderOverride`)

```
 1. Camera frame background (desaturated, dimmed via SKColorFilter)
 2. Scanline + vignette overlays
 3. Zone tiles on ground plane
 4. Ground grid (layer 0)
 5. Ground shadow pass (voxels at y > 0 project shadow diamonds)
 6. Active layer grid (if layer > 0)
 7. Sort voxels by painter's algorithm: (gx + gz) asc, y asc
 8. Per voxel: AO × fog × asset color → 3 iso faces + edge highlights + contact shadow
 9. Stacking preview (dashed wireframes from ground to cursor layer)
10. Ghost cursor voxel
11. Crosshair overlay
12. Hand skeleton overlay (21 landmarks connected by bones, when camera active)
```

### 7.3 Hand Skeleton Overlay

When camera mode is active, draw the 21 detected landmarks and bone connections directly on the SkiaSharp canvas, providing visual feedback that tracking is working:

```csharp
private void DrawHandSkeleton(SKCanvas canvas, HandTrackingResult hand, Size area)
{
    if (!hand.HandDetected) return;

    var lm = hand.Landmarks;
    using var dotPaint = new SKPaint { Color = SKColors.White.WithAlpha(180), IsAntialias = true };
    using var bonePaint = new SKPaint { Color = SKColors.White.WithAlpha(80), StrokeWidth = 1.5f,
                                         IsAntialias = true, Style = SKPaintStyle.Stroke };

    // Draw bone connections
    int[][] bones = {
        new[] {0,1,2,3,4}, new[] {0,5,6,7,8}, new[] {0,9,10,11,12},
        new[] {0,13,14,15,16}, new[] {0,17,18,19,20}, new[] {5,9,13,17}
    };

    foreach (var chain in bones)
    {
        for (int i = 0; i < chain.Length - 1; i++)
        {
            var a = lm[chain[i]]; var b = lm[chain[i + 1]];
            canvas.DrawLine(a.X * (float)area.Width, a.Y * (float)area.Height,
                            b.X * (float)area.Width, b.Y * (float)area.Height, bonePaint);
        }
    }

    // Draw landmark dots
    foreach (var l in lm)
        canvas.DrawCircle(l.X * (float)area.Width, l.Y * (float)area.Height, 3f, dotPaint);

    // Highlight active gesture landmarks
    if (hand.Gesture == GestureType.Pinch)
    {
        using var pinchPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        var thumb = lm[4]; var index = lm[8];
        canvas.DrawCircle(thumb.X * (float)area.Width, thumb.Y * (float)area.Height, 6f, pinchPaint);
        canvas.DrawCircle(index.X * (float)area.Width, index.Y * (float)area.Height, 6f, pinchPaint);
    }
}
```

### 7.4 Ambient Occlusion

Per-face AO checks adjacent voxels and accumulates an occlusion factor (0 = full shadow, 1 = clear):

```csharp
public static float ComputeAO_Top(IImmutableDictionary<(int, int, int), AssetType> cells, int x, int y, int z)
{
    if (cells.ContainsKey((x, y + 1, z))) return 0.35f;
    float occ = 0f;
    if (cells.ContainsKey((x - 1, y + 1, z))) occ += 0.12f;
    if (cells.ContainsKey((x + 1, y + 1, z))) occ += 0.12f;
    if (cells.ContainsKey((x, y + 1, z - 1))) occ += 0.12f;
    if (cells.ContainsKey((x, y + 1, z + 1))) occ += 0.12f;
    if (cells.ContainsKey((x - 1, y, z)))     occ += 0.06f;
    if (cells.ContainsKey((x, y, z - 1)))     occ += 0.06f;
    return MathF.Max(0f, 1f - occ);
}
```

Left and right face AO follow the same pattern, checking neighbors in the respective directions.

---

## 8. Input System

### 8.1 Pointer Input (Always Active)

Standard WinUI/Uno pointer events on the canvas control:

```xml
<local:IsometricCanvasControl
    PointerMoved="OnPointerMoved"
    PointerPressed="OnPointerPressed"
    RightTapped="OnRightTapped"
    ManipulationMode="TranslateX,TranslateY,Scale"
    ManipulationDelta="OnManipulationDelta" />
```

- `PointerMoved` → update cursor grid position via inverse isometric projection
- `PointerPressed` → place/erase at cursor
- `RightTapped` → erase at cursor
- `ManipulationDelta` → pinch-to-zoom, pan offset
- `PointerWheelChanged` → cycle layers

Uno's pointer events map to native touch on every platform (`Touches*` on iOS, `dispatchTouchEvent` on Android, `pointer*` on WASM, managed on Skia).

### 8.2 Gesture Input (When Camera Enabled)

The `HandTrackingLoop` processes camera frames on a background thread and pushes `HandTrackingResult` into `EditorModel.HandState`. The model reacts:

```csharp
// In EditorModel — subscribing to hand state changes
public async ValueTask OnHandStateChanged(HandTrackingResult hand)
{
    if (!hand.HandDetected) return;

    // Map hand cursor to isometric grid
    var (gx, gz) = IsoMath.FromScreenToGrid(
        hand.CursorX * canvasWidth, hand.CursorY * canvasHeight,
        World.ActiveLayer, originX, originY);
    await CursorPosition.Set((Math.Clamp(gx, 0, GridConstants.GridSize - 1),
                               Math.Clamp(gz, 0, GridConstants.GridSize - 1)));

    // Gesture → action
    await HandleGestureAction(hand);
}
```

Both input modes coexist — pointer input always works, gesture input overlays when camera is enabled. No mode switching needed.

---

## 9. Camera Service

```csharp
public interface ICameraService
{
    bool IsSupported { get; }
    Task<bool> StartAsync();
    Task StopAsync();
    IAsyncEnumerable<CameraFrame> GetFrames(CancellationToken ct);
}

public record CameraFrame(byte[] RgbPixels, int Width, int Height);
```

| Platform | Implementation |
|----------|---------------|
| **Windows** | `MediaCapture` → `MediaFrameSource` → `SoftwareBitmap` → RGB bytes |
| **Android** | `CameraX` ImageAnalysis → YUV→RGB conversion |
| **iOS** | `AVCaptureSession` → `CVPixelBuffer` → RGB bytes |
| **WASM** | JS interop: `getUserMedia` → OffscreenCanvas → `getImageData` |
| **macOS/Linux** | `MediaCapture` or platform camera lib → frame buffer |

The camera delivers 640×480 at ~30fps. The hand tracker downscales internally to 192×192 for palm detection, so input resolution isn't a bottleneck.

---

## 10. View Layer (XAML)

```xml
<Page x:Class="GridForm.EditorPage"
      xmlns:local="using:GridForm.Controls"
      xmlns:utu="using:Uno.Toolkit.UI">

    <Grid>
        <MediaPlayerElement x:Name="CameraPreview"
                            Visibility="{Binding CameraEnabled}"
                            Stretch="UniformToFill"
                            AutoPlay="True" />

        <local:IsometricCanvasControl x:Name="VoxelCanvas"
                                       World="{Binding World}"
                                       CursorPosition="{Binding CursorPosition}"
                                       HandState="{Binding HandState}" />

        <!-- HUD omitted for brevity — same as v1 brief -->
    </Grid>
</Page>
```

---

## 11. Services Registration

```csharp
public static IHostBuilder ConfigureGridForm(this IHostBuilder builder) =>
    builder
        .UseConfiguration()
        .UseLogging()
        .UseSerialization()
        .ConfigureServices(services =>
        {
            // Camera
            services.AddSingleton<ICameraService, CameraService>();

            // Hand tracking — single ONNX-based implementation
            services.AddSingleton<OnnxHandTracker>(sp =>
                new OnnxHandTracker(Path.Combine(
                    Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
                    "Assets", "Models")));
            services.AddSingleton<HandTrackingLoop>();

            // Persistence
            services.AddSingleton<IVoxelStorageService, JsonVoxelStorageService>();
            services.AddSingleton<IPresetLibrary, BuiltInPresets>();
        });
```

---

## 12. File Structure

```
GridForm/
├── App.xaml / App.xaml.cs
├── GlobalUsings.cs
├── GridForm.csproj
│
├── Models/
│   ├── EditorModel.cs              # MVUX model
│   ├── VoxelWorldState.cs          # Immutable state records
│   ├── HandTrackingResult.cs       # ML output types
│   ├── MetricsCalculator.cs        # Derived computations
│   └── Presets.cs                  # Built-in layouts
│
├── Controls/
│   ├── IsometricCanvasControl.cs   # SKCanvasElement subclass
│   ├── IsoMath.cs                  # Coordinate conversions
│   ├── VoxelRenderer.cs            # Draw functions (AO, edges, shadows, fog)
│   ├── HandSkeletonRenderer.cs     # 21-point landmark overlay
│   ├── MetricsPanel.xaml           # Utilization sidebar
│   ├── LayerControl.xaml           # Vertical layer selector
│   └── ToolbarPanel.xaml           # Bottom toolbar
│
├── ML/
│   ├── OnnxHandTracker.cs          # Two-stage inference pipeline
│   ├── PalmDecoder.cs              # Anchor decoding + NMS for palm detection
│   ├── LandmarkDecoder.cs          # Landmark output parsing + inverse warp
│   ├── GestureClassifier.cs        # Geometry-based gesture recognition
│   └── HandTrackingLoop.cs         # Background frame processing loop
│
├── Services/
│   ├── ICameraService.cs
│   ├── CameraService.cs            # Platform-conditional camera
│   ├── IVoxelStorageService.cs
│   └── JsonVoxelStorageService.cs  # Save/load layouts as JSON
│
├── Views/
│   └── EditorPage.xaml / .cs       # Main editor page
│
├── Themes/
│   ├── ColorPaletteOverride.xaml   # Monochrome palette
│   └── TextBlock.xaml              # Typography overrides
│
└── Assets/
    ├── Models/
    │   ├── palm_detection_lite.onnx
    │   ├── hand_landmark_lite.onnx
    │   └── anchors.bin
    └── Icons/
```

---

## 13. Performance Strategy

| Technique | Impact |
|-----------|--------|
| **Dirty-flag invalidation** | `Invalidate()` only on state change — no continuous repaint |
| **Cached voxel bitmap** | Static geometry cached as `SKBitmap`, only cursor/overlay redrawn per frame |
| **Occlusion culling** | Skip fully surrounded voxels — 40-60% fewer draw calls in dense builds |
| **Sort-once** | Painter's algorithm sort runs only when voxels change |
| **Background ML thread** | ONNX inference never blocks the UI thread |
| **Frame skipping** | If ML inference takes >33ms, skip the oldest queued frame |
| **Session warmup** | First `InferenceSession.Run` is slow (JIT compile); run a dummy frame at startup |
| **DirectML on Windows** | GPU-accelerated inference via `AppendExecutionProvider_DML` |

---

## 14. Future: Custom Gestures via Roboflow

The Roboflow pipeline becomes relevant if you need domain-specific gestures beyond what MediaPipe landmarks + geometry can classify (e.g., "two-finger swipe to rotate view" or "L-shape to toggle tool"). The workflow:

1. Capture gesture training data from GRIDFORM's camera feed
2. Label in Roboflow (bounding boxes or keypoints)
3. Train with RF-DETR on Roboflow
4. Export to ONNX
5. Drop the `.onnx` into `Assets/Models/` and load via a third `InferenceSession`

This doesn't replace the MediaPipe pipeline — it supplements it with a custom gesture classifier running on the same ONNX Runtime infrastructure.

---

## 15. Key API References

| API | Package | Usage |
|-----|---------|-------|
| `SKCanvasElement` | `Uno.WinUI.Graphics2DSK` | HW-accelerated SkiaSharp canvas |
| `SKXamlCanvas` | `SkiaSharp.Views.Uno.WinUI` | Fallback SkiaSharp canvas |
| `InferenceSession` | `Microsoft.ML.OnnxRuntime` | Load + run ONNX models |
| `OrtValue` / `Tensor<float>` | `Microsoft.ML.OnnxRuntime` | Model input/output tensors |
| `DirectML EP` | `Microsoft.ML.OnnxRuntime.DirectML` | GPU acceleration on Windows |
| `PointerMoved/Pressed` | `Microsoft.UI.Xaml.UIElement` | Pointer input |
| `ManipulationDelta` | `Microsoft.UI.Xaml.UIElement` | Multi-touch pan/zoom |
| `MediaCapture` | `Windows.Media.Capture` | Camera frame access |
| `MediaPlayerElement` | `Microsoft.UI.Xaml.Controls` | Camera preview in XAML |
| `IState<T>` / `IFeed<T>` | Uno MVUX Extensions | Reactive state |

# UnoVox - Desktop Voxel Editor

## Problem Statement

Create a cross-platform 3D voxel editor with webcam-based hand tracking for gesture controls. The application demonstrates Uno Platform's ability to integrate custom SkiaSharp rendering, real-time webcam capture, and MediaPipe hand tracking for an innovative hands-free editing experience.

**Target user**: Developers exploring creative applications, 3D visualization, or gesture-based interfaces with Uno Platform.

## Design Brief

**Key characteristics**:
- Dark theme optimized for creative work
- 32x32x32 voxel grid with isometric projection
- Left panel: Main viewport with SkiaSharp 3D rendering
- Right panel: Webcam preview with hand skeleton overlay
- Top toolbar: File operations, tool selection, color picker
- Bottom status bar: Cursor position, FPS, tool info

**Visual Style**:
- Slate/dark color palette (`#1e293b`, `#0f172a`)
- Accent colors for tools and highlights
- Grid floor with coordinate axes
- Semi-transparent UI panels

## Architecture

**Project Structure**:
```
UnoVox/
├── Models/
│   ├── Voxel.cs                # Single voxel (position, color, active)
│   ├── VoxelGrid.cs            # 32×32×32 voxel grid manager
│   ├── VoxelCommand.cs         # Command pattern for undo/redo
│   ├── UndoStack.cs            # 10-step undo/redo history
│   └── CameraController.cs     # Camera position and controls
├── Services/
│   ├── VoxelRenderer.cs        # SkiaSharp 3D rendering
│   └── Endpoints/              # MediaPipe integration
├── Presentation/
│   ├── VoxelEditorPage.xaml    # Main editor UI
│   ├── VoxelEditorViewModel.cs # Editor view model
│   └── MainPage.xaml           # Landing page
└── Styles/
    └── ColorPaletteOverride.xaml
```

**Key Decisions**:
- SkiaSharp for custom 3D voxel rendering (isometric projection)
- Command pattern for undo/redo operations (10-step history)
- MVVM architecture with CommunityToolkit.Mvvm
- JSON file format for save/load (active voxels only)
- MediaPipe.NET for hand landmark detection (future phases)

## Implementation Phases

### Phase 1 - Project Foundation ✅
- Uno Platform project setup with SkiaSharp
- Core data structures (Voxel, VoxelGrid, UndoStack, CameraController)
- XAML layout with viewport, panels, and toolbar
- Mouse-based camera controls (orbit, pan, zoom)
- Basic isometric voxel grid rendering

### Phase 2 - Mouse-Based Editing (Next)
- Ray casting from mouse to voxel grid
- Voxel selection and highlighting
- Place/remove voxel operations
- Color palette integration
- Undo/redo with keyboard shortcuts

### Phase 3 - File System
- JSON serialization/deserialization
- File picker (New/Open/Save/Save As)
- Dirty state tracking

### Phase 4-7 - Hand Tracking
- Webcam capture integration
- MediaPipe hand landmark detection
- Gesture recognition (pinch, open hand, fist)
- Hand-to-voxel coordinate mapping

## Validation Notes

- Platform: Desktop (net9.0-desktop)
- SkiaSharp canvas for 3D rendering
- Mouse controls: Right-drag orbit, middle-drag pan, wheel zoom
- Keyboard: Ctrl+Z/Y undo/redo, P/E place/erase mode, R reset camera
- Target: 60 FPS rendering, 30 FPS hand tracking

## Technical Recap

**Platform targets**:
- [ ] WebAssembly
- [x] Windows
- [ ] iOS
- [ ] Android
- [x] macOS
- [x] Linux

**Architecture pattern**:
- [ ] MVUX
- [x] MVVM
- [ ] Other

**Key Uno Platform features**:
- SKXamlCanvas for SkiaSharp integration
- Custom rendering with SKCanvas
- Command bindings with keyboard accelerators
- Material theme resources

**Uno Toolkit components**:
- Material theme
- Color palette customization

**Third-party integrations**:
- SkiaSharp - 2D/3D rendering
- ONNX Runtime - Local hand detection models
- OpenCvSharp - Webcam capture and image processing
- Roboflow - Cloud-based gesture classification (optional)

## Controls

### Mouse
| Action | Control |
|--------|---------|
| Orbit camera | Right-drag |
| Pan camera | Middle-drag or Shift+Right-drag |
| Zoom | Mouse wheel |
| Place voxel | Left-click (Phase 2) |
| Remove voxel | Right-click (Phase 2) |

### Keyboard
| Shortcut | Action |
|----------|--------|
| Ctrl+N | New project |
| Ctrl+O | Open project |
| Ctrl+S | Save project |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| P | Place mode |
| E | Erase mode |
| R | Reset camera |

## File Format

```json
{
  "version": "1.0",
  "gridSize": 32,
  "voxels": [
    {"x": 0, "y": 0, "z": 0, "color": "#FF0000"}
  ],
  "palette": [
    "#FF0000", "#00FF00", "#0000FF",
    "#FFFF00", "#FF00FF", "#00FFFF",
    "#FF8000", "#8000FF", "#FFFFFF",
    "#808080", "#404040", "#008000"
  ]
}
```

## Building and Running

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Build
```powershell
cd UnoVox
dotnet build UnoVox/UnoVox.csproj -f net9.0-desktop
```

### Run
```powershell
dotnet run --project UnoVox/UnoVox.csproj -f net9.0-desktop
```

## Performance Targets

| Metric | Target |
|--------|--------|
| Rendering | 60 FPS |
| Hand tracking (local) | 30 FPS |
| Roboflow latency | 100-300ms |
| Input latency | <50ms |
| Memory | Active voxels only |

## Roboflow Integration

UnoVox includes optional cloud-based gesture classification via [Roboflow](https://roboflow.com). This provides an additional layer of gesture recognition that runs in parallel with local detection.

### Configuration

1. Get a free API key from [Roboflow](https://app.roboflow.com)
2. Set the API key via one of these methods:

**Option A: Environment Variable (Recommended for demos)**
```powershell
$env:ROBOFLOW_API_KEY = "your_api_key_here"
dotnet run --project UnoVox/UnoVox.csproj -f net9.0-desktop
```

**Option B: appsettings.json**
```json
{
  "Roboflow": {
    "ApiKey": "your_api_key_here",
    "ModelEndpoint": "hand-gesture-recognition-9ldly/1",
    "MinConfidence": 0.4,
    "ThrottleIntervalMs": 300
  }
}
```

### How It Works

| Component | Function |
|-----------|----------|
| Local ONNX | Fast hand detection and landmark tracking (~30 FPS) |
| Roboflow Cloud | Gesture classification with pre-trained models (~3 calls/sec) |
| Hybrid Mode | Local for position, cloud for gesture accuracy |

### Supported Gestures

| Roboflow Class | Maps To | Action |
|----------------|---------|--------|
| palm, open_hand | OpenPalm | Navigation mode |
| fist, closed_fist | ClosedFist | Erase voxels |
| pinch, ok_sign | Pinch | Place voxels |
| pointing, one | Point | Cycle colors |
| thumbs_up | ThumbsUp | (Reserved) |

### Cost Considerations

- Roboflow free tier: 1,000 credits/month
- Throttling: Max 3 API calls/second (configurable)
- ROI cropping: Reduces bandwidth by sending only hand region

## Twitter Caption

*Ready-to-post caption for video recording:*

Built a 3D voxel editor with Uno Platform! Custom SkiaSharp rendering, hand tracking with ONNX, and Roboflow cloud gesture recognition - all running cross-platform. Draw voxels with pinch gestures, erase with fist!

#UnoPlatform #dotnet #XAML #SkiaSharp #Roboflow #ComputerVision #CrossPlatform

## Assets

- [Screenshots](*.png) - Development screenshots
- [Source code](UnoVox/) - Complete Uno Platform project

## Lessons Learned

- SkiaSharp integrates seamlessly with Uno Platform via SKXamlCanvas
- Isometric projection creates effective 3D visualization without full 3D engine
- Command pattern with capped stack works well for creative tool undo/redo
- Camera controller abstraction simplifies orbit/pan/zoom implementation
- Hybrid local/cloud gesture recognition provides best balance of speed and accuracy
- Roboflow REST API integrates easily with C# HttpClient
- Throttling and ROI cropping are essential for managing cloud API costs

## Known Issues

- Camera controls are right-click orbit (not yet configurable)
- Roboflow requires internet connection (falls back to local detection when offline)
- Some Roboflow gesture classes may not map perfectly to app gestures

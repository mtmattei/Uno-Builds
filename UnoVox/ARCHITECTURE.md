# UnoVox Architecture Guide

## Overview

UnoVox is built using the MVVM (Model-View-ViewModel) pattern with a clean separation of concerns. This document explains the architecture, design decisions, and how everything fits together.

## Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  (XAML Views + ViewModels + Event Handlers)             │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│                    Service Layer                         │
│  (VoxelRenderer + Future: FileService, CameraService)   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│                    Model Layer                           │
│  (VoxelGrid, Voxel, CameraController, Commands)         │
└─────────────────────────────────────────────────────────┘
```

## Core Components

### 1. Models (Data Layer)

#### **Voxel** (struct)
```csharp
public struct Voxel
{
    public int X, Y, Z;        // Grid position
    public string Color;        // Hex color string
    public bool IsActive;       // Visibility flag
}
```

**Design Decisions:**
- **Struct vs Class**: Voxel is a value type for memory efficiency
- **Color as String**: Simplifies JSON serialization and cross-platform compatibility
- **IsActive Flag**: Allows "soft delete" for undo/redo optimization

#### **VoxelGrid** (class)
```csharp
public class VoxelGrid
{
    private Dictionary<(int x, int y, int z), Voxel> _voxels;
    public int Size { get; } = 32;
}
```

**Design Decisions:**
- **Dictionary Storage**: Only stores active voxels (sparse array)
  - Memory: 32³ = 32,768 possible positions
  - With dictionary: Only store actual voxels (~100-1000 typical)
  - Saves ~99% memory for typical scenes
- **Tuple Key**: Fast lookup, no custom hash code needed
- **Size Property**: Allows future grid size flexibility

**Performance:**
- Lookup: O(1) average
- Insert: O(1) average
- Remove: O(1) average
- Memory: O(n) where n = active voxels

#### **CameraController** (class)
```csharp
public class CameraController
{
    public float RotationX, RotationY;  // Pitch, Yaw
    public float Zoom;
    public float PanX, PanY;
}
```

**Camera Math:**
1. **Orbit**: Rotate around center point
   - Yaw (Y-axis rotation): 0° to 360°
   - Pitch (X-axis rotation): -89° to +89° (prevent gimbal lock)
   
2. **Pan**: Translate in screen space
   - Independent of rotation
   - Scaled by zoom level
   
3. **Zoom**: Distance from center
   - Min: 10 (close up)
   - Max: 200 (far away)
   - Default: 50 (balanced view)

#### **Command Pattern**
```csharp
interface IVoxelCommand
{
    void Execute(VoxelGrid grid);
    void Undo(VoxelGrid grid);
}
```

**Why Command Pattern?**
- Encapsulates operations as objects
- Enables undo/redo without coupling to grid
- Easy to extend with new command types
- Can log/serialize commands for replay

**UndoStack Implementation:**
```
Undo Stack: [Cmd5, Cmd4, Cmd3, Cmd2, Cmd1] ← Push/Pop
Redo Stack: [Cmd6, Cmd7, Cmd8]

Execute Cmd9 → Push to Undo, Clear Redo
Undo → Pop from Undo, Push to Redo
Redo → Pop from Redo, Push to Undo
```

### 2. Services (Business Logic)

#### **VoxelRenderer** (class)

**Rendering Pipeline:**
```
3D World Coords → Rotation → Projection → 2D Screen Coords
    (x,y,z)          (R)         (P)         (screenX, screenY)
```

**Projection Math:**
```csharp
// 1. Rotate around Y-axis (yaw)
x1 = x * cos(yaw) - z * sin(yaw)
z1 = x * sin(yaw) + z * cos(yaw)

// 2. Rotate around X-axis (pitch)
y1 = y * cos(pitch) - z1 * sin(pitch)
z2 = y * sin(pitch) + z1 * cos(pitch)

// 3. Isometric projection
screenX = x1 * zoom + centerX + panX
screenY = y1 * zoom + centerY + panY
```

**Z-Sorting:**
```csharp
// Simple depth sort (back to front)
voxels.OrderBy(v => v.X + v.Y + v.Z)
```
- Ensures far voxels render before near voxels
- Prevents visual artifacts with transparency

**Cube Rendering:**
```
     4─────────5
    /│        /│
   7─────────6 │
   │ │       │ │
   │ 0───────│─1
   │/        │/
   3─────────2

Faces rendered: Top (4567), Right (1562), Front (0123)
```

**Optimization Strategies:**
1. **Face Culling**: Only draw visible faces
2. **Batch Drawing**: Group voxels by color (future)
3. **Frustum Culling**: Skip off-screen voxels (future)

### 3. Presentation (UI Layer)

#### **VoxelEditorViewModel**

**Observable Properties (MVVM):**
```csharp
[ObservableProperty]
private string _currentColor;  // Generates: public string CurrentColor

// XAML binds to generated property:
// {x:Bind ViewModel.CurrentColor, Mode=OneWay}
```

**Relay Commands (MVVM):**
```csharp
[RelayCommand]
public void Undo()  // Generates: public ICommand UndoCommand

// XAML binds to generated command:
// Command="{x:Bind ViewModel.UndoCommand}"
```

**Event Handlers:**
```csharp
public void OnPointerMoved(object sender, PointerRoutedEventArgs e)
{
    // Mouse delta calculation
    deltaX = currentPos.X - lastPos.X
    deltaY = currentPos.Y - lastPos.Y
    
    // Apply to camera based on button state
    if (rightMouseDown)
        camera.Orbit(deltaX, deltaY)
}
```

**Render Loop:**
```
Timer (16.67ms)  →  Invalidate Canvas  →  OnPaintSurface
     ↓                      ↓                     ↓
  60 FPS              Request Redraw         Actual Draw
```

## Data Flow Diagrams

### Voxel Placement Flow (Phase 2)
```
User Click
    ↓
OnPointerPressed (ViewModel)
    ↓
Ray Casting (Convert 2D → 3D)
    ↓
Grid Intersection Test
    ↓
Create PlaceVoxelCommand
    ↓
Execute via UndoStack
    ↓
VoxelGrid.PlaceVoxel
    ↓
Invalidate Canvas
    ↓
OnPaintSurface
    ↓
VoxelRenderer.Render
    ↓
Screen Update
```

### Undo Flow
```
User presses Ctrl+Z
    ↓
UndoCommand triggered
    ↓
UndoStack.Undo
    ↓
Pop from undo stack
    ↓
command.Undo(grid)
    ↓
Push to redo stack
    ↓
Update CanUndo/CanRedo
    ↓
Invalidate Canvas
    ↓
Screen Update
```

## Design Patterns Used

### 1. **MVVM Pattern**
- **Model**: VoxelGrid, Voxel, Camera
- **View**: VoxelEditorPage.xaml
- **ViewModel**: VoxelEditorViewModel
- **Benefit**: Testable, maintainable, reactive UI

### 2. **Command Pattern**
- **Interface**: IVoxelCommand
- **Concrete**: PlaceVoxelCommand, RemoveVoxelCommand
- **Invoker**: UndoStack
- **Benefit**: Undo/redo, logging, macros

### 3. **Observer Pattern**
- **Observable**: ObservableObject (CommunityToolkit.Mvvm)
- **Properties**: [ObservableProperty] attributes
- **Benefit**: Automatic UI updates via INotifyPropertyChanged

### 4. **Service Locator** (via DI)
- **Container**: Uno Extensions Hosting
- **Services**: VoxelRenderer, FileService (future)
- **Benefit**: Loose coupling, testability

## Performance Considerations

### Memory Optimization
```
Full Array:    32 × 32 × 32 × 40 bytes = 1.3 MB per grid
Dictionary:    100 voxels × 40 bytes = 4 KB (typical)
Savings:       99.7% memory reduction!
```

### Rendering Optimization
```
Worst Case:    32,768 voxels × 3 faces × 4 vertices = 393,216 vertices
Typical Case:  100 voxels × 3 faces × 4 vertices = 1,200 vertices
Target:        60 FPS @ 1920×1080
```

**Future Optimizations:**
1. Spatial partitioning (Octree)
2. Level of Detail (LOD)
3. Instanced rendering
4. GPU acceleration (compute shaders)

## Threading Model

```
UI Thread              Render Thread         Background Thread
    │                        │                       │
    │──── User Input ────────┤                       │
    │                        │                       │
    │                        │                       │
    │──── Invalidate ────────┤                       │
    │                        │                       │
    │                        │──── Paint Surface ────┤
    │                        │                       │
    │                        │                       │
    │◄─── UI Update ─────────┤                       │
    │                        │                       │
    │                        │                       │
    │                        │         (Phase 5: MediaPipe)
    │                        │         Hand Tracking ────┤
    │◄────────────────────── Event ──────────────────────┤
```

## File Format Design (Phase 3)

```json
{
  "version": "1.0",
  "metadata": {
    "created": "2025-11-11T12:00:00Z",
    "modified": "2025-11-11T13:30:00Z",
    "author": "User"
  },
  "gridSize": 32,
  "voxels": [
    {"x": 0, "y": 0, "z": 0, "color": "#FF0000"},
    {"x": 1, "y": 0, "z": 0, "color": "#00FF00"}
  ],
  "palette": [
    "#FF0000", "#00FF00", "#0000FF",
    "#FFFF00", "#FF00FF", "#00FFFF"
  ],
  "camera": {
    "rotationX": 30,
    "rotationY": 45,
    "zoom": 50,
    "panX": 0,
    "panY": 0
  }
}
```

**Benefits:**
- Human-readable (debugging)
- Cross-platform compatible
- Version field for migration
- Minimal size (only active voxels)
- Camera state preserved

## Extension Points

### Future Features

1. **Layers System**
```csharp
class Layer
{
    string Name { get; set; }
    VoxelGrid Grid { get; set; }
    bool Visible { get; set; }
}
```

2. **Materials**
```csharp
struct Voxel
{
    // ... existing
    Material Material { get; set; }  // Metallic, Roughness, Emission
}
```

3. **Animation**
```csharp
class VoxelAnimation
{
    List<Keyframe> Keyframes { get; set; }
    float Duration { get; set; }
}
```

## Testing Strategy

### Unit Tests (Future)
```csharp
[Test]
public void PlaceVoxel_ValidPosition_ReturnsTrue()
{
    var grid = new VoxelGrid(32);
    var result = grid.PlaceVoxel(0, 0, 0, "#FF0000");
    Assert.IsTrue(result);
}

[Test]
public void UndoStack_ExceedsLimit_MaintainsSize()
{
    var stack = new UndoStack();
    for (int i = 0; i < 15; i++)
    {
        stack.ExecuteCommand(new PlaceVoxelCommand(i, 0, 0, "#FF0000"), grid);
    }
    // Verify stack has max 10 commands
}
```

### Integration Tests
- Camera control responsiveness
- Undo/redo with multiple operations
- File save/load round-trip
- Hand tracking to voxel mapping (Phase 7)

## Troubleshooting

### Common Issues

**Low FPS:**
- Too many voxels? Enable face culling
- Debug mode? Use Release build
- Integrated GPU? Check graphics drivers

**Camera Gimbal Lock:**
- Pitch clamped to ±89°
- Quaternions alternative (future)

**Memory Leaks:**
- Dispose SKPaint objects
- Clear event handlers on page unload
- Stop render timer when not visible

## Conclusion

The architecture is designed for:
- **Scalability**: Easy to add features
- **Maintainability**: Clean separation of concerns
- **Performance**: Optimized data structures
- **Testability**: Loose coupling via interfaces
- **Cross-Platform**: Uno Platform + SkiaSharp

This solid foundation supports all 7 phases of development!

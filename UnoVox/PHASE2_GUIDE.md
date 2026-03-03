# Phase 2 Implementation Guide - Mouse-Based Voxel Editing

## Overview

Phase 2 adds interactive voxel editing capabilities with mouse controls. This includes ray casting, voxel selection, placement, removal, and visual feedback.

## Goals

- [x] Ray casting from 2D mouse to 3D world
- [x] Voxel grid intersection detection
- [x] Place voxel on left click
- [x] Remove voxel on right click
- [x] Visual 3D cursor
- [x] Selected voxel highlighting
- [x] Undo/redo integration
- [x] Color palette integration

## Implementation Steps

### Step 1: Ray Casting System

Create `Services/RayCaster.cs`:

```csharp
public class RayCaster
{
    /// <summary>
    /// Converts 2D screen position to 3D ray in world space
    /// </summary>
    public (Vector3 origin, Vector3 direction) ScreenToWorldRay(
        float screenX, float screenY,
        CameraController camera,
        float screenWidth, float screenHeight)
    {
        // Inverse projection: screen → world
        // Account for camera rotation, pan, zoom
    }
    
    /// <summary>
    /// Finds intersection point with voxel grid
    /// Returns voxel coordinates if hit, null otherwise
    /// </summary>
    public (int x, int y, int z)? RayGridIntersection(
        Vector3 origin, Vector3 direction,
        VoxelGrid grid)
    {
        // DDA (Digital Differential Analyzer) algorithm
        // or ray-box intersection tests
    }
}
```

**Algorithm Options:**

**Option A: DDA Voxel Traversal**
```csharp
// Fast, exact voxel traversal
// Paper: "A Fast Voxel Traversal Algorithm" by Amanatides & Woo
```

**Option B: Ray-AABB Per Voxel**
```csharp
// Test each voxel's bounding box
// Good for sparse grids (our case)
```

### Step 2: Visual Cursor

Add to `VoxelRenderer.cs`:

```csharp
public void DrawCursor(SKCanvas canvas, 
    int x, int y, int z, 
    CameraController camera, 
    float width, float height)
{
    // Draw wireframe cube at cursor position
    // Use bright color (e.g., cyan/yellow)
    // Slightly larger than voxel (1.1x scale)
}
```

### Step 3: Selection Highlighting

```csharp
public void DrawSelectedVoxel(SKCanvas canvas,
    Voxel voxel,
    CameraController camera,
    float width, float height)
{
    // Draw overlay with different color/transparency
    // or animated outline effect
}
```

### Step 4: Update ViewModel

Modify `VoxelEditorViewModel.cs`:

```csharp
public partial class VoxelEditorViewModel : ObservableObject
{
    private readonly RayCaster _rayCaster;
    
    [ObservableProperty]
    private (int x, int y, int z)? _cursorVoxel;
    
    [ObservableProperty]
    private (int x, int y, int z)? _selectedVoxel;
    
    public void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as UIElement);
        
        // Cast ray
        var ray = _rayCaster.ScreenToWorldRay(
            (float)point.Position.X,
            (float)point.Position.Y,
            _camera, canvasWidth, canvasHeight);
        
        var hit = _rayCaster.RayGridIntersection(
            ray.origin, ray.direction, _voxelGrid);
        
        if (hit.HasValue)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                // Place voxel
                var cmd = new PlaceVoxelCommand(
                    hit.Value.x, hit.Value.y, hit.Value.z, 
                    CurrentColor);
                _undoStack.ExecuteCommand(cmd, _voxelGrid);
                UpdateUndoRedoState();
            }
            else if (point.Properties.IsRightButtonPressed)
            {
                // Remove voxel (only if exists)
                if (_voxelGrid.HasVoxel(hit.Value.x, hit.Value.y, hit.Value.z))
                {
                    var cmd = new RemoveVoxelCommand(
                        hit.Value.x, hit.Value.y, hit.Value.z);
                    _undoStack.ExecuteCommand(cmd, _voxelGrid);
                    UpdateUndoRedoState();
                }
            }
        }
    }
    
    public void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        // ... existing camera code ...
        
        // Update cursor position
        var point = e.GetCurrentPoint(sender as UIElement);
        var ray = _rayCaster.ScreenToWorldRay(...);
        var hit = _rayCaster.RayGridIntersection(ray.origin, ray.direction, _voxelGrid);
        
        CursorVoxel = hit;
        CursorPosition = hit.HasValue 
            ? $"{hit.Value.x}, {hit.Value.y}, {hit.Value.z}"
            : "Out of bounds";
    }
}
```

### Step 5: Update Renderer

Modify `VoxelRenderer.Render()`:

```csharp
public void Render(SKCanvas canvas, VoxelGrid grid, CameraController camera,
    float width, float height, 
    (int x, int y, int z)? cursorPos = null,
    (int x, int y, int z)? selectedPos = null)
{
    // ... existing rendering ...
    
    // Draw cursor
    if (cursorPos.HasValue)
    {
        DrawCursor(canvas, cursorPos.Value.x, cursorPos.Value.y, 
            cursorPos.Value.z, camera, width, height);
    }
    
    // Draw selection
    if (selectedPos.HasValue && grid.HasVoxel(...))
    {
        var voxel = grid.GetVoxel(...)!.Value;
        DrawSelectedVoxel(canvas, voxel, camera, width, height);
    }
}
```

### Step 6: Keyboard Shortcuts

Add to `VoxelEditorPage.xaml`:

```xaml
<Page.KeyboardAccelerators>
    <KeyboardAccelerator Key="Z" Modifiers="Control" 
                         Invoked="{x:Bind ViewModel.UndoCommand}" />
    <KeyboardAccelerator Key="Y" Modifiers="Control" 
                         Invoked="{x:Bind ViewModel.RedoCommand}" />
    <KeyboardAccelerator Key="Delete" 
                         Invoked="{x:Bind ViewModel.DeleteSelectedCommand}" />
</Page.KeyboardAccelerators>
```

## Ray Casting Math Details

### Inverse Projection

```csharp
// 1. Screen to Normalized Device Coordinates (NDC)
float ndcX = (screenX - screenWidth/2) / (screenWidth/2);
float ndcY = (screenY - screenHeight/2) / (screenHeight/2);

// 2. Apply inverse zoom
float worldX = ndcX / zoom;
float worldY = ndcY / zoom;

// 3. Apply inverse rotation (transpose of rotation matrix)
// Rotation is orthogonal, so inverse = transpose
Matrix4x4 invRotation = Matrix4x4.Transpose(rotationMatrix);
Vector3 rayDir = Vector3.Transform(new Vector3(ndcX, ndcY, -1), invRotation);

// 4. Ray origin is camera position
Vector3 rayOrigin = new Vector3(panX, panY, zoom);
```

### Grid Intersection

**DDA Algorithm:**
```csharp
public (int x, int y, int z)? RayGridIntersection(
    Vector3 origin, Vector3 direction, VoxelGrid grid)
{
    // Starting voxel
    int x = (int)Math.Floor(origin.X + grid.Size/2);
    int y = (int)Math.Floor(origin.Y + grid.Size/2);
    int z = (int)Math.Floor(origin.Z + grid.Size/2);
    
    // Step direction
    int stepX = direction.X > 0 ? 1 : -1;
    int stepY = direction.Y > 0 ? 1 : -1;
    int stepZ = direction.Z > 0 ? 1 : -1;
    
    // t values for next voxel boundary
    float tMaxX = ((x + (stepX > 0 ? 1 : 0)) - origin.X) / direction.X;
    float tMaxY = ((y + (stepY > 0 ? 1 : 0)) - origin.Y) / direction.Y;
    float tMaxZ = ((z + (stepZ > 0 ? 1 : 0)) - origin.Z) / direction.Z;
    
    // t delta per voxel
    float tDeltaX = Math.Abs(1f / direction.X);
    float tDeltaY = Math.Abs(1f / direction.Y);
    float tDeltaZ = Math.Abs(1f / direction.Z);
    
    // Traverse voxels
    int maxSteps = grid.Size * 3; // Prevent infinite loop
    for (int i = 0; i < maxSteps; i++)
    {
        // Check if current voxel is valid and occupied
        if (grid.IsValidPosition(x, y, z))
        {
            if (grid.HasVoxel(x, y, z))
                return (x, y, z);
        }
        else
        {
            break; // Out of bounds
        }
        
        // Step to next voxel
        if (tMaxX < tMaxY && tMaxX < tMaxZ)
        {
            x += stepX;
            tMaxX += tDeltaX;
        }
        else if (tMaxY < tMaxZ)
        {
            y += stepY;
            tMaxY += tDeltaY;
        }
        else
        {
            z += stepZ;
            tMaxZ += tDeltaZ;
        }
    }
    
    return null; // No intersection
}
```

## Visual Feedback Examples

### Cursor Styles

**Option 1: Wireframe Cube**
```csharp
// Draw edges of cube
for each edge in cubeEdges:
    canvas.DrawLine(edge.start, edge.end, cursorPaint)
```

**Option 2: Semi-transparent Overlay**
```csharp
// Draw filled cube with transparency
cursorPaint.Color = SKColors.Cyan.WithAlpha(100)
DrawVoxel(canvas, cursorX, cursorY, cursorZ, cursorPaint)
```

**Option 3: Animated Outline**
```csharp
// Pulsing effect
float pulse = Math.Sin(time * 2) * 0.5 + 0.5
cursorPaint.StrokeWidth = 2 + pulse * 2
```

### Selection Highlight

```csharp
// Overlay with different color
var overlayPaint = new SKPaint
{
    Color = SKColors.Yellow.WithAlpha(80),
    Style = SKPaintStyle.Fill
};
// Draw voxel with overlay paint
```

## Testing Phase 2

### Manual Tests

1. **Cursor Tracking**
   - Move mouse over grid
   - Cursor should follow mouse position
   - Should snap to voxel grid

2. **Voxel Placement**
   - Left click empty space
   - Voxel appears with selected color
   - Undo button becomes enabled

3. **Voxel Removal**
   - Right click existing voxel
   - Voxel disappears
   - Can undo removal

4. **Undo/Redo**
   - Place 5 voxels
   - Press Ctrl+Z five times
   - Press Ctrl+Y five times
   - All voxels should return

5. **Color Selection**
   - Select color from palette
   - Place voxel
   - Verify voxel has correct color

### Edge Cases

- Click outside grid bounds
- Place voxel where one exists
- Remove voxel from empty space
- Undo when nothing to undo
- Redo when nothing to redo
- Fill entire grid (32³ voxels)

## Performance Targets

- **Ray casting**: < 1ms per ray
- **Rendering 1000 voxels**: 60 FPS
- **Undo/redo**: < 10ms
- **Cursor update**: < 5ms

## Common Issues

**Ray doesn't hit voxels:**
- Check coordinate system (grid centered at origin)
- Verify camera transformations are inverted correctly
- Debug: Draw ray line in 3D

**Cursor jumps around:**
- Smooth ray direction calculation
- Check for division by zero in ray
- Clamp to grid bounds

**Undo doesn't work:**
- Verify command stores previous state
- Check stack size limit
- Ensure ExecuteCommand is called

## Next Steps After Phase 2

Once Phase 2 is complete, you'll have a fully functional voxel editor! Then you can move to:

- **Phase 3**: File save/load with JSON
- **Phase 4**: Webcam integration
- **Phase 5-7**: Hand tracking and gesture controls

## Resources

- **Ray Casting**: [scratchapixel.com/ray-tracing](https://www.scratchapixel.com)
- **DDA Algorithm**: "A Fast Voxel Traversal Algorithm" (Amanatides & Woo, 1987)
- **Uno Platform Docs**: [platform.uno](https://platform.uno)

Good luck with Phase 2! 🚀

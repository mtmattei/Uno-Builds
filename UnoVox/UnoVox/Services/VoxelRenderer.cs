using SkiaSharp;

namespace UnoVox.Services;

/// <summary>
/// Renders voxel grid using SkiaSharp with isometric projection
/// </summary>
public class VoxelRenderer
{
    private float _voxelSize = 1.0f;
    public float VoxelSize
    {
        get => _voxelSize;
        set => _voxelSize = Math.Clamp(value, 0.5f, 3.0f);
    }
    
    private SKPaint _voxelPaint;
    private SKPaint _gridPaint;
    private SKPaint _axisPaint;
    private SKPaint _edgePaint;
    private SKPaint _cursorPaint;
    private SKPaint _highlightPaint;

    public VoxelRenderer()
    {
        _voxelPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        _gridPaint = new SKPaint
        {
            Color = SKColors.Gray.WithAlpha(100),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        _axisPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        _edgePaint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(100),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        _cursorPaint = new SKPaint
        {
            Color = SKColors.Cyan,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        _highlightPaint = new SKPaint
        {
            Color = SKColors.Yellow.WithAlpha(80),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
    }

    /// <summary>
    /// Projects 3D world coordinates to 2D screen coordinates using isometric projection
    /// Uses cached trigonometry from camera for performance
    /// </summary>
    private SKPoint ProjectToScreen(float x, float y, float z, 
        Models.CameraController camera,
        float screenWidth, float screenHeight)
    {
        // Apply rotation around Y axis (yaw) using cached values
        float x1 = x * camera.CosY - z * camera.SinY;
        float z1 = x * camera.SinY + z * camera.CosY;
        
        // Apply rotation around X axis (pitch) using cached values
        float y1 = y * camera.CosX - z1 * camera.SinX;
        float z2 = y * camera.SinX + z1 * camera.CosX;

        // Isometric projection
        float screenX = x1 * camera.Zoom + screenWidth / 2 + camera.PanX;
        float screenY = y1 * camera.Zoom + screenHeight / 2 + camera.PanY;

        return new SKPoint(screenX, screenY);
    }

    /// <summary>
    /// Renders the voxel grid
    /// </summary>
    public void Render(SKCanvas canvas, Models.VoxelGrid grid, Models.CameraController camera, 
        float width, float height, (int x, int y, int z)? cursorPos = null, 
        (int x, int y, int z)? selectedPos = null)
    {
        // Don't clear canvas here - we want to preserve the camera feed drawn earlier
        // canvas.Clear(SKColors.Black);

        // REMOVED: Grid floor visual - hide grid lines
        // DrawGrid(canvas, camera, width, height, grid.Size);

        // REMOVED: Coordinate axes - hide visual guides
        // DrawAxes(canvas, camera, width, height);

        // REMOVED: 3D grid box - too confusing
        // Draw3DGridBox(canvas, camera, width, height, grid.Size);

        // Voxels are pre-sorted by depth in the grid
        // Render voxels
        foreach (var voxel in grid.ActiveVoxels)
        {
            DrawVoxel(canvas, voxel, camera, width, height);
        }

        // Draw cursor (wireframe cube at placement position)
        if (cursorPos.HasValue)
        {
            DrawCursor(canvas, cursorPos.Value.x, cursorPos.Value.y, cursorPos.Value.z, 
                camera, width, height);
        }

        // Draw selection highlight
        if (selectedPos.HasValue && grid.HasVoxel(selectedPos.Value.x, selectedPos.Value.y, selectedPos.Value.z))
        {
            var voxel = grid.GetVoxel(selectedPos.Value.x, selectedPos.Value.y, selectedPos.Value.z);
            if (voxel.HasValue)
            {
                DrawSelectedVoxel(canvas, voxel.Value, camera, width, height);
            }
        }
    }

    /// <summary>
    /// Draws a single voxel as a cube
    /// </summary>
    private void DrawVoxel(SKCanvas canvas, Models.Voxel voxel, Models.CameraController camera,
        float width, float height)
    {
        if (!voxel.IsActive) return;

        // Parse hex color
        var color = SKColor.Parse(voxel.Color);
        _voxelPaint.Color = color;

        float x = voxel.X - 16; // Center grid
        float y = voxel.Y - 16;
        float z = voxel.Z - 16;

        // Calculate cube vertices
        var vertices = new[]
        {
            ProjectToScreen(x, y, z, camera, width, height),
            ProjectToScreen(x + VoxelSize, y, z, camera, width, height),
            ProjectToScreen(x + VoxelSize, y + VoxelSize, z, camera, width, height),
            ProjectToScreen(x, y + VoxelSize, z, camera, width, height),
            ProjectToScreen(x, y, z + VoxelSize, camera, width, height),
            ProjectToScreen(x + VoxelSize, y, z + VoxelSize, camera, width, height),
            ProjectToScreen(x + VoxelSize, y + VoxelSize, z + VoxelSize, camera, width, height),
            ProjectToScreen(x, y + VoxelSize, z + VoxelSize, camera, width, height),
        };

        // Draw visible faces (simple approach - draw top, right, and front)
        using var path = new SKPath();

        // Top face
        path.MoveTo(vertices[4]);
        path.LineTo(vertices[5]);
        path.LineTo(vertices[6]);
        path.LineTo(vertices[7]);
        path.Close();
        _voxelPaint.Color = color;
        canvas.DrawPath(path, _voxelPaint);

        // Right face
        path.Reset();
        path.MoveTo(vertices[1]);
        path.LineTo(vertices[5]);
        path.LineTo(vertices[6]);
        path.LineTo(vertices[2]);
        path.Close();
        _voxelPaint.Color = color.WithAlpha(200);
        canvas.DrawPath(path, _voxelPaint);

        // Front face
        path.Reset();
        path.MoveTo(vertices[0]);
        path.LineTo(vertices[1]);
        path.LineTo(vertices[2]);
        path.LineTo(vertices[3]);
        path.Close();
        _voxelPaint.Color = color.WithAlpha(230);
        canvas.DrawPath(path, _voxelPaint);

        // Draw edges using cached paint
        canvas.DrawPath(path, _edgePaint);
    }

    /// <summary>
    /// Draws the grid floor
    /// </summary>
    private void DrawGrid(SKCanvas canvas, Models.CameraController camera, 
        float width, float height, int gridSize)
    {
        int halfSize = gridSize / 2;

        for (int x = -halfSize; x <= halfSize; x += 4)
        {
            var start = ProjectToScreen(x, -halfSize, -halfSize, camera, width, height);
            var end = ProjectToScreen(x, -halfSize, halfSize, camera, width, height);
            canvas.DrawLine(start, end, _gridPaint);
        }

        for (int z = -halfSize; z <= halfSize; z += 4)
        {
            var start = ProjectToScreen(-halfSize, -halfSize, z, camera, width, height);
            var end = ProjectToScreen(halfSize, -halfSize, z, camera, width, height);
            canvas.DrawLine(start, end, _gridPaint);
        }
    }

    /// <summary>
    /// Draws coordinate axes
    /// </summary>
    private void DrawAxes(SKCanvas canvas, Models.CameraController camera, float width, float height)
    {
        var origin = ProjectToScreen(0, 0, 0, camera, width, height);

        // X axis (red)
        _axisPaint.Color = SKColors.Red;
        var xEnd = ProjectToScreen(5, 0, 0, camera, width, height);
        canvas.DrawLine(origin, xEnd, _axisPaint);

        // Y axis (green)
        _axisPaint.Color = SKColors.Green;
        var yEnd = ProjectToScreen(0, 5, 0, camera, width, height);
        canvas.DrawLine(origin, yEnd, _axisPaint);

        // Z axis (blue)
        _axisPaint.Color = SKColors.Blue;
        var zEnd = ProjectToScreen(0, 0, 5, camera, width, height);
        canvas.DrawLine(origin, zEnd, _axisPaint);
    }

    /// <summary>
    /// Draws a 3D wireframe bounding box with grid lines showing the voxel space.
    /// This helps users understand the 3D volume where voxels can be placed.
    /// </summary>
    private void Draw3DGridBox(SKCanvas canvas, Models.CameraController camera,
        float width, float height, int gridSize)
    {
        int halfSize = gridSize / 2;

        // Create paint for semi-transparent grid
        using var gridPaint = new SKPaint
        {
            Color = SKColors.Cyan.WithAlpha(60),  // Subtle but visible
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        // Paint for outer box edges (more prominent)
        using var boxPaint = new SKPaint
        {
            Color = SKColors.Cyan.WithAlpha(150),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        // Draw the 12 edges of the bounding box
        var corners = new (float x, float y, float z)[]
        {
            (-halfSize, -halfSize, -halfSize), // 0: back-bottom-left
            (halfSize, -halfSize, -halfSize),  // 1: back-bottom-right
            (halfSize, -halfSize, halfSize),   // 2: front-bottom-right
            (-halfSize, -halfSize, halfSize),  // 3: front-bottom-left
            (-halfSize, halfSize, -halfSize),  // 4: back-top-left
            (halfSize, halfSize, -halfSize),   // 5: back-top-right
            (halfSize, halfSize, halfSize),    // 6: front-top-right
            (-halfSize, halfSize, halfSize),   // 7: front-top-left
        };

        // Bottom face edges
        DrawEdge(canvas, camera, width, height, corners[0], corners[1], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[1], corners[2], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[2], corners[3], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[3], corners[0], boxPaint);

        // Top face edges
        DrawEdge(canvas, camera, width, height, corners[4], corners[5], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[5], corners[6], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[6], corners[7], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[7], corners[4], boxPaint);

        // Vertical edges connecting top and bottom
        DrawEdge(canvas, camera, width, height, corners[0], corners[4], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[1], corners[5], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[2], corners[6], boxPaint);
        DrawEdge(canvas, camera, width, height, corners[3], corners[7], boxPaint);

        // Draw grid lines inside the box (every 4 units)
        int step = 4;

        // XZ planes (horizontal slices at different Y heights)
        for (int y = -halfSize + step; y < halfSize; y += step)
        {
            // Lines along X axis
            for (int z = -halfSize; z <= halfSize; z += step)
            {
                var start = ProjectToScreen(-halfSize, y, z, camera, width, height);
                var end = ProjectToScreen(halfSize, y, z, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
            // Lines along Z axis
            for (int x = -halfSize; x <= halfSize; x += step)
            {
                var start = ProjectToScreen(x, y, -halfSize, camera, width, height);
                var end = ProjectToScreen(x, y, halfSize, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
        }

        // XY planes (vertical slices at different Z depths)
        for (int z = -halfSize + step; z < halfSize; z += step)
        {
            // Lines along X axis
            for (int y = -halfSize; y <= halfSize; y += step)
            {
                var start = ProjectToScreen(-halfSize, y, z, camera, width, height);
                var end = ProjectToScreen(halfSize, y, z, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
            // Lines along Y axis
            for (int x = -halfSize; x <= halfSize; x += step)
            {
                var start = ProjectToScreen(x, -halfSize, z, camera, width, height);
                var end = ProjectToScreen(x, halfSize, z, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
        }

        // YZ planes (vertical slices at different X positions)
        for (int x = -halfSize + step; x < halfSize; x += step)
        {
            // Lines along Y axis
            for (int z = -halfSize; z <= halfSize; z += step)
            {
                var start = ProjectToScreen(x, -halfSize, z, camera, width, height);
                var end = ProjectToScreen(x, halfSize, z, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
            // Lines along Z axis
            for (int y = -halfSize; y <= halfSize; y += step)
            {
                var start = ProjectToScreen(x, y, -halfSize, camera, width, height);
                var end = ProjectToScreen(x, y, halfSize, camera, width, height);
                canvas.DrawLine(start, end, gridPaint);
            }
        }
    }

    /// <summary>
    /// Helper method to draw an edge between two 3D points
    /// </summary>
    private void DrawEdge(SKCanvas canvas, Models.CameraController camera,
        float width, float height, (float x, float y, float z) start,
        (float x, float y, float z) end, SKPaint paint)
    {
        var screenStart = ProjectToScreen(start.x, start.y, start.z, camera, width, height);
        var screenEnd = ProjectToScreen(end.x, end.y, end.z, camera, width, height);
        canvas.DrawLine(screenStart, screenEnd, paint);
    }

    /// <summary>
    /// Draws a wireframe cursor showing where a voxel will be placed
    /// </summary>
    private void DrawCursor(SKCanvas canvas, int x, int y, int z, 
        Models.CameraController camera, float width, float height)
    {
        float wx = x - 16; // Center grid
        float wy = y - 16;
        float wz = z - 16;

        // Slightly larger than voxel for visibility
        float scale = 1.05f;
        float offset = (scale - 1f) * VoxelSize / 2f;

        // Calculate 8 corners of cursor cube
        var corners = new SKPoint[8];
        corners[0] = ProjectToScreen(wx - offset, wy - offset, wz - offset, camera, width, height);
        corners[1] = ProjectToScreen(wx + VoxelSize + offset, wy - offset, wz - offset, camera, width, height);
        corners[2] = ProjectToScreen(wx + VoxelSize + offset, wy + VoxelSize + offset, wz - offset, camera, width, height);
        corners[3] = ProjectToScreen(wx - offset, wy + VoxelSize + offset, wz - offset, camera, width, height);
        corners[4] = ProjectToScreen(wx - offset, wy - offset, wz + VoxelSize + offset, camera, width, height);
        corners[5] = ProjectToScreen(wx + VoxelSize + offset, wy - offset, wz + VoxelSize + offset, camera, width, height);
        corners[6] = ProjectToScreen(wx + VoxelSize + offset, wy + VoxelSize + offset, wz + VoxelSize + offset, camera, width, height);
        corners[7] = ProjectToScreen(wx - offset, wy + VoxelSize + offset, wz + VoxelSize + offset, camera, width, height);

        // Draw 12 edges using cached paint
        canvas.DrawLine(corners[0], corners[1], _cursorPaint); // Bottom front
        canvas.DrawLine(corners[1], corners[2], _cursorPaint); // Bottom right
        canvas.DrawLine(corners[2], corners[3], _cursorPaint); // Bottom back
        canvas.DrawLine(corners[3], corners[0], _cursorPaint); // Bottom left
        
        canvas.DrawLine(corners[4], corners[5], _cursorPaint); // Top front
        canvas.DrawLine(corners[5], corners[6], _cursorPaint); // Top right
        canvas.DrawLine(corners[6], corners[7], _cursorPaint); // Top back
        canvas.DrawLine(corners[7], corners[4], _cursorPaint); // Top left
        
        canvas.DrawLine(corners[0], corners[4], _cursorPaint); // Vertical front-left
        canvas.DrawLine(corners[1], corners[5], _cursorPaint); // Vertical front-right
        canvas.DrawLine(corners[2], corners[6], _cursorPaint); // Vertical back-right
        canvas.DrawLine(corners[3], corners[7], _cursorPaint); // Vertical back-left
    }

    /// <summary>
    /// Draws a highlight overlay on a selected voxel
    /// </summary>
    private void DrawSelectedVoxel(SKCanvas canvas, Models.Voxel voxel, 
        Models.CameraController camera, float width, float height)
    {
        float x = voxel.X - 16;
        float y = voxel.Y - 16;
        float z = voxel.Z - 16;

        // Draw highlight overlay (same as voxel but with yellow tint)
        var vertices = new[]
        {
            ProjectToScreen(x, y, z, camera, width, height),
            ProjectToScreen(x + VoxelSize, y, z, camera, width, height),
            ProjectToScreen(x + VoxelSize, y + VoxelSize, z, camera, width, height),
            ProjectToScreen(x, y + VoxelSize, z, camera, width, height),
            ProjectToScreen(x, y, z + VoxelSize, camera, width, height),
            ProjectToScreen(x + VoxelSize, y, z + VoxelSize, camera, width, height),
            ProjectToScreen(x + VoxelSize, y + VoxelSize, z + VoxelSize, camera, width, height),
            ProjectToScreen(x, y + VoxelSize, z + VoxelSize, camera, width, height),
        };

        using var path = new SKPath();
        
        // Top face
        path.MoveTo(vertices[4]);
        path.LineTo(vertices[5]);
        path.LineTo(vertices[6]);
        path.LineTo(vertices[7]);
        path.Close();
        canvas.DrawPath(path, _highlightPaint);
        
        // Right face
        path.Reset();
        path.MoveTo(vertices[1]);
        path.LineTo(vertices[5]);
        path.LineTo(vertices[6]);
        path.LineTo(vertices[2]);
        path.Close();
        canvas.DrawPath(path, _highlightPaint);
        
        // Front face
        path.Reset();
        path.MoveTo(vertices[0]);
        path.LineTo(vertices[1]);
        path.LineTo(vertices[2]);
        path.LineTo(vertices[3]);
        path.Close();
        canvas.DrawPath(path, _highlightPaint);
    }
}


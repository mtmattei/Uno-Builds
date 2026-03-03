namespace UnoVox.Models;

/// <summary>
/// Manages camera position and orientation for voxel grid rendering
/// </summary>
public class CameraController
{
    // Camera position
    public float CameraX { get; private set; }
    public float CameraY { get; private set; }
    public float CameraZ { get; private set; }

    // Camera rotation (in degrees)
    private float _rotationX = 30f; // Pitch
    private float _rotationY = 45f; // Yaw

    public float RotationX 
    { 
        get => _rotationX;
        set 
        {
            _rotationX = value;
            UpdateRotationCache();
        }
    }
    
    public float RotationY 
    { 
        get => _rotationY;
        set 
        {
            _rotationY = value;
            UpdateRotationCache();
        }
    }

    // Cached trigonometry values for performance
    public float CosX { get; private set; }
    public float SinX { get; private set; }
    public float CosY { get; private set; }
    public float SinY { get; private set; }
    
    // Camera zoom
    public float Zoom { get; set; } = 50f;
    
    // Pan offset
    public float PanX { get; set; }
    public float PanY { get; set; }

    // Constraints
    public float MinZoom { get; set; } = 10f;
    public float MaxZoom { get; set; } = 200f;

    public CameraController()
    {
        // Default camera position
        CameraX = 0;
        CameraY = 0;
        CameraZ = 0;
        // Center grid vertically in viewport by adjusting default pan
        PanY = -200f; // Move grid down to center it better
        UpdateRotationCache();
    }

    private void UpdateRotationCache()
    {
        CosX = (float)Math.Cos(_rotationX * Math.PI / 180.0);
        SinX = (float)Math.Sin(_rotationX * Math.PI / 180.0);
        CosY = (float)Math.Cos(_rotationY * Math.PI / 180.0);
        SinY = (float)Math.Sin(_rotationY * Math.PI / 180.0);
    }

    /// <summary>
    /// Orbit camera around the center point
    /// </summary>
    public void Orbit(float deltaX, float deltaY)
    {
        RotationY += deltaX * 0.5f;
        RotationX += deltaY * 0.5f;

        // Clamp pitch to prevent gimbal lock
        RotationX = Math.Clamp(RotationX, -89f, 89f);

        // Normalize yaw
        while (RotationY > 360f) RotationY -= 360f;
        while (RotationY < 0f) RotationY += 360f;
    }

    /// <summary>
    /// Pan camera in screen space
    /// </summary>
    public void Pan(float deltaX, float deltaY)
    {
        PanX += deltaX * 0.1f;
        PanY -= deltaY * 0.1f;
    }

    /// <summary>
    /// Zoom camera in/out
    /// </summary>
    public void ZoomCamera(float delta)
    {
        Zoom += delta * 0.1f;
        Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Reset camera to default position
    /// </summary>
    public void Reset()
    {
        CameraX = 0;
        CameraY = 0;
        CameraZ = 0;
        RotationX = 30f;
        RotationY = 45f;
        Zoom = 50f;
        PanX = 0;
        PanY = -200f; // Center grid vertically
        UpdateRotationCache();
    }
}

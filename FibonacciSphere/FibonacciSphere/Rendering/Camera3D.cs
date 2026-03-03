using System;
using System.Numerics;

namespace FibonacciSphere.Rendering;

/// <summary>
/// Simple 3D camera for projecting world coordinates to screen space.
/// </summary>
public class Camera3D
{
    private Vector3 _position;
    private Vector3 _target;
    private Vector3 _up;
    private float _fieldOfView;
    private float _aspectRatio;
    private float _nearPlane;
    private float _farPlane;

    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewProjectionMatrix;
    private bool _isDirty = true;

    /// <summary>
    /// Camera position in world space.
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Point the camera is looking at.
    /// </summary>
    public Vector3 Target
    {
        get => _target;
        set
        {
            _target = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Up direction for the camera.
    /// </summary>
    public Vector3 Up
    {
        get => _up;
        set
        {
            _up = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Field of view in radians.
    /// </summary>
    public float FieldOfView
    {
        get => _fieldOfView;
        set
        {
            _fieldOfView = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Aspect ratio (width / height).
    /// </summary>
    public float AspectRatio
    {
        get => _aspectRatio;
        set
        {
            _aspectRatio = value;
            _isDirty = true;
        }
    }

    public Camera3D()
    {
        _position = new Vector3(0, 0, 3.5f);
        _target = Vector3.Zero;
        _up = Vector3.UnitY;
        _fieldOfView = MathF.PI / 4; // 45 degrees
        _aspectRatio = 1f;
        _nearPlane = 0.1f;
        _farPlane = 100f;
    }

    /// <summary>
    /// Sets the camera distance from the target along the Z axis.
    /// </summary>
    public void SetDistance(float distance)
    {
        _position = new Vector3(0, 0, distance);
        _isDirty = true;
    }

    /// <summary>
    /// Updates the camera aspect ratio based on screen dimensions.
    /// </summary>
    public void SetScreenSize(float width, float height)
    {
        _aspectRatio = width / height;
        _isDirty = true;
    }

    /// <summary>
    /// Rotates the camera around the target point.
    /// </summary>
    /// <param name="deltaYaw">Horizontal rotation in radians</param>
    /// <param name="deltaPitch">Vertical rotation in radians</param>
    public void RotateAround(float deltaYaw, float deltaPitch)
    {
        var offset = _position - _target;

        // Rotate around Y axis (yaw)
        var yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, deltaYaw);
        offset = Vector3.Transform(offset, yawRotation);

        // Calculate right vector for pitch rotation
        var forward = Vector3.Normalize(_target - _position);
        var right = Vector3.Normalize(Vector3.Cross(forward, _up));

        // Rotate around right axis (pitch) with limits
        var pitchRotation = Quaternion.CreateFromAxisAngle(right, deltaPitch);
        var newOffset = Vector3.Transform(offset, pitchRotation);

        // Prevent flipping by limiting pitch
        var newForward = Vector3.Normalize(_target - (_target + newOffset));
        var dot = Vector3.Dot(newForward, Vector3.UnitY);
        if (MathF.Abs(dot) < 0.95f)
        {
            offset = newOffset;
        }

        _position = _target + offset;
        _isDirty = true;
    }

    /// <summary>
    /// Zooms the camera by adjusting distance to target.
    /// </summary>
    public void Zoom(float delta)
    {
        var direction = Vector3.Normalize(_position - _target);
        var distance = Vector3.Distance(_position, _target);
        distance = MathF.Max(1.5f, MathF.Min(10f, distance + delta));
        _position = _target + direction * distance;
        _isDirty = true;
    }

    /// <summary>
    /// Updates matrices if camera state has changed.
    /// </summary>
    private void UpdateMatrices()
    {
        if (!_isDirty)
        {
            return;
        }

        _viewMatrix = Matrix4x4.CreateLookAt(_position, _target, _up);
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            _fieldOfView,
            _aspectRatio,
            _nearPlane,
            _farPlane);
        _viewProjectionMatrix = _viewMatrix * _projectionMatrix;
        _isDirty = false;
    }

    /// <summary>
    /// Projects a 3D world position to 2D screen coordinates.
    /// </summary>
    /// <param name="worldPosition">Position in world space</param>
    /// <param name="screenSize">Screen dimensions (width, height)</param>
    /// <returns>Screen coordinates and depth (Z value for sorting)</returns>
    public (Vector2 screenPos, float depth) ProjectToScreen(Vector3 worldPosition, Vector2 screenSize)
    {
        UpdateMatrices();

        // Transform to clip space
        var clipPos = Vector4.Transform(new Vector4(worldPosition, 1f), _viewProjectionMatrix);

        // Perspective divide (to normalized device coordinates)
        if (clipPos.W == 0)
        {
            return (Vector2.Zero, 0);
        }

        var ndc = new Vector3(clipPos.X / clipPos.W, clipPos.Y / clipPos.W, clipPos.Z / clipPos.W);

        // Map to screen coordinates (Y is flipped in screen space)
        var screenX = (ndc.X + 1f) * 0.5f * screenSize.X;
        var screenY = (1f - ndc.Y) * 0.5f * screenSize.Y;

        // Depth for z-ordering (smaller = closer to camera)
        var depth = ndc.Z;

        return (new Vector2(screenX, screenY), depth);
    }

    /// <summary>
    /// Transforms a world position to view space (camera relative).
    /// </summary>
    public Vector3 WorldToViewSpace(Vector3 worldPosition)
    {
        UpdateMatrices();
        return Vector3.Transform(worldPosition, _viewMatrix);
    }

    /// <summary>
    /// Gets the distance from camera to a world position.
    /// </summary>
    public float GetDistanceToCamera(Vector3 worldPosition)
    {
        return Vector3.Distance(_position, worldPosition);
    }
}

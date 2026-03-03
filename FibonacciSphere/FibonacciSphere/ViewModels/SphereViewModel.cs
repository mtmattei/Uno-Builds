using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FibonacciSphere.Math;
using FibonacciSphere.Models;
using FibonacciSphere.Rendering;

namespace FibonacciSphere.ViewModels;

/// <summary>
/// ViewModel for the Fibonacci sphere visualization with all bindable properties and commands.
/// </summary>
public partial class SphereViewModel : ObservableObject
{
    private readonly SphereRenderer _renderer;
    private SphereSettings _currentSettings;

    public SphereRenderer Renderer => _renderer;

    public SphereViewModel()
    {
        _renderer = new SphereRenderer();
        _currentSettings = new SphereSettings();
    }

    // Shape selection
    [ObservableProperty]
    private int _selectedShapeIndex;

    public IReadOnlyList<string> ShapeOptions { get; } = new[] { "Sphere", "Uno Logo" };

    // Point generation
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PointCountDisplay))]
    private int _pointCount = 200;

    public string PointCountDisplay => PointCount.ToString();

    // Rotation
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RotationSpeedDisplay))]
    private double _rotationSpeed = 0.5;

    public string RotationSpeedDisplay => _rotationSpeed.ToString("F2");

    [ObservableProperty]
    private bool _isRotating = true;

    [ObservableProperty]
    private bool _rotateClockwise = true;

    [ObservableProperty]
    private int _selectedEasingIndex;

    public IReadOnlyList<string> EasingOptions { get; } = new[] { "Linear", "Ease In/Out", "Elastic" };

    // Wobble effect
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WobbleAmplitudeDisplay))]
    private double _wobbleAmplitude = 0.1;

    public string WobbleAmplitudeDisplay => _wobbleAmplitude.ToString("F2");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WobbleFrequencyDisplay))]
    private double _wobbleFrequency = 2.0;

    public string WobbleFrequencyDisplay => _wobbleFrequency.ToString("F1");

    [ObservableProperty]
    private int _selectedWobbleAxisIndex;

    public IReadOnlyList<string> WobbleAxisOptions { get; } = new[] { "Radial", "Tangential", "Random" };

    // Point size
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BasePointSizeDisplay))]
    private double _basePointSize = 8.0;

    public string BasePointSizeDisplay => _basePointSize.ToString("F0");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SizeVariationDisplay))]
    private double _sizeVariation = 0.0;

    public string SizeVariationDisplay => _sizeVariation.ToString("F1");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PulseSpeedDisplay))]
    private double _pulseSpeed = 0.0;

    public string PulseSpeedDisplay => _pulseSpeed.ToString("F1");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PulseAmountDisplay))]
    private double _pulseAmount = 0.0;

    public string PulseAmountDisplay => _pulseAmount.ToString("F2");

    [ObservableProperty]
    private bool _depthScaling = true;

    // Trails
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrailLengthDisplay))]
    private int _trailLength = 20;

    public string TrailLengthDisplay => _trailLength.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrailOpacityDisplay))]
    private double _trailOpacity = 0.5;

    public string TrailOpacityDisplay => (_trailOpacity * 100).ToString("F0") + "%";

    [ObservableProperty]
    private int _selectedTrailStyleIndex;

    public IReadOnlyList<string> TrailStyleOptions { get; } = new[] { "Line", "Dots", "Ribbon" };

    // Camera
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CameraDistanceDisplay))]
    private double _cameraDistance = 3.5;

    public string CameraDistanceDisplay => _cameraDistance.ToString("F1");

    // Colors
    [ObservableProperty]
    private bool _useGradientColors = true;

    // Selection info
    [ObservableProperty]
    private string _selectedPointInfo = string.Empty;

    [ObservableProperty]
    private int _selectedPointCount;

    /// <summary>
    /// Builds the current settings from property values.
    /// </summary>
    public SphereSettings BuildSettings()
    {
        return new SphereSettings
        {
            Shape = (ShapeType)SelectedShapeIndex,
            PointCount = PointCount,
            RotationSpeed = (float)RotationSpeed,
            IsRotating = IsRotating,
            RotateClockwise = RotateClockwise,
            EasingType = (Easing.EasingType)SelectedEasingIndex,
            WobbleAmplitude = (float)WobbleAmplitude,
            WobbleFrequency = (float)WobbleFrequency,
            WobbleAxis = (WobbleAxis)SelectedWobbleAxisIndex,
            BasePointSize = (float)BasePointSize,
            SizeVariation = (float)SizeVariation,
            PulseSpeed = (float)PulseSpeed,
            PulseAmount = (float)PulseAmount,
            DepthScaling = DepthScaling,
            TrailLength = TrailLength,
            TrailOpacity = (float)TrailOpacity,
            TrailStyle = (TrailStyle)SelectedTrailStyleIndex,
            CameraDistance = (float)CameraDistance,
            UseGradientColors = UseGradientColors
        };
    }

    /// <summary>
    /// Applies current settings to the renderer.
    /// </summary>
    public void ApplySettings()
    {
        _currentSettings = BuildSettings();
        _renderer.UpdateSettings(_currentSettings);
    }

    partial void OnSelectedShapeIndexChanged(int value) => ApplySettings();
    partial void OnPointCountChanged(int value) => ApplySettings();
    partial void OnRotationSpeedChanged(double value) => ApplySettings();
    partial void OnIsRotatingChanged(bool value) => ApplySettings();
    partial void OnRotateClockwiseChanged(bool value) => ApplySettings();
    partial void OnSelectedEasingIndexChanged(int value) => ApplySettings();
    partial void OnWobbleAmplitudeChanged(double value) => ApplySettings();
    partial void OnWobbleFrequencyChanged(double value) => ApplySettings();
    partial void OnSelectedWobbleAxisIndexChanged(int value) => ApplySettings();
    partial void OnBasePointSizeChanged(double value) => ApplySettings();
    partial void OnSizeVariationChanged(double value) => ApplySettings();
    partial void OnPulseSpeedChanged(double value) => ApplySettings();
    partial void OnPulseAmountChanged(double value) => ApplySettings();
    partial void OnDepthScalingChanged(bool value) => ApplySettings();
    partial void OnTrailLengthChanged(int value) => ApplySettings();
    partial void OnTrailOpacityChanged(double value) => ApplySettings();
    partial void OnSelectedTrailStyleIndexChanged(int value) => ApplySettings();
    partial void OnCameraDistanceChanged(double value) => ApplySettings();
    partial void OnUseGradientColorsChanged(bool value) => ApplySettings();

    /// <summary>
    /// Updates the selected point info display.
    /// </summary>
    public void UpdateSelectedPointInfo(SpherePoint? point)
    {
        if (point == null)
        {
            SelectedPointInfo = string.Empty;
        }
        else
        {
            var pos = point.CurrentPosition;
            SelectedPointInfo = $"Point #{point.Index}\nPosition: ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})";
        }
    }

    /// <summary>
    /// Updates the count of selected points.
    /// </summary>
    public void UpdateSelectedCount()
    {
        int count = 0;
        foreach (var point in _renderer.Points)
        {
            if (point.IsSelected)
            {
                count++;
            }
        }
        SelectedPointCount = count;
    }

    [RelayCommand]
    private void Reset()
    {
        _renderer.Reset();
        SelectedPointInfo = string.Empty;
        SelectedPointCount = 0;
    }

    [RelayCommand]
    private void Randomize()
    {
        var random = new Random();
        PointCount = random.Next(100, 500);
        RotationSpeed = random.NextDouble() * 1.5;
        WobbleAmplitude = random.NextDouble() * 0.3;
        WobbleFrequency = 1.0 + random.NextDouble() * 3.0;
        BasePointSize = 4.0 + random.NextDouble() * 12.0;
        TrailLength = random.Next(0, 40);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        _renderer.ClearSelection();
        SelectedPointInfo = string.Empty;
        SelectedPointCount = 0;
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var point in _renderer.Points)
        {
            point.IsSelected = true;
        }
        UpdateSelectedCount();
    }
}

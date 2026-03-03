using System.Numerics;

namespace UnoVox.Services;

/// <summary>
/// Simple 1D Kalman filter for smoothing position values
/// More responsive than simple averaging while reducing jitter
/// </summary>
public class KalmanFilter
{
    private float _estimate;
    private float _errorCovariance;
    
    private readonly float _processNoise; // Q: How much we trust the model
    private readonly float _measurementNoise; // R: How much we trust the measurements
    
    private bool _initialized = false;

    public KalmanFilter(float processNoise = 0.01f, float measurementNoise = 0.1f)
    {
        _processNoise = processNoise;
        _measurementNoise = measurementNoise;
        _errorCovariance = 1.0f;
    }

    /// <summary>
    /// Update the filter with a new measurement and get the filtered estimate
    /// </summary>
    public float Update(float measurement)
    {
        if (!_initialized)
        {
            _estimate = measurement;
            _initialized = true;
            return _estimate;
        }

        // Prediction step
        var predictedEstimate = _estimate;
        var predictedCovariance = _errorCovariance + _processNoise;

        // Update step
        var kalmanGain = predictedCovariance / (predictedCovariance + _measurementNoise);
        _estimate = predictedEstimate + kalmanGain * (measurement - predictedEstimate);
        _errorCovariance = (1 - kalmanGain) * predictedCovariance;

        return _estimate;
    }

    /// <summary>
    /// Reset the filter state
    /// </summary>
    public void Reset()
    {
        _initialized = false;
        _errorCovariance = 1.0f;
    }
}

/// <summary>
/// 3D Kalman filter for smoothing hand positions using System.Numerics
/// </summary>
public class KalmanFilter3D
{
    private readonly KalmanFilter _filterX;
    private readonly KalmanFilter _filterY;
    private readonly KalmanFilter _filterZ;

    public KalmanFilter3D(float processNoise = 0.01f, float measurementNoise = 0.1f)
    {
        _filterX = new KalmanFilter(processNoise, measurementNoise);
        _filterY = new KalmanFilter(processNoise, measurementNoise);
        _filterZ = new KalmanFilter(processNoise, measurementNoise);
    }

    /// <summary>
    /// Update all three axes and return smoothed position using Vector3
    /// </summary>
    public Vector3 Update(Vector3 measurement)
    {
        return new Vector3(
            _filterX.Update(measurement.X),
            _filterY.Update(measurement.Y),
            _filterZ.Update(measurement.Z)
        );
    }
    
    /// <summary>
    /// Update all three axes from individual components (backwards compatibility)
    /// </summary>
    public (float x, float y, float z) Update(float x, float y, float z)
    {
        return (
            _filterX.Update(x),
            _filterY.Update(y),
            _filterZ.Update(z)
        );
    }

    /// <summary>
    /// Reset all filters
    /// </summary>
    public void Reset()
    {
        _filterX.Reset();
        _filterY.Reset();
        _filterZ.Reset();
    }
}

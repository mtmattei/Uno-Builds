using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Options;
using UnoVox.Configuration;
using UnoVox.Models;

namespace UnoVox.Services
{
    /// <summary>
    /// Smooths detected hand landmarks per hand using Kalman filters.
    /// Greatly reduces jitter and improves gesture stability.
    /// </summary>
    public sealed class LandmarkSmoother
    {
        private readonly Dictionary<int, KalmanFilter3D[]> _handFilters = new();
        private readonly float _processNoise;
        private readonly float _measurementNoise;

        public LandmarkSmoother(IOptions<HandTrackingConfig> config)
        {
            var cfg = config.Value;
            _processNoise = cfg.LandmarkSmootherProcessNoise;
            _measurementNoise = cfg.LandmarkSmootherMeasurementNoise;

            Console.WriteLine($"[LandmarkSmoother] Initialized with process={_processNoise:F3}, measure={_measurementNoise:F3}");
        }

        /// <summary>
        /// Smooth a single hand detection and return a new HandDetection with filtered landmarks.
        /// </summary>
        public HandDetection Smooth(HandDetection hand)
        {
            if (!_handFilters.TryGetValue(hand.HandId, out var filters))
            {
                filters = new KalmanFilter3D[21];
                for (int i = 0; i < filters.Length; i++)
                {
                    filters[i] = new KalmanFilter3D(_processNoise, _measurementNoise);
                }
                _handFilters[hand.HandId] = filters;
            }

            var smoothed = new List<HandLandmark>(hand.Landmarks.Count);
            foreach (var lm in hand.Landmarks)
            {
                var idx = Math.Clamp(lm.Index, 0, 20);
                var filtered = filters[idx].Update(new Vector3(lm.X, lm.Y, lm.Z));
                smoothed.Add(new HandLandmark(idx, filtered.X, filtered.Y, filtered.Z));
            }

            return new HandDetection(hand.HandId, smoothed);
        }

        /// <summary>
        /// Reset smoothing state for all hands (e.g., when camera stops).
        /// </summary>
        public void Reset()
        {
            _handFilters.Clear();
        }
    }
}

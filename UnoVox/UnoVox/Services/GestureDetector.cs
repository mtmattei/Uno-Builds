using System.Numerics;
using Microsoft.Extensions.Options;
using UnoVox.Configuration;
using UnoVox.Models;

namespace UnoVox.Services;

public class GestureDetector
{
    // Finger tip indices: thumb=4, index=8, middle=12, ring=16, pinky=20
    // Finger base indices: thumb=2, index=6, middle=10, ring=14, pinky=18
    // Wrist = 0

    private readonly HandTrackingConfig _config;
    private int _logCounter = 0; // Log every 60 frames to avoid spam

    public GestureDetector(IOptions<HandTrackingConfig> config)
    {
        _config = config.Value;
        Console.WriteLine($"[GestureDetector] Initialized with pinch={_config.BasePinchThreshold:F2}, " +
                         $"extension={_config.FingerExtensionRatio:F2}, " +
                         $"confidence={_config.MinGestureConfidenceThreshold:F2}");
    }

    /// <summary>
    /// Calculate hand scale from wrist to middle finger tip distance
    /// This normalizes for different hand sizes and distances from camera
    /// </summary>
    private float CalculateHandScale(HandLandmark[] landmarks)
    {
        // Use average distance from wrist to index and middle fingertips for a more stable scale
        var wrist = landmarks[0];
        var indexTip = landmarks[8];
        var middleTip = landmarks[12];
        var d1 = Distance(wrist, indexTip);
        var d2 = Distance(wrist, middleTip);
        var handSize = (d1 + d2) * 0.5f;
        return Math.Max(handSize, 1e-4f);
    }

    public HandGesture DetectGesture(HandDetection hand)
    {
        if (hand.Landmarks.Count < 21)
            return HandGesture.None;

        var landmarks = hand.Landmarks.ToArray();
        
        // Calculate hand size for normalized thresholds
        var handScale = CalculateHandScale(landmarks);

        // Calculate confidence scores for all gestures (with normalized thresholds)
        var scores = new Dictionary<GestureType, float>
        {
            [GestureType.Pinch] = CalculatePinchConfidence(landmarks, handScale),
            [GestureType.ClosedFist] = CalculateClosedFistConfidence(landmarks, handScale),
            [GestureType.Point] = CalculatePointConfidence(landmarks, handScale),
            [GestureType.OpenPalm] = CalculateOpenPalmConfidence(landmarks, handScale)
        };

        // Find best and second best matches
        var sorted = scores.OrderByDescending(x => x.Value).ToList();
        var bestMatch = sorted[0];
        var secondBest = sorted.Count > 1 ? sorted[1] : new KeyValuePair<GestureType, float>(GestureType.None, 0);

        // Winner must be significantly better than second place to avoid ambiguity
        if (bestMatch.Value > 0.5f && bestMatch.Value > secondBest.Value * _config.MinGestureConfidenceThreshold)
        {
            // Map confidence 0.5-1.0 to output confidence 0.7-0.95
            var normalizedConfidence = 0.7f + (bestMatch.Value - 0.5f) * 0.5f;

            // Log every 60 frames to track gesture detection
            if (_logCounter++ % 60 == 0)
            {
                Console.WriteLine($"[GestureDetector] Detected {bestMatch.Key} (conf={normalizedConfidence:F2}, " +
                                $"raw={bestMatch.Value:F2}, second={secondBest.Key}:{secondBest.Value:F2})");
            }

            return new HandGesture(bestMatch.Key, normalizedConfidence);
        }

        // No clear winner - return None
        return new HandGesture(GestureType.None, 0.0f);
    }

    /// <summary>
    /// Calculate confidence score for pinch gesture (0-1)
    /// Pinch = thumb and index fingertips close, other fingers can be relaxed
    /// </summary>
    private float CalculatePinchConfidence(HandLandmark[] landmarks, float handScale)
    {
        var thumbTip = landmarks[4];
        var indexTip = landmarks[8];
        var middleTip = landmarks[12];
        var ringTip = landmarks[16];
        
        // Normalize pinch threshold by hand size
        var pinchThreshold = _config.BasePinchThreshold * handScale;
        var thumbIndexDist = Distance(thumbTip, indexTip);

        // Core requirement: thumb and index close together
        if (thumbIndexDist >= pinchThreshold)
        {
            return 0f;
        }
        
        // Score based on how close (closer = higher confidence)
        // Use squared distance for more forgiving detection at slightly larger distances
        var distanceRatio = thumbIndexDist / pinchThreshold;
        var distanceScore = 1.0f - (distanceRatio * distanceRatio); // Quadratic falloff
        
        // Bonus: middle finger NOT also pinched (distinguishes from fist)
        var middleIndexDist = Distance(middleTip, indexTip);
        var ringIndexDist = Distance(ringTip, indexTip);
        
        // Check if other fingers are separated from the pinch point
        var otherFingersSeparated = middleIndexDist > thumbIndexDist * 1.2f || ringIndexDist > thumbIndexDist * 1.2f;
        var separationScore = otherFingersSeparated ? 1.0f : 0.7f;

        var finalScore = distanceScore * separationScore;

        return finalScore;
    }

    /// <summary>
    /// Detect resize gesture (thumb-pinky spread/pinch)
    /// Returns (IsActive, Spread) where Spread is normalized 0.0-1.0
    /// </summary>
    public static (bool IsActive, float Spread) DetectResizeGesture(HandDetection hand, double handScale = 1.0)
    {
        if (hand.Landmarks.Count < 21)
            return (false, 0f);

        var thumbTip = hand.Landmarks[4];    // Thumb tip
        var pinkyTip = hand.Landmarks[20];   // Pinky tip
        var wrist = hand.Landmarks[0];       // Wrist for normalization

        // Calculate thumb-pinky distance
        var spreadDistance = CalculateDistance(thumbTip, pinkyTip);
        
        // Normalize by hand scale (wrist to middle finger base distance)
        var middleBase = hand.Landmarks[9]; // Palm center
        var handSize = CalculateDistance(wrist, middleBase);
        var normalizedSpread = (float)(spreadDistance / (handSize * handScale));

        // Gesture is active when spread is significant (> 1.5x hand size)
        var isActive = normalizedSpread > 1.5f;
        
        // Clamp spread to 0-1 range for UI (1.5 = minimum, 3.5 = maximum)
        var spreadValue = Math.Clamp((normalizedSpread - 1.5f) / 2.0f, 0f, 1f);

        return (isActive, spreadValue);
    }

    private static float CalculateDistance(HandLandmark a, HandLandmark b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Calculate confidence score for closed fist (0-1)
    /// All fingers curled close to palm
    /// </summary>
    private float CalculateClosedFistConfidence(HandLandmark[] landmarks, float handScale)
    {
        var palm = landmarks[0]; // Wrist as palm reference
        var indexTip = landmarks[8];
        var middleTip = landmarks[12];
        var ringTip = landmarks[16];
        var pinkyTip = landmarks[20];

        // Calculate how close all fingertips are to palm
        var distances = new[]
        {
            Distance(indexTip, palm),
            Distance(middleTip, palm),
            Distance(ringTip, palm),
            Distance(pinkyTip, palm)
        };

        // All fingers should be close (< 0.25)
        var maxAllowedDistance = 0.25f;
        var avgDistance = distances.Average();
        
        if (avgDistance >= maxAllowedDistance)
            return 0f;
        
        // Score: closer = higher confidence
        var score = 1.0f - (avgDistance / maxAllowedDistance);
        
        // Penalty if fingers are spread (check variance)
        var variance = distances.Sum(d => Math.Pow(d - avgDistance, 2)) / distances.Length;
        var tightnessBonus = variance < 0.01 ? 1.0f : 0.7f;
        
        return score * tightnessBonus;
    }

    /// <summary>
    /// Calculate confidence score for pointing gesture (0-1)
    /// Index finger extended, other fingers curled
    /// </summary>
    private float CalculatePointConfidence(HandLandmark[] landmarks, float handScale)
    {
        var palm = landmarks[0];
        
        // Check finger extension states
        var indexExtended = IsFingerExtended(landmarks, 8, 6, palm);
        var middleCurled = !IsFingerExtended(landmarks, 12, 10, palm);
        var ringCurled = !IsFingerExtended(landmarks, 16, 14, palm);
        var pinkyCurled = !IsFingerExtended(landmarks, 20, 18, palm);

        // Core requirement: only index extended
        if (!indexExtended)
            return 0f;
        
        // Score based on how many other fingers are curled (more curled = better point)
        var curledCount = 0;
        if (middleCurled) curledCount++;
        if (ringCurled) curledCount++;
        if (pinkyCurled) curledCount++;
        
        var score = curledCount / 3.0f; // 0.33, 0.66, or 1.0
        
        // Bonus: index should be significantly extended (normalized by hand scale)
        var indexTip = landmarks[8];
        var indexDist = Distance(indexTip, palm);
        var minExtension = 0.3f * handScale;
        var extensionBonus = indexDist > minExtension ? 1.2f : 1.0f;
        
        return Math.Min(score * extensionBonus, 1.0f);
    }

    /// <summary>
    /// Calculate confidence score for open palm (0-1)
    /// All fingers extended and spread
    /// </summary>
    private float CalculateOpenPalmConfidence(HandLandmark[] landmarks, float handScale)
    {
        var palm = landmarks[0];
        
        // Check all fingers extended
        var fingersExtended = new[]
        {
            IsFingerExtended(landmarks, 8, 6, palm),   // Index
            IsFingerExtended(landmarks, 12, 10, palm), // Middle  
            IsFingerExtended(landmarks, 16, 14, palm), // Ring
            IsFingerExtended(landmarks, 20, 18, palm)  // Pinky
        };

        var extendedCount = fingersExtended.Count(x => x);
        
        // Need at least 3 fingers extended for open palm
        if (extendedCount < 3)
            return 0f;
        
        // Score based on how many fingers extended
        var score = extendedCount / 4.0f;
        
        // Bonus: fingers should be spread apart (check distances between fingertips)
        var indexTip = landmarks[8];
        var middleTip = landmarks[12];
        var ringTip = landmarks[16];
        
        var indexMiddleDist = Distance(indexTip, middleTip);
        var middleRingDist = Distance(middleTip, ringTip);
        var avgSpread = (indexMiddleDist + middleRingDist) / 2.0f;
        
        var spreadBonus = avgSpread > 0.08f ? 1.2f : 0.9f;
        
        return Math.Min(score * spreadBonus, 1.0f);
    }

    /// <summary>
    /// Check if a finger is extended by comparing distances
    /// Tip should be farther from palm than base is
    /// </summary>
    private bool IsFingerExtended(HandLandmark[] landmarks, int tipIndex, int baseIndex, HandLandmark palm)
    {
        var tip = landmarks[tipIndex];
        var basePoint = landmarks[baseIndex];

        var tipToPalm = Distance(tip, palm);
        var baseToPalm = Distance(basePoint, palm);

        // Finger is extended if tip is significantly farther from palm than base
        return tipToPalm > baseToPalm * _config.FingerExtensionRatio;
    }

    private float Distance(HandLandmark a, HandLandmark b)
    {
        Vector3 aVec = a.ToVector3();
        Vector3 bVec = b.ToVector3();
        return Vector3.Distance(aVec, bVec);
    }
}

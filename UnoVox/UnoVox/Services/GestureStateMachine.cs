using Microsoft.Extensions.Options;
using UnoVox.Configuration;
using UnoVox.Models;

namespace UnoVox.Services;

/// <summary>
/// Implements a state machine for gesture recognition with multi-frame confidence.
/// Prevents false positives by requiring consistent gesture detection across multiple frames.
/// OPTIMIZED for better pinch responsiveness and continuous drawing.
/// </summary>
public class GestureStateMachine
{
    private readonly HandTrackingConfig _config;
    private readonly int _requiredConfirmationFrames;
    private readonly float _minConfidenceThreshold;
    private readonly float _hysteresisThreshold;
    private readonly float _weakContinuationThreshold;

    private GestureType _currentState = GestureType.None;
    private GestureType _pendingState = GestureType.None;
    private int _confirmationFrameCount = 0;
    private Queue<(GestureType type, float confidence)> _gestureHistory = new(10);
    private int _logCounter = 0; // Log every 90 frames to avoid spam

    public GestureStateMachine(IOptions<HandTrackingConfig> config)
    {
        _config = config.Value;
        _requiredConfirmationFrames = _config.GestureConfirmationFrames;
        _minConfidenceThreshold = _config.GestureConfidenceThreshold;
        _hysteresisThreshold = _config.GestureHysteresisThreshold;
        _weakContinuationThreshold = _config.GestureWeakContinuationThreshold;

        Console.WriteLine($"[GestureStateMachine] Initialized with confirmFrames={_requiredConfirmationFrames}, " +
                         $"minConf={_minConfidenceThreshold:F2}, " +
                         $"hysteresis={_hysteresisThreshold:F2}, " +
                         $"weakCont={_weakContinuationThreshold:F2}");
    }
    
    /// <summary>
    /// Current confirmed gesture state
    /// </summary>
    public GestureType CurrentGesture => _currentState;
    
    /// <summary>
    /// Whether the current gesture is confirmed and stable
    /// </summary>
    public bool IsGestureConfirmed => _confirmationFrameCount >= _requiredConfirmationFrames;
    
    /// <summary>
    /// Updates the state machine with a new gesture detection
    /// </summary>
    /// <param name="detectedGesture">The gesture detected in the current frame</param>
    /// <returns>True if the gesture state changed</returns>
    public bool Update(HandGesture detectedGesture)
    {
        var type = detectedGesture.Type;
        var confidence = detectedGesture.Confidence;
        
        // Add to history for temporal analysis
        _gestureHistory.Enqueue((type, confidence));
        if (_gestureHistory.Count >= 10)
            _gestureHistory.Dequeue();
        
        // State machine logic with hysteresis
        bool stateChanged = false;
        
        if (_currentState == GestureType.None)
        {
            // In idle state - require high confidence to enter gesture
            if (type != GestureType.None && confidence >= _minConfidenceThreshold)
            {
                if (_pendingState == type)
                {
                    _confirmationFrameCount++;
                    
                    if (_confirmationFrameCount >= _requiredConfirmationFrames)
                    {
                        // Gesture confirmed!
                        _currentState = type;
                        stateChanged = true;

                        Console.WriteLine($"[GestureStateMachine] ✓ Gesture confirmed: {type} (confidence={confidence:F2})");
                    }
                }
                else
                {
                    // New gesture detected, start confirmation
                    _pendingState = type;
                    _confirmationFrameCount = 1;
                }
            }
            else
            {
                // Reset if no valid gesture
                _pendingState = GestureType.None;
                _confirmationFrameCount = 0;
            }
        }
        else
        {
            // In active gesture state - use hysteresis (lower threshold to exit)
            if (type == _currentState && confidence >= _hysteresisThreshold)
            {
                // Gesture continues strongly - keep it confirmed for continuous actions
                _confirmationFrameCount = _requiredConfirmationFrames;

                // Log every 90 frames to track continuous gestures
                if (_logCounter++ % 90 == 0)
                {
                    Console.WriteLine($"[GestureStateMachine] Continuing {type} (confidence={confidence:F2})");
                }
            }
            else if (type == _currentState && confidence >= _weakContinuationThreshold)
            {
                // OPTIMIZED: Gesture weakening but user is still trying - be VERY forgiving for continuous drawing
                // This is KEY for smooth pinch-and-drag drawing!
                if (_confirmationFrameCount >= _requiredConfirmationFrames)
                {
                    // Keep gesture active - user is probably still pinching while moving
                    _confirmationFrameCount = _requiredConfirmationFrames;
                }
                else
                {
                    // Still building up confirmation
                    _confirmationFrameCount = Math.Max(_confirmationFrameCount - 1, 0);
                }
            }
            else if (type == _currentState && confidence > 0f)
            {
                // Very weak confidence but still detected - slowly lose confirmation
                _confirmationFrameCount = Math.Max(_confirmationFrameCount - 1, 0);
                if (_confirmationFrameCount == 0)
                {
                    // Lost confirmation
                    Console.WriteLine($"[GestureStateMachine] Lost gesture: {_currentState} (confidence too low)");
                    _currentState = GestureType.None;
                    _pendingState = GestureType.None;
                    stateChanged = true;
                }
            }
            else
            {
                // Different gesture or no gesture detected - switch/exit
                _currentState = GestureType.None;
                _pendingState = type != GestureType.None ? type : GestureType.None;
                _confirmationFrameCount = type != GestureType.None ? 1 : 0;
                stateChanged = true;
            }
        }
        
        return stateChanged;
    }
    
    /// <summary>
    /// Gets the average confidence over the last N frames for temporal stability analysis
    /// </summary>
    public float GetAverageConfidence(int frameCount = 5)
    {
        if (_gestureHistory.Count == 0) return 0f;
        
        var recent = _gestureHistory
            .Where(g => g.type == _currentState)
            .TakeLast(frameCount)
            .ToList();
        
        return recent.Count > 0 ? recent.Average(g => g.confidence) : 0f;
    }
    
    /// <summary>
    /// Gets the current state as a string with confirmation status
    /// </summary>
    public string GetStateDescription()
    {
        if (_currentState != GestureType.None)
        {
            return $"{_currentState} (Confirmed)";
        }
        else if (_pendingState != GestureType.None)
        {
            return $"{_pendingState} (Pending {_confirmationFrameCount}/{_requiredConfirmationFrames})";
        }
        else
        {
            return "None";
        }
    }
    
    /// <summary>
    /// Resets the state machine to idle
    /// </summary>
    public void Reset()
    {
        _currentState = GestureType.None;
        _pendingState = GestureType.None;
        _confirmationFrameCount = 0;
        _gestureHistory.Clear();
    }
}

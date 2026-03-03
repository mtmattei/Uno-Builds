using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SkiaSharp;
using UnoVox.Models;

namespace UnoVox.Services;

public class OnnxHandTracker : IHandTracker, IDisposable
{
    private InferenceSession? _palmSession;
    private InferenceSession? _landmarkSession;
    private bool _isInitialized;
    private bool _modelsAvailable = false;
    private readonly object _lock = new();

    public bool ModelsAvailable => _modelsAvailable;
    
    // Motion detection for filtering static objects (walls, neck, etc.)
    private Mat? _previousFrame;
    private Mat? _backgroundFrame; // Captured background for better subtraction
    private Point? _previousHandCenter;
    private int _staticFrameCount = 0;
    private const int MinMotionPixels = 10; // Minimum pixels moved to be considered motion
    private bool _useBackgroundSubtraction = false;
    
    // Hand tracking state for confirmed hands
    private readonly Dictionary<int, (Point center, int confirmedFrames)> _trackedHands = new();
    private int _nextHandId = 0;
    private const int HandTrackingDistance = 100; // Max pixels to consider same hand
    private const int HandConfirmationFrames = 3; // Frames to confirm a hand

    public async Task<bool> InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] Initializing...");
            // For now, we'll use a simplified approach with OpenCV-based tracking
            // Full MediaPipe ONNX models require downloading ~6MB of model files
            // TODO: Download and load actual MediaPipe models
            _isInitialized = true;
            System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] Initialized successfully");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Init failed: {ex.Message}");
            _isInitialized = false;
            return false;
        }
    }

    public async Task<IReadOnlyList<HandDetection>> DetectAsync(SKBitmap frame, CancellationToken ct = default)
    {
        System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] DetectAsync called, initialized: {_isInitialized}, frame: {frame?.Width}x{frame?.Height}");
        
        if (!_isInitialized || frame == null)
        {
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Skipping detection - initialized: {_isInitialized}, frame null: {frame == null}");
            return Array.Empty<HandDetection>();
        }

        try
        {
            // Convert SKBitmap to OpenCV Mat
            using var mat = ConvertToMat(frame);
            if (mat == null || mat.Empty())
                return Array.Empty<HandDetection>();

            // Simple skin color detection to find hand regions
            var hands = await DetectHandsViaSkinColor(mat, ct);
            return hands;
        }
        catch
        {
            return Array.Empty<HandDetection>();
        }
    }

    private Mat? ConvertToMat(SKBitmap bitmap)
    {
        try
        {
            var mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC4);
            unsafe
            {
                var src = (byte*)bitmap.GetPixels();
                var dst = mat.Data;
                Buffer.MemoryCopy(src, dst.ToPointer(), mat.Total() * 4, bitmap.ByteCount);
            }
            return mat;
        }
        catch
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<HandDetection>> DetectHandsViaSkinColor(Mat frame, CancellationToken ct)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Processing frame: {frame.Width}x{frame.Height}");

            // Multi-stage skin detection using both HSV and YCrCb color spaces
            // This reduces false positives from white walls while still detecting hands
            
            using var bgr = new Mat();
            Cv2.CvtColor(frame, bgr, ColorConversionCodes.BGRA2BGR);
            
            // Background subtraction if calibrated
            using var motionMask = new Mat();
            if (_useBackgroundSubtraction && _backgroundFrame != null && !_backgroundFrame.Empty())
            {
                // Use background frame for better motion detection
                using var diff = new Mat();
                Cv2.Absdiff(bgr, _backgroundFrame, diff);
                using var gray = new Mat();
                Cv2.CvtColor(diff, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(gray, motionMask, 30, 255, ThresholdTypes.Binary);
                
                // Dilate motion mask more aggressively with background subtraction
                using var motionKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(20, 20));
                Cv2.Dilate(motionMask, motionMask, motionKernel, iterations: 4);
            }
            else if (_previousFrame != null && !_previousFrame.Empty())
            {
                // Fall back to frame-to-frame motion detection
                using var diff = new Mat();
                Cv2.Absdiff(bgr, _previousFrame, diff);
                using var gray = new Mat();
                Cv2.CvtColor(diff, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(gray, motionMask, 25, 255, ThresholdTypes.Binary);
                
                // Dilate motion mask to connect hand regions
                using var motionKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(15, 15));
                Cv2.Dilate(motionMask, motionMask, motionKernel, iterations: 3);
            }
            else
            {
                // First frame - accept all (will be filtered by shape)
                motionMask.Create(bgr.Size(), MatType.CV_8UC1);
                motionMask.SetTo(Scalar.All(255));
            }
            
            // Store current frame for next comparison (don't overwrite background!)
            _previousFrame?.Dispose();
            _previousFrame = bgr.Clone();
            
            // HSV detection (good for varying lighting)
            using var hsv = new Mat();
            Cv2.CvtColor(bgr, hsv, ColorConversionCodes.BGR2HSV);
            
            // HSV skin ranges - MUCH more restrictive to avoid false positives
            var lowerSkin1 = new Scalar(0, 40, 60);  // Higher saturation minimum (was 20)
            var upperSkin1 = new Scalar(20, 255, 255);  // Narrower hue range (was 25)
            
            var lowerSkin2 = new Scalar(165, 40, 60);  // Higher saturation minimum
            var upperSkin2 = new Scalar(180, 255, 255);
            
            using var hsvMask1 = new Mat();
            using var hsvMask2 = new Mat();
            using var hsvMask = new Mat();
            
            Cv2.InRange(hsv, lowerSkin1, upperSkin1, hsvMask1);
            Cv2.InRange(hsv, lowerSkin2, upperSkin2, hsvMask2);
            Cv2.BitwiseOr(hsvMask1, hsvMask2, hsvMask);
            
            // YCrCb detection (better for skin tones, less affected by walls)
            using var ycrcb = new Mat();
            Cv2.CvtColor(bgr, ycrcb, ColorConversionCodes.BGR2YCrCb);
            
            // YCrCb ranges - tighter to specifically target skin
            var lowerYCrCb = new Scalar(0, 135, 85);  // More restrictive Cr and Cb (was 133, 77)
            var upperYCrCb = new Scalar(255, 170, 120);  // Narrower range (was 173, 127)
            
            using var ycrcbMask = new Mat();
            Cv2.InRange(ycrcb, lowerYCrCb, upperYCrCb, ycrcbMask);
            
            // Combine all three masks - must pass color detection AND motion (triple AND)
            using var colorMask = new Mat();
            Cv2.BitwiseAnd(hsvMask, ycrcbMask, colorMask);
            
            using var mask = new Mat();
            Cv2.BitwiseAnd(colorMask, motionMask, mask);

            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Mask created, non-zero pixels: {Cv2.CountNonZero(mask)}");

            // Apply morphological operations to reduce noise - more aggressive erosion
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7));
            Cv2.Erode(mask, mask, kernel, iterations: 3);  // More erosion to break apart large regions
            Cv2.Dilate(mask, mask, kernel, iterations: 2); // Less dilation
            Cv2.GaussianBlur(mask, mask, new Size(5, 5), 0);

            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] After morphology, non-zero pixels: {Cv2.CountNonZero(mask)}");

            // Find contours
            Cv2.FindContours(mask, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Found {contours.Length} contours");

            if (contours.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] No contours found!");
                return await Task.FromResult(Array.Empty<HandDetection>());
            }

            // Filter contours by hand characteristics with shape analysis
            var handCandidates = new List<(Point[] contour, double area, Rect bounds, double score)>();
            
            foreach (var contour in contours)
            {
                var area = Cv2.ContourArea(contour);
                
                // Stricter size requirements - reject too small AND too large
                if (area < 2000 || area > 80000) continue;  // Was 1000 min, no max
                
                var bounds = Cv2.BoundingRect(contour);
                
                // Reject objects that are too wide or tall (likely torso/neck)
                if (bounds.Width > frame.Width * 0.6 || bounds.Height > frame.Height * 0.6) continue;
                
                // Calculate shape metrics to distinguish hands from walls/background
                var perimeter = Cv2.ArcLength(contour, true);
                var compactness = 4 * Math.PI * area / (perimeter * perimeter); // Circle = 1.0, irregular = lower
                
                // Convex hull for solidity calculation
                using var convexHull = new Mat();
                Cv2.ConvexHull(InputArray.Create(contour), convexHull);
                var hullArea = Cv2.ContourArea(convexHull);
                var solidity = hullArea > 0 ? area / hullArea : 0; // Hand = 0.7-0.9, wall = close to 1.0
                
                var aspectRatio = (double)bounds.Height / bounds.Width;
                var centerY = bounds.Y + bounds.Height / 2;
                var centerX = bounds.X + bounds.Width / 2;
                var relativeY = (double)centerY / frame.Height;
                var relativeX = (double)centerX / frame.Width;
                
                System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Contour: area={area:F0}, compact={compactness:F3}, solid={solidity:F3}, aspect={aspectRatio:F2}, pos=({relativeX:F2},{relativeY:F2})");
                
                // RELAXED FACE FILTERING - Focus on most reliable criteria
                bool isFaceLike = false;
                
                // Hand confidence boost: Lower 60% of frame is more likely to be hands
                bool inHandZone = relativeY > 0.4; // Below 40% from top = hand likely zone
                float handBoost = inHandZone ? 0.3f : 0f; // 30% score boost for lower frame
                
                // Criterion 1: Very large solid object in upper portion (clearly a face/torso)
                if (aspectRatio > 0.85 && aspectRatio < 1.15 && relativeY < 0.4 && area > 30000 && solidity > 0.85)
                {
                    isFaceLike = true;
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ Face detected: large round in upper area");
                }
                
                // Criterion 2: Extremely large solid object anywhere (face very close to camera)
                if (area > 70000 && solidity > 0.88)
                {
                    isFaceLike = true;
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ Face detected: extremely large solid object");
                }
                
                // Criterion 3: Very high solidity in center-top (face directly facing camera)
                if (solidity > 0.92 && area > 25000 && relativeY < 0.5 && 
                    relativeX > 0.3 && relativeX < 0.7)
                {
                    isFaceLike = true;
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ Face detected: high solidity centered");
                }
                
                // Skip remaining checks if clearly face-like
                if (isFaceLike)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ Skipping face-like region");
                    continue;
                }
                
                // SMART FILTERING:
                // 1. Reject very compact, solid shapes (likely walls or flat objects)
                // Walls have high compactness (rectangular) and high solidity (no gaps)
                bool isWallLike = compactness > 0.55 && solidity > 0.90;  // Made stricter
                
                // 2. Require better hand characteristics
                // Hands have visible fingers (lower solidity due to gaps)
                // Aspect ratio should be reasonable (not super wide or tall)
                bool isHandLike = compactness > 0.12 && compactness < 0.55 &&  // Narrower range
                                  solidity > 0.55 && solidity < 0.88 &&         // Stricter solidity
                                  aspectRatio > 0.4 && aspectRatio < 2.5 &&    // Reasonable proportions
                                  relativeY > 0.30;  // Hands in middle to bottom area
                
                // 3. Reject torso/neck - very tall relative to width
                bool isTorsoLike = aspectRatio > 2.0 || (bounds.Height > frame.Height * 0.5 && aspectRatio > 1.5);
                
                if (!isFaceLike && !isWallLike && !isTorsoLike && isHandLike)
                {
                    // Score based on multiple factors - HEAVILY favor bottom half and good shape
                    var positionScore = relativeY > 0.55 ? 4.0 :  // Bottom half gets 4x boost
                                       relativeY > 0.4 ? 1.8 :    // Middle gets 1.8x
                                       0.3;                        // Upper portion heavily penalized
                    
                    // Prefer hand-like solidity (fingers visible = gaps = lower solidity)
                    var shapeScore = (1.0 - Math.Abs(solidity - 0.70)) * 3; // Target 0.70 solidity
                    
                    // Prefer reasonable aspect ratios (not too stretched)
                    var aspectScore = aspectRatio > 0.6 && aspectRatio < 1.8 ? 1.5 : 1.0;
                    
                    var score = area * positionScore * shapeScore * aspectScore * (1.0 + handBoost);
                    
                    handCandidates.Add((contour, area, bounds, score));
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✓ Hand candidate accepted (score={score:F0}, boost={handBoost})");
                }
                else
                {
                    var reason = isFaceLike ? "face" : 
                                 isWallLike ? "wall/flat" : 
                                 isTorsoLike ? "torso/neck" : 
                                 "not hand-like shape";
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ Rejected ({reason})");
                }
            }
            
            if (handCandidates.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] No valid hand candidates found!");
                _trackedHands.Clear(); // Clear tracked hands when none detected
                return await Task.FromResult(Array.Empty<HandDetection>());
            }

            // Support multiple hands - return top 3 candidates (sorted by score)
            var topHands = handCandidates.OrderByDescending(h => h.score).Take(3).ToList();
            var detections = new List<HandDetection>();
            
            // Process each hand candidate
            foreach (var (contour, area, boundRect, score) in topHands)
            {
                var currentCenter = new Point(boundRect.X + boundRect.Width / 2, boundRect.Y + boundRect.Height / 2);
                
                // Check if this is a tracked hand (previously confirmed)
                int? trackedHandId = null;
                foreach (var kvp in _trackedHands)
                {
                    var distance = Math.Sqrt(
                        Math.Pow(currentCenter.X - kvp.Value.center.X, 2) +
                        Math.Pow(currentCenter.Y - kvp.Value.center.Y, 2));
                    
                    if (distance < HandTrackingDistance)
                    {
                        trackedHandId = kvp.Key;
                        break;
                    }
                }
                
                bool isConfirmedHand = false;
                
                if (trackedHandId.HasValue)
                {
                    // This is a tracked hand - update position and increment confirmation
                    var tracked = _trackedHands[trackedHandId.Value];
                    _trackedHands[trackedHandId.Value] = (currentCenter, tracked.confirmedFrames + 1);
                    isConfirmedHand = tracked.confirmedFrames >= HandConfirmationFrames;
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Tracked hand {trackedHandId.Value}, frames={tracked.confirmedFrames + 1}");
                }
                else
                {
                    // New hand candidate - apply motion check
                    if (_previousHandCenter.HasValue)
                    {
                        var distance = Math.Sqrt(
                            Math.Pow(currentCenter.X - _previousHandCenter.Value.X, 2) +
                            Math.Pow(currentCenter.Y - _previousHandCenter.Value.Y, 2));
                        
                        if (distance < MinMotionPixels)
                        {
                            _staticFrameCount++;
                            
                            // Reject NEW candidates that are static (but not tracked hands)
                            if (_staticFrameCount > 5)
                            {
                                System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✗ New candidate rejected: No motion for {_staticFrameCount} frames");
                                continue; // Skip this candidate, try next
                            }
                        }
                        else
                        {
                            _staticFrameCount = 0; // Reset on motion
                        }
                    }
                    
                    // Add as new tracked hand
                    trackedHandId = _nextHandId++;
                    _trackedHands[trackedHandId.Value] = (currentCenter, 1);
                    System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] New hand candidate {trackedHandId.Value}");
                }
                
                _previousHandCenter = currentCenter;
                
                // Generate landmarks from contour analysis
                using var hullIndices = new Mat();
                using var hullPoints = new Mat();
                Cv2.ConvexHull(InputArray.Create(contour), hullPoints, returnPoints: true);
                
                var hull = new List<Point>();
                if (!hullPoints.Empty() && hullPoints.Type() == MatType.CV_32SC2)
                {
                    for (int i = 0; i < hullPoints.Rows; i++)
                    {
                        hull.Add(new Point(hullPoints.At<Vec2i>(i).Item0, hullPoints.At<Vec2i>(i).Item1));
                    }
                }

                var landmarks = GenerateLandmarksFromContour(contour, boundRect, hull.ToArray(), frame.Width, frame.Height);
                
                detections.Add(new HandDetection(detections.Count, landmarks));
                
                System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ✓ Hand detection added (id={trackedHandId}, confirmed={isConfirmedHand}, confidence={( isConfirmedHand ? 0.95f : 0.7f):F2}, landmarks={landmarks.Count})");
            }
            
            // Clean up stale tracked hands (not seen in this frame)
            var currentCenters = topHands.Select(h => new Point(h.bounds.X + h.bounds.Width / 2, h.bounds.Y + h.bounds.Height / 2)).ToList();
            var staleHands = new List<int>();
            
            foreach (var kvp in _trackedHands)
            {
                bool found = false;
                foreach (var center in currentCenters)
                {
                    var dist = Math.Sqrt(Math.Pow(center.X - kvp.Value.center.X, 2) + Math.Pow(center.Y - kvp.Value.center.Y, 2));
                    if (dist < HandTrackingDistance)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    staleHands.Add(kvp.Key);
                }
            }
            
            foreach (var staleId in staleHands)
            {
                _trackedHands.Remove(staleId);
                System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Removed stale hand {staleId}");
            }

            return await Task.FromResult<IReadOnlyList<HandDetection>>(detections);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Stack: {ex.StackTrace}");
            return await Task.FromResult(Array.Empty<HandDetection>());
        }
    }

    private List<HandLandmark> GenerateLandmarksFromContour(Point[] contour, Rect boundRect, Point[] hull, int frameWidth, int frameHeight)
    {
        var landmarks = new List<HandLandmark>(21);

        // Normalize coordinates to [0, 1]
        float Normalize(int value, int max) => Math.Clamp(value / (float)max, 0, 1);

        // Find center of palm (centroid of bounding box)
        var centerX = boundRect.X + boundRect.Width / 2;
        var centerY = boundRect.Y + boundRect.Height / 2;

        // 0: Wrist (bottom center of bounding box)
        var wristY = boundRect.Y + boundRect.Height;
        landmarks.Add(new HandLandmark(0, Normalize(centerX, frameWidth), Normalize(wristY, frameHeight), 0));

        // Find fingertips (points farthest from palm center in different directions)
        // Improved: Use tighter angular sectors for better finger separation
        var fingertips = FindFingertips(hull, centerX, centerY, 5);

        // Generate landmarks for each finger with better positioning
        for (int finger = 0; finger < 5; finger++)
        {
            var fingertip = finger < fingertips.Count ? fingertips[finger] : new Point(centerX, centerY - boundRect.Height / 2);
            
            // Calculate vector from palm to fingertip
            var dirX = (double)(fingertip.X - centerX);
            var dirY = (double)(fingertip.Y - centerY);
            var length = Math.Sqrt(dirX * dirX + dirY * dirY);
            
            if (length > 0)
            {
                dirX /= length;
                dirY /= length;
            }
            
            // Base landmark (closer to palm)
            var baseX = centerX + dirX * length * 0.25;
            var baseY = centerY + dirY * length * 0.25;
            landmarks.Add(new HandLandmark(finger * 4 + 1, Normalize((int)baseX, frameWidth), Normalize((int)baseY, frameHeight), 0));

            // First joint (30% from palm)
            var mid1X = centerX + dirX * length * 0.45;
            var mid1Y = centerY + dirY * length * 0.45;
            landmarks.Add(new HandLandmark(finger * 4 + 2, Normalize((int)mid1X, frameWidth), Normalize((int)mid1Y, frameHeight), 0));

            // Second joint (65% from palm)
            var mid2X = centerX + dirX * length * 0.7;
            var mid2Y = centerY + dirY * length * 0.7;
            landmarks.Add(new HandLandmark(finger * 4 + 3, Normalize((int)mid2X, frameWidth), Normalize((int)mid2Y, frameHeight), 0));

            // Fingertip (100%)
            landmarks.Add(new HandLandmark(finger * 4 + 4, Normalize(fingertip.X, frameWidth), Normalize(fingertip.Y, frameHeight), 0));
        }

        return landmarks;
    }

    private List<Point> FindFingertips(Point[] hull, int centerX, int centerY, int count)
    {
        var fingertips = new List<Point>();
        
        // Divide hull into angular sectors and find farthest point in each
        for (int i = 0; i < count; i++)
        {
            double startAngle = (i / (double)count) * 2 * Math.PI;
            double endAngle = ((i + 1) / (double)count) * 2 * Math.PI;

            Point? farthest = null;
            double maxDist = 0;

            foreach (var pt in hull)
            {
                var angle = Math.Atan2(pt.Y - centerY, pt.X - centerX);
                if (angle < 0) angle += 2 * Math.PI;

                if (angle >= startAngle && angle < endAngle)
                {
                    var dist = Math.Sqrt(Math.Pow(pt.X - centerX, 2) + Math.Pow(pt.Y - centerY, 2));
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        farthest = pt;
                    }
                }
            }

            if (farthest.HasValue)
                fingertips.Add(farthest.Value);
        }

        return fingertips;
    }

    /// <summary>
    /// Captures the current frame as background for improved motion detection
    /// Call this when no hands are visible in the frame
    /// </summary>
    public void CaptureBackground(SKBitmap frame)
    {
        try
        {
            using var mat = ConvertToMat(frame);
            if (mat != null && !mat.Empty())
            {
                _backgroundFrame?.Dispose();
                using var bgr = new Mat();
                Cv2.CvtColor(mat, bgr, ColorConversionCodes.BGRA2BGR);
                _backgroundFrame = bgr.Clone();
                _useBackgroundSubtraction = true;
                System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] Background captured for improved detection");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnnxHandTracker] Failed to capture background: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the background frame and disables background subtraction
    /// </summary>
    public void ClearBackground()
    {
        _backgroundFrame?.Dispose();
        _backgroundFrame = null;
        _useBackgroundSubtraction = false;
        System.Diagnostics.Debug.WriteLine("[OnnxHandTracker] Background cleared");
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _palmSession?.Dispose();
            _landmarkSession?.Dispose();
            _previousFrame?.Dispose();
            _backgroundFrame?.Dispose();
            _palmSession = null;
            _landmarkSession = null;
            _previousFrame = null;
            _backgroundFrame = null;
            _isInitialized = false;
        }
    }
}

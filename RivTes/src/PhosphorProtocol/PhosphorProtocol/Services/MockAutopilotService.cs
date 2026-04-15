using System.Collections.Immutable;
using PhosphorProtocol.Models;

namespace PhosphorProtocol.Services;

public class MockAutopilotService : IAutopilotService
{
    private readonly Random _random = new();
    private double _steeringAngle;
    private int _tick;

    // Road perspective: objects must sit within the converging road trapezoid.
    // The canvas draws vanishY at 0.22, road center at X=0.5, and road half-width
    // narrows from ~0.25 at bottom (Y=1.0) to ~0.01 at vanishing point (Y=0.22).
    private double RoadHalfWidth(double yNorm)
    {
        // yNorm: 0 = vanishing point (0.22 screen), 1 = bottom of screen
        return 0.02 + 0.23 * yNorm;
    }

    private double ScreenY(double depth)
    {
        // depth: 0 = near (bottom), 1 = far (horizon)
        // Maps to screen Y: 0.85 (near) to 0.25 (far)
        return 0.85 - depth * 0.60;
    }

    private double YNormForScreenY(double screenY)
    {
        // Inverse: screen Y to road-relative 0..1 (0=vanish, 1=bottom)
        return (screenY - 0.22) / (1.0 - 0.22);
    }

    public ValueTask<AutopilotState> GetCurrentState(CancellationToken ct)
    {
        _tick++;
        // Gentle steering oscillation (±3°, very subtle)
        _steeringAngle = Math.Sin(_tick * 0.02) * 3.0;

        var objects = ImmutableList.CreateBuilder<DetectedObject>();

        // Lead vehicle — center lane, ahead at depth ~0.55
        double leadDepth = 0.55 + Math.Sin(_tick * 0.015) * 0.05;
        double leadY = ScreenY(leadDepth);
        double leadYNorm = YNormForScreenY(leadY);
        double roadW = RoadHalfWidth(leadYNorm);
        // Slight lateral drift within center lane
        double leadX = 0.5 + Math.Sin(_tick * 0.012) * roadW * 0.08;
        // Size scales with perspective
        double leadScale = leadYNorm;
        objects.Add(new DetectedObject(
            Type: "vehicle",
            RelativeX: leadX,
            RelativeY: leadY,
            Width: 0.08 * leadScale,
            Height: 0.06 * leadScale,
            Confidence: 0.96 + _random.NextDouble() * 0.03,
            Velocity: 58 + _random.Next(-2, 3)));

        // Right lane vehicle — offset right, slightly further
        double rightDepth = 0.45 + Math.Sin(_tick * 0.01) * 0.04;
        double rightY = ScreenY(rightDepth);
        double rightYNorm = YNormForScreenY(rightY);
        double rightRoadW = RoadHalfWidth(rightYNorm);
        double rightX = 0.5 + rightRoadW * 0.55 + Math.Sin(_tick * 0.018) * rightRoadW * 0.05;
        double rightScale = rightYNorm;
        objects.Add(new DetectedObject(
            Type: "vehicle",
            RelativeX: rightX,
            RelativeY: rightY,
            Width: 0.07 * rightScale,
            Height: 0.05 * rightScale,
            Confidence: 0.91 + _random.NextDouble() * 0.05,
            Velocity: 55 + _random.Next(-3, 4)));

        // Occasional pedestrian — on sidewalk (left edge of road)
        if (Math.Sin(_tick * 0.008) > 0.3)
        {
            double pedDepth = 0.35;
            double pedY = ScreenY(pedDepth);
            double pedYNorm = YNormForScreenY(pedY);
            double pedRoadW = RoadHalfWidth(pedYNorm);
            // Just outside left road edge
            double pedX = 0.5 - pedRoadW * 1.15 + Math.Sin(_tick * 0.01) * 0.01;
            double pedScale = pedYNorm;
            objects.Add(new DetectedObject(
                Type: "pedestrian",
                RelativeX: pedX,
                RelativeY: pedY,
                Width: 0.02 * pedScale,
                Height: 0.04 * pedScale,
                Confidence: 0.84 + _random.NextDouble() * 0.10,
                Velocity: 3));
        }

        // Cyclist — left lane, closer
        if (Math.Sin(_tick * 0.006) > 0.5)
        {
            double cycDepth = 0.30;
            double cycY = ScreenY(cycDepth);
            double cycYNorm = YNormForScreenY(cycY);
            double cycRoadW = RoadHalfWidth(cycYNorm);
            double cycX = 0.5 - cycRoadW * 0.45 + Math.Sin(_tick * 0.02) * cycRoadW * 0.06;
            double cycScale = cycYNorm;
            objects.Add(new DetectedObject(
                Type: "cyclist",
                RelativeX: cycX,
                RelativeY: cycY,
                Width: 0.025 * cycScale,
                Height: 0.035 * cycScale,
                Confidence: 0.88 + _random.NextDouble() * 0.08,
                Velocity: 15));
        }

        // Predicted path — straight line down center lane with very gentle drift
        // Matches the road perspective: starts at car (bottom) going to horizon
        var path = ImmutableList.CreateBuilder<PathPoint>();
        for (int i = 0; i < 20; i++)
        {
            double t = i / 19.0; // 0 = near (car), 1 = far (horizon)
            double y = ScreenY(t); // perspective Y
            // Very subtle lane-keeping corrections (±1% of road width at that depth)
            double yNorm = YNormForScreenY(y);
            double laneW = RoadHalfWidth(yNorm);
            double drift = Math.Sin(_tick * 0.015) * laneW * 0.08 * (1.0 - t * 0.5);
            double x = 0.5 + drift;
            double conf = 1.0 - t * 0.35 + _random.NextDouble() * 0.03;
            path.Add(new PathPoint(x, y, Math.Clamp(conf, 0.4, 1.0)));
        }

        return ValueTask.FromResult(new AutopilotState(
            Confidence: 0.92 + Math.Sin(_tick * 0.04) * 0.05 + _random.NextDouble() * 0.02,
            Mode: "FULL SELF-DRIVING",
            InterventionStatus: "NOMINAL",
            SteeringAngle: Math.Round(_steeringAngle, 1),
            ThrottlePercent: Math.Round(42 + Math.Sin(_tick * 0.03) * 8, 0),
            BrakePercent: 0,
            DetectedObjects: objects.Count,
            TrackedVehicles: objects.Count(o => o.Type == "vehicle"),
            TrackedPedestrians: objects.Count(o => o.Type is "pedestrian" or "cyclist"),
            LaneOffset: Math.Sin(_tick * 0.02) * 0.05,
            NextManeuver: "CONTINUE",
            ManeuverDistance: "1.2 mi",
            Objects: objects.ToImmutable(),
            PredictedPath: path.ToImmutable()));
    }
}

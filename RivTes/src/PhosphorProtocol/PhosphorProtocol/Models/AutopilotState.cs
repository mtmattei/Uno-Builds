using System.Collections.Immutable;

namespace PhosphorProtocol.Models;

public record AutopilotState(
    double Confidence,
    string Mode,
    string InterventionStatus,
    double SteeringAngle,
    double ThrottlePercent,
    double BrakePercent,
    int DetectedObjects,
    int TrackedVehicles,
    int TrackedPedestrians,
    double LaneOffset,
    string NextManeuver,
    string ManeuverDistance,
    ImmutableList<DetectedObject> Objects,
    ImmutableList<PathPoint> PredictedPath);

public record DetectedObject(
    string Type,
    double RelativeX,
    double RelativeY,
    double Width,
    double Height,
    double Confidence,
    double Velocity);

public record PathPoint(double X, double Y, double Confidence);

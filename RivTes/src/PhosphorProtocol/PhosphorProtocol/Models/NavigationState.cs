namespace PhosphorProtocol.Models;

public record NavigationState(
    string NextTurnDirection,
    string NextTurnDistance,
    string NextTurnRoad,
    string DestinationName,
    string ETA,
    double CarLatitude,
    double CarLongitude);

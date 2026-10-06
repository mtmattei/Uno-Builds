namespace SmartCity.Services;

/// <summary>
/// Emits the building-twin geometry. Behind an interface so a richer (OBJ-loaded) mesh can replace
/// the stacked-box tower later without touching the view. Decision 1 architecture.
/// </summary>
public interface IBuildingGeometryProvider
{
    /// <summary>Build a tower of <paramref name="floorCount"/> stacked slabs; one flagged floor glows.</summary>
    IReadOnlyList<FloorBox> BuildTower(int floorCount, int flaggedFloor);
}

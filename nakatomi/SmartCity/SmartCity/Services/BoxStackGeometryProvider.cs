namespace SmartCity.Services;

/// <summary>
/// Builds the tower as N stacked extruded boxes (one per storey), centred on the origin and stacked
/// along +Y. The footprint is shaped to evoke Nakatomi Plaza (Fox Plaza): a near-square slender
/// shaft with a slightly wider podium at the base and a terraced, stepped-back crown at the top.
/// Each storey is a prism (vertical walls) so the setbacks read as crisp terraces, not a smooth taper.
/// </summary>
public class BoxStackGeometryProvider : IBuildingGeometryProvider
{
    // Model-space footprint + per-storey height. Units are arbitrary; the view scales to fit.
    private const float BaseHalfWidth = 0.92f;
    private const float BaseHalfDepth = 0.86f;
    private const float FloorHeight = 0.30f;
    private const float ShaftTaper = 0.03f;   // very gentle inward lean — reads as a clean slab

    public IReadOnlyList<FloorBox> BuildTower(int floorCount, int flaggedFloor)
    {
        var floors = new List<FloorBox>(floorCount);
        var totalHeight = floorCount * FloorHeight;
        var yBottomStart = -totalHeight / 2f;

        for (var i = 0; i < floorCount; i++)
        {
            var y0 = yBottomStart + i * FloorHeight;
            var y1 = y0 + FloorHeight * 0.94f; // small gap between storeys reads as floor lines

            // Prism: identical footprint top and bottom of the storey → crisp terrace edges.
            var (hw, hd) = Footprint(i, floorCount);

            var corners = new[]
            {
                new Vec3(-hw, y0, -hd),  // 0
                new Vec3( hw, y0, -hd),  // 1
                new Vec3( hw, y0,  hd),  // 2
                new Vec3(-hw, y0,  hd),  // 3
                new Vec3(-hw, y1, -hd),  // 4
                new Vec3( hw, y1, -hd),  // 5
                new Vec3( hw, y1,  hd),  // 6
                new Vec3(-hw, y1,  hd),  // 7
            };

            var floorNumber = i + 1;
            floors.Add(new FloorBox(floorNumber, $"{floorNumber}F", corners, floorNumber == flaggedFloor));
        }

        return floors;
    }

    /// <summary>Half-width / half-depth for storey <paramref name="i"/> (0 = ground).</summary>
    private static (float Hw, float Hd) Footprint(int i, int floorCount)
    {
        var t = floorCount <= 1 ? 0f : (float)i / (floorCount - 1);
        var lean = 1f - ShaftTaper * t;

        var hw = BaseHalfWidth * lean;
        var hd = BaseHalfDepth * lean;

        // Wider podium plinth at the base (bottom two storeys).
        if (i == 0) { hw *= 1.07f; hd *= 1.07f; }
        else if (i == 1) { hw *= 1.03f; hd *= 1.03f; }

        // Mostly-rectangular slab (reference Fox Plaza): a single slight cap inset, otherwise flat.
        var fromTop = floorCount - 1 - i;
        var crown = fromTop switch
        {
            0 => 0.90f,   // subtle cap inset
            1 => 0.97f,
            _ => 1.0f,
        };

        return (hw * crown, hd * crown);
    }
}

namespace SmartCity.Models;

/// <summary>A point in model space (right-handed: +Y up, tower stacked along Y).</summary>
public readonly record struct Vec3(float X, float Y, float Z);

/// <summary>
/// One floor slab as an extruded box: 8 corners, ordered bottom face 0-3 then top face 4-7
/// (each face CCW from -X/-Z). <see cref="Edges"/> indexes pairs into <see cref="Corners"/>.
/// </summary>
public record FloorBox(int Floor, string Label, Vec3[] Corners, bool IsFlagged)
{
    /// <summary>The 12 box edges as corner-index pairs (bottom ring, top ring, verticals).</summary>
    public static readonly (int A, int B)[] Edges =
    {
        (0,1),(1,2),(2,3),(3,0),   // bottom
        (4,5),(5,6),(6,7),(7,4),   // top
        (0,4),(1,5),(2,6),(3,7),   // verticals
    };

    /// <summary>
    /// The 5 visible faces as 4 corner indices each (4 sides + top, CCW). The bottom face is never
    /// seen from the orbit camera, so it is omitted. Faces are painter's-sorted by view-space depth
    /// at draw time so the 30%-opacity glass reads correctly back-to-front.
    /// </summary>
    public static readonly (int A, int B, int C, int D)[] Faces =
    {
        (0,1,5,4),   // -Z (front-ish)
        (1,2,6,5),   // +X
        (2,3,7,6),   // +Z
        (3,0,4,7),   // -X
        (4,5,6,7),   // top
    };
}

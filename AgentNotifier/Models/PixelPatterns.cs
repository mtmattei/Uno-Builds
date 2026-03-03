namespace AgentNotifier.Models;

public static class PixelPatterns
{
    // 8x8 pixel patterns where '#' = filled, '.' = empty
    public static readonly string[] Working =
    [
        "..####..",
        ".#....#.",
        "#..##..#",
        "#.#..#.#",
        "#.#..#.#",
        "#..##..#",
        ".#....#.",
        "..####.."
    ];

    public static readonly string[] Waiting =
    [
        "...##...",
        "...##...",
        "...##...",
        "...##...",
        "...##...",
        "........",
        "...##...",
        "...##..."
    ];

    public static readonly string[] Finished =
    [
        "........",
        ".......#",
        "......#.",
        "#....#..",
        ".#..#...",
        "..##....",
        "...#....",
        "........"
    ];

    public static readonly string[] Error =
    [
        ".######.",
        "#......#",
        "#.####.#",
        "#.#..#.#",
        "#.####.#",
        "#......#",
        "#..##..#",
        ".######."
    ];

    public static readonly string[] Idle =
    [
        "........",
        "..####..",
        ".#....#.",
        ".#....#.",
        ".#....#.",
        ".#....#.",
        "..####..",
        "........"
    ];

    public static string[] GetPattern(AgentStatus status) => status switch
    {
        AgentStatus.Working => Working,
        AgentStatus.Waiting => Waiting,
        AgentStatus.Finished => Finished,
        AgentStatus.Error => Error,
        _ => Idle
    };
}

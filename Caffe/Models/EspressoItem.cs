namespace Caffe.Models;

public record EspressoItem(string Name, int VolumeMl, string Description);

public enum GrindLevel
{
    Fine = 0,
    Medium = 1,
    Coarse = 2
}

public static class GrindLevelExtensions
{
    public static string GetLabel(this GrindLevel level) => level switch
    {
        GrindLevel.Fine => "Fine",
        GrindLevel.Medium => "Medium",
        GrindLevel.Coarse => "Coarse",
        _ => "Medium"
    };

    public static string GetHint(this GrindLevel level) => level switch
    {
        GrindLevel.Fine => "Slower",
        GrindLevel.Medium => "Balanced",
        GrindLevel.Coarse => "Faster",
        _ => "Balanced"
    };

    public static int GetParticleCount(this GrindLevel level) => level switch
    {
        GrindLevel.Fine => 12,
        GrindLevel.Medium => 9,
        GrindLevel.Coarse => 6,
        _ => 9
    };

    public static double GetParticleSize(this GrindLevel level) => level switch
    {
        GrindLevel.Fine => 3,
        GrindLevel.Medium => 5,
        GrindLevel.Coarse => 8,
        _ => 5
    };

    public static string GetFirstLetter(this GrindLevel level) => level switch
    {
        GrindLevel.Fine => "F",
        GrindLevel.Medium => "M",
        GrindLevel.Coarse => "C",
        _ => "M"
    };
}

namespace ClaudeDash.Models;

public class ActivityDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double MaxValue { get; set; } = 100;
    public string TooltipText { get; set; } = string.Empty;
}

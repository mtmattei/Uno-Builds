namespace ClaudeDash.Models;

public class HourlyActivity
{
    public string HourLabel { get; set; } = string.Empty;
    public double Value { get; set; }
    public double MaxValue { get; set; } = 100;
    public string TooltipText { get; set; } = string.Empty;
}

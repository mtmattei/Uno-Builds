namespace Vitalis.Models;

public static class OrganData
{
    public static readonly Organ Heart = new(
        Id: "heart",
        Name: "Heart",
        Description: "Primary cardiovascular pump maintaining systemic circulation",
        Color: "#ef4444",
        Icon: "\uE95A",
        Metrics:
        [
            new("Heart Rate", "72", "BPM", TrendDirection.Stable, HealthStatus.Optimal),
            new("Blood Pressure", "138/88", "mmHg", TrendDirection.Up, HealthStatus.Warning),
            new("Oxygen Sat.", "98", "%", TrendDirection.Up, HealthStatus.Optimal)
        ],
        History:
        [
            new("00:00", 68), new("04:00", 62), new("08:00", 75),
            new("12:00", 82), new("16:00", 78), new("20:00", 72),
            new("24:00", 70)
        ]
    );

    public static readonly Organ Brain = new(
        Id: "brain",
        Name: "Brain",
        Description: "Central nervous system command center for cognitive functions",
        Color: "#a855f7",
        Icon: "\uE9F5",
        Metrics:
        [
            new("Stress Level", "32", "%", TrendDirection.Down, HealthStatus.Optimal),
            new("Focus Index", "87", "pts", TrendDirection.Up, HealthStatus.Optimal),
            new("Sleep Quality", "7.2", "hrs", TrendDirection.Stable, HealthStatus.Optimal)
        ],
        History:
        [
            new("00:00", 15), new("04:00", 12), new("08:00", 45),
            new("12:00", 52), new("16:00", 38), new("20:00", 28),
            new("24:00", 18)
        ]
    );

    public static readonly Organ Lungs = new(
        Id: "lungs",
        Name: "Lungs",
        Description: "Respiratory system for gas exchange and oxygen delivery",
        Color: "#3b82f6",
        Icon: "\uE9F4",
        Metrics:
        [
            new("Resp. Rate", "16", "/min", TrendDirection.Stable, HealthStatus.Optimal),
            new("Lung Capacity", "94", "%", TrendDirection.Stable, HealthStatus.Optimal),
            new("Air Quality", "Good", "", TrendDirection.Up, HealthStatus.Optimal)
        ],
        History:
        [
            new("00:00", 14), new("04:00", 12), new("08:00", 18),
            new("12:00", 20), new("16:00", 17), new("20:00", 15),
            new("24:00", 14)
        ]
    );

    public static readonly Organ Liver = new(
        Id: "liver",
        Name: "Liver",
        Description: "Metabolic processing center for detoxification and synthesis",
        Color: "#eab308",
        Icon: "\uE9F6",
        Metrics:
        [
            new("Glucose", "95", "mg/dL", TrendDirection.Stable, HealthStatus.Optimal),
            new("ALT Level", "28", "U/L", TrendDirection.Down, HealthStatus.Optimal),
            new("Toxin Load", "Low", "", TrendDirection.Down, HealthStatus.Optimal)
        ],
        History:
        [
            new("00:00", 92), new("04:00", 88), new("08:00", 105),
            new("12:00", 118), new("16:00", 98), new("20:00", 94),
            new("24:00", 90)
        ]
    );

    public static readonly IImmutableList<Organ> All = [Heart, Brain, Lungs, Liver];
}

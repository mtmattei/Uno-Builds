using Wellmetrix.Models;

namespace Wellmetrix.Services;

public interface IHealthDataService
{
    IReadOnlyList<Organ> GetOrgans();
    Organ? GetOrganById(string id);
}

public class HealthDataService : IHealthDataService
{
    private readonly List<Organ> _organs;

    public HealthDataService()
    {
        _organs = CreateSampleData();
    }

    public IReadOnlyList<Organ> GetOrgans() => _organs;

    public Organ? GetOrganById(string id) => _organs.FirstOrDefault(o => o.Id == id);

    private static List<Organ> CreateSampleData()
    {
        return new List<Organ>
        {
            new Organ(
                Id: "heart",
                Name: "Heart",
                Icon: "\uE95B",
                AccentColorKey: "HeartAccentBrush",
                HealthScore: 94,
                Status: "Optimal",
                Metrics: new List<HealthMetric>
                {
                    new("hr", "Heart Rate", "72", "BPM", TrendDirection.Stable, 0, 60, 100,
                        new List<double> { 70, 72, 71, 73, 72, 71, 72 }),
                    new("rhr", "Resting HR", "62", "BPM", TrendDirection.Down, -3, 50, 80,
                        new List<double> { 65, 64, 63, 64, 62, 63, 62 }),
                    new("bp", "Blood Pressure", "118/76", "mmHg", TrendDirection.Stable, 0, 90, 120,
                        new List<double> { 120, 118, 119, 117, 118, 119, 118 }),
                    new("hrv", "HRV", "48", "ms", TrendDirection.Up, 12, 20, 70,
                        new List<double> { 42, 44, 43, 46, 45, 47, 48 })
                },
                Insights: new List<Insight>
                {
                    new("h1", "Your heart rhythm looks steady today. The slight HRV increase suggests good recovery from yesterday's activity.", InsightType.Positive),
                    new("h2", "Consider a 10-minute meditation before bed to further improve heart rate variability.", InsightType.Suggestion),
                    new("h3", "Blood pressure readings have been consistent this week.", InsightType.Neutral)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 91, 75),
                    new("Tue", 89, 70),
                    new("Wed", 93, 80),
                    new("Thu", 92, 78),
                    new("Fri", 94, 85),
                    new("Sat", 95, 88),
                    new("Sun", 94, 85)
                }
            ),
            new Organ(
                Id: "brain",
                Name: "Brain",
                Icon: "\uE9E8",
                AccentColorKey: "BrainAccentBrush",
                HealthScore: 88,
                Status: "Good",
                Metrics: new List<HealthMetric>
                {
                    new("cog", "Cognitive Score", "92", "pts", TrendDirection.Up, 5, 0, 100,
                        new List<double> { 86, 88, 87, 90, 89, 91, 92 }),
                    new("sleep", "Sleep Quality", "85", "%", TrendDirection.Up, 8, 0, 100,
                        new List<double> { 78, 80, 79, 82, 83, 84, 85 }),
                    new("focus", "Focus Index", "78", "pts", TrendDirection.Stable, 0, 0, 100,
                        new List<double> { 76, 78, 77, 79, 78, 77, 78 }),
                    new("stress", "Stress Level", "32", "%", TrendDirection.Down, -15, 0, 100,
                        new List<double> { 45, 42, 40, 38, 35, 34, 32 })
                },
                Insights: new List<Insight>
                {
                    new("b1", "Your cognitive metrics are strong. Sleep quality improvement is contributing to better focus.", InsightType.Positive),
                    new("b2", "Stress levels are down. Keep up the relaxation practices!", InsightType.Positive),
                    new("b3", "Try a 5-minute breathing exercise in the afternoon to boost your focus index.", InsightType.Suggestion)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 82, 65),
                    new("Tue", 84, 70),
                    new("Wed", 83, 68),
                    new("Thu", 86, 75),
                    new("Fri", 85, 72),
                    new("Sat", 87, 78),
                    new("Sun", 88, 82)
                }
            ),
            new Organ(
                Id: "lungs",
                Name: "Lungs",
                Icon: "\uE9D9",
                AccentColorKey: "LungsAccentBrush",
                HealthScore: 91,
                Status: "Optimal",
                Metrics: new List<HealthMetric>
                {
                    new("rr", "Respiratory Rate", "14", "/min", TrendDirection.Stable, 0, 12, 20,
                        new List<double> { 14, 15, 14, 14, 15, 14, 14 }),
                    new("spo2", "SpO2", "98", "%", TrendDirection.Stable, 0, 95, 100,
                        new List<double> { 97, 98, 98, 97, 98, 98, 98 }),
                    new("cap", "Lung Capacity", "94", "%", TrendDirection.Up, 2, 80, 100,
                        new List<double> { 91, 92, 92, 93, 93, 94, 94 }),
                    new("vo2", "VO2 Max", "42", "ml/kg", TrendDirection.Up, 4, 35, 50,
                        new List<double> { 39, 40, 40, 41, 41, 42, 42 })
                },
                Insights: new List<Insight>
                {
                    new("l1", "Excellent respiratory function. Your VO2 max improvement shows cardiovascular fitness gains.", InsightType.Positive),
                    new("l2", "Oxygen saturation is optimal throughout the day.", InsightType.Neutral),
                    new("l3", "Continue your aerobic exercise routine for sustained lung health.", InsightType.Suggestion)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 88, 70),
                    new("Tue", 89, 72),
                    new("Wed", 90, 78),
                    new("Thu", 89, 72),
                    new("Fri", 91, 82),
                    new("Sat", 90, 78),
                    new("Sun", 91, 82)
                }
            ),
            new Organ(
                Id: "liver",
                Name: "Liver",
                Icon: "\uE9DA",
                AccentColorKey: "LiverAccentBrush",
                HealthScore: 86,
                Status: "Good",
                Metrics: new List<HealthMetric>
                {
                    new("alt", "ALT Levels", "28", "U/L", TrendDirection.Stable, 0, 7, 56,
                        new List<double> { 29, 28, 29, 28, 28, 27, 28 }),
                    new("ast", "AST Levels", "24", "U/L", TrendDirection.Down, -8, 10, 40,
                        new List<double> { 28, 27, 26, 26, 25, 24, 24 }),
                    new("detox", "Detox Score", "82", "pts", TrendDirection.Up, 6, 0, 100,
                        new List<double> { 76, 77, 78, 79, 80, 81, 82 }),
                    new("meta", "Metabolic Rate", "1650", "kcal", TrendDirection.Stable, 0, 1400, 2000,
                        new List<double> { 1640, 1655, 1648, 1652, 1645, 1658, 1650 })
                },
                Insights: new List<Insight>
                {
                    new("li1", "Liver enzyme levels are within healthy ranges. Great job on dietary choices!", InsightType.Positive),
                    new("li2", "Hydration is supporting your detoxification processes well.", InsightType.Neutral),
                    new("li3", "Consider adding more leafy greens to further support liver function.", InsightType.Suggestion)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 83, 65),
                    new("Tue", 84, 68),
                    new("Wed", 84, 68),
                    new("Thu", 85, 72),
                    new("Fri", 85, 72),
                    new("Sat", 86, 75),
                    new("Sun", 86, 75)
                }
            ),
            new Organ(
                Id: "kidneys",
                Name: "Kidneys",
                Icon: "\uE9DB",
                AccentColorKey: "KidneysAccentBrush",
                HealthScore: 89,
                Status: "Good",
                Metrics: new List<HealthMetric>
                {
                    new("gfr", "GFR", "105", "mL/min", TrendDirection.Stable, 0, 90, 120,
                        new List<double> { 104, 105, 103, 106, 104, 105, 105 }),
                    new("hydration", "Hydration", "92", "%", TrendDirection.Up, 5, 70, 100,
                        new List<double> { 85, 87, 88, 89, 90, 91, 92 }),
                    new("creat", "Creatinine", "0.95", "mg/dL", TrendDirection.Stable, 0, 0, 2,
                        new List<double> { 0.96, 0.95, 0.97, 0.94, 0.96, 0.95, 0.95 }),
                    new("elec", "Electrolytes", "94", "pts", TrendDirection.Up, 3, 80, 100,
                        new List<double> { 90, 91, 91, 92, 93, 93, 94 })
                },
                Insights: new List<Insight>
                {
                    new("k1", "Kidney function is strong. Your improved hydration is making a positive impact.", InsightType.Positive),
                    new("k2", "Electrolyte balance is excellent after your recent dietary adjustments.", InsightType.Positive),
                    new("k3", "Keep up the water intake, especially after physical activity.", InsightType.Suggestion)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 86, 70),
                    new("Tue", 87, 72),
                    new("Wed", 87, 72),
                    new("Thu", 88, 78),
                    new("Fri", 88, 78),
                    new("Sat", 89, 82),
                    new("Sun", 89, 82)
                }
            ),
            new Organ(
                Id: "pancreas",
                Name: "Pancreas",
                Icon: "\uE9DC",
                AccentColorKey: "PancreasAccentBrush",
                HealthScore: 87,
                Status: "Good",
                Metrics: new List<HealthMetric>
                {
                    new("gluc", "Blood Glucose", "92", "mg/dL", TrendDirection.Stable, 0, 70, 100,
                        new List<double> { 95, 93, 94, 92, 93, 91, 92 }),
                    new("insulin", "Insulin Index", "88", "pts", TrendDirection.Up, 4, 70, 100,
                        new List<double> { 83, 84, 85, 86, 87, 87, 88 }),
                    new("hba1c", "HbA1c", "5.2", "%", TrendDirection.Stable, 0, 4, 6,
                        new List<double> { 5.3, 5.3, 5.2, 5.3, 5.2, 5.2, 5.2 }),
                    new("glyc", "Glycemic Var.", "18", "%", TrendDirection.Down, -10, 10, 25,
                        new List<double> { 24, 23, 22, 21, 20, 19, 18 })
                },
                Insights: new List<Insight>
                {
                    new("p1", "Blood sugar control is excellent. Your consistent meal timing is paying off.", InsightType.Positive),
                    new("p2", "Glycemic variability has decreased, indicating stable energy throughout the day.", InsightType.Positive),
                    new("p3", "Consider a post-meal walk to maintain optimal glucose levels.", InsightType.Suggestion)
                },
                WeeklyTrend: new List<TrendDataPoint>
                {
                    new("Mon", 84, 68),
                    new("Tue", 85, 70),
                    new("Wed", 85, 70),
                    new("Thu", 86, 74),
                    new("Fri", 86, 74),
                    new("Sat", 87, 78),
                    new("Sun", 87, 78)
                }
            )
        };
    }
}

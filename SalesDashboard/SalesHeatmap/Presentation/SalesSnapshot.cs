namespace SalesHeatmap.Presentation;

public sealed record SalesSnapshot(
    double Monthly,
    double MonthlyChange,
    double Yearly,
    double YearlyChange,
    double LosAngeles,
    double LosAngelesChange,
    double NewYork,
    double NewYorkChange,
    double Canada,
    double CanadaChange)
{
    public static SalesSnapshot Initial { get; } = new(
        Monthly: 312_134, MonthlyChange: 12.4,
        Yearly: 3_745_608, YearlyChange: 8.2,
        LosAngeles: 98_420, LosAngelesChange: 15.2,
        NewYork: 87_650, NewYorkChange: -3.8,
        Canada: 65_230, CanadaChange: 22.1);

    public SalesSnapshot NextRandom(Random random) => new(
        Monthly: Monthly * (1 + (random.NextDouble() - 0.5) * 0.10),
        MonthlyChange: (random.NextDouble() - 0.3) * 30,
        Yearly: Yearly * (1 + (random.NextDouble() - 0.5) * 0.05),
        YearlyChange: (random.NextDouble() - 0.3) * 20,
        LosAngeles: LosAngeles * (1 + (random.NextDouble() - 0.5) * 0.15),
        LosAngelesChange: (random.NextDouble() - 0.3) * 40,
        NewYork: NewYork * (1 + (random.NextDouble() - 0.5) * 0.15),
        NewYorkChange: (random.NextDouble() - 0.4) * 30,
        Canada: Canada * (1 + (random.NextDouble() - 0.5) * 0.15),
        CanadaChange: (random.NextDouble() - 0.2) * 35);
}

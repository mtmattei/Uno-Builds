namespace test.Models;

public record Entity(string Name);

public record ThermostatState
{
    public double CurrentTemperature { get; init; } = 21.8;
    public double TargetTemperature { get; init; } = 24.0;
    public double MaxTemperature { get; init; } = 30.0;
    public double MinTemperature { get; init; } = 16.0;
    public string Mode { get; init; } = "Heating";
    public bool IsActive { get; init; } = true;
}

public record MetricCard
{
    public string Icon { get; init; } = string.Empty;
    public string IconColor { get; init; } = "#00D9FF";
    public string Value { get; init; } = "0";
    public string Unit { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

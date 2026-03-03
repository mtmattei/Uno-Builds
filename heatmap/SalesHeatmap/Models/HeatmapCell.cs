using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SalesHeatmap.Models;

public partial class HeatmapCell : ObservableObject
{
    [ObservableProperty]
    private double _intensity;

    [ObservableProperty]
    private double _salesValue;

    public int Row { get; }
    public int Column { get; }
    public int WeekNumber { get; }
    public string Id => $"{Row}-{Column}";

    public string Tooltip => $"Week {WeekNumber}: ${SalesValue:N0}";

    public HeatmapCell(int row, int column, int weekNumber, double intensity, double salesValue)
    {
        Row = row;
        Column = column;
        WeekNumber = weekNumber;
        _intensity = intensity;
        _salesValue = salesValue;
    }

    partial void OnSalesValueChanged(double value)
    {
        OnPropertyChanged(nameof(Tooltip));
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using Windows.UI;

namespace SalesHeatmap.Models;

public partial class CityData : ObservableObject
{
    public string Name { get; }
    public Color DotColor { get; }

    [ObservableProperty]
    private double _value;

    [ObservableProperty]
    private double _displayValue;

    [ObservableProperty]
    private double _change;

    public CityData(string name, double value, Color dotColor, double change = 0)
    {
        Name = name;
        _value = value;
        _displayValue = value;
        DotColor = dotColor;
        _change = change;
    }
}

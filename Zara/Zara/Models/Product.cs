using CommunityToolkit.Mvvm.ComponentModel;

namespace Zara.Models;

public partial class Product : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private string _currency = "GBP";

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private string _colorIndicator = "#000000";

    [ObservableProperty]
    private int _variantCount;

    [ObservableProperty]
    private string _fit = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private bool _isBookmarked;

    public string FormattedPrice => $"{Price:F2} {Currency}";
}

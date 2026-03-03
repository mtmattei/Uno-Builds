using CommunityToolkit.Mvvm.ComponentModel;

namespace Zara.Models;

public partial class Category : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

using Microsoft.UI.Xaml.Media;
using FormaEspresso.ViewModels;
using FormaEspresso.Models;
using FormaEspresso.Controls;

namespace FormaEspresso;

public sealed partial class MainPage : Page
{
    private static readonly SolidColorBrush Stone900 = new(Windows.UI.Color.FromArgb(255, 28, 25, 23));
    private static readonly SolidColorBrush Stone100 = new(Windows.UI.Color.FromArgb(255, 245, 245, 244));
    private static readonly SolidColorBrush Stone400 = new(Windows.UI.Color.FromArgb(255, 168, 162, 158));
    private static readonly SolidColorBrush White = new(Windows.UI.Color.FromArgb(255, 255, 255, 255));

    public MainViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.DataContext = ViewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedItem))
        {
            UpdateCardSelectionStates();
        }
    }

    private void UpdateCardSelectionStates()
    {
        for (int i = 0; i < ViewModel.MenuItems.Length; i++)
        {
            var container = MenuRepeater.TryGetElement(i);
            if (container is Button button && button.Content is EspressoCard card)
            {
                card.IsSelected = ViewModel.SelectedItem?.Id == card.Item?.Id;
            }
        }
    }

    private void OnEspressoCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is EspressoItem item)
        {
            ViewModel.SelectItemCommand.Execute(item);
        }
    }

    public static Brush GetQuantityBackground(int currentQty, int buttonQty)
    {
        return currentQty == buttonQty ? Stone900 : Stone100;
    }

    public static Brush GetQuantityForeground(int currentQty, int buttonQty)
    {
        return currentQty == buttonQty ? White : Stone400;
    }

    public static double GetProgressWidth(double progress)
    {
        return (progress / 100.0) * 192;
    }
}

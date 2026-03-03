using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Zara.Views.Controls;

public sealed partial class HeaderControl : UserControl
{
    public event EventHandler? MenuClicked;
    public event EventHandler? FiltersClicked;
    public event EventHandler? SearchClicked;
    public event EventHandler? CartClicked;

    public HeaderControl()
    {
        this.InitializeComponent();
    }

    private void OnMenuClick(object sender, RoutedEventArgs e)
    {
        MenuClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnFiltersClick(object sender, RoutedEventArgs e)
    {
        FiltersClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnSearchClick(object sender, RoutedEventArgs e)
    {
        SearchClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnCartClick(object sender, RoutedEventArgs e)
    {
        CartClicked?.Invoke(this, EventArgs.Empty);
    }
}

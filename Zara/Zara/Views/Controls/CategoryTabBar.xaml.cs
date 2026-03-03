using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Zara.Models;

namespace Zara.Views.Controls;

public sealed partial class CategoryTabBar : UserControl
{
    public ObservableCollection<Category> Categories
    {
        get => (ObservableCollection<Category>)GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    public static readonly DependencyProperty CategoriesProperty =
        DependencyProperty.Register(
            nameof(Categories),
            typeof(ObservableCollection<Category>),
            typeof(CategoryTabBar),
            new PropertyMetadata(null));

    public event EventHandler<Category>? CategorySelected;

    public CategoryTabBar()
    {
        this.InitializeComponent();
    }

    private void OnCategoryClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Category category)
        {
            CategorySelected?.Invoke(this, category);
        }
    }
}

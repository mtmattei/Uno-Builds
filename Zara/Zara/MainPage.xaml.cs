using Zara.Views;

namespace Zara;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += MainPage_Loaded;
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        RootFrame.Navigate(typeof(ProductCatalogPage));
    }
}

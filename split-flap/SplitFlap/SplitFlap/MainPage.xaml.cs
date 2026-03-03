using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SplitFlap;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void OnCardButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string character)
        {
            SingleCard.Character = character;
        }
    }
}

using AdaptiveInputDemo.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AdaptiveInputDemo.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private void OnInputSubmitted(object? sender, InputSubmittedEventArgs e)
    {
        // Input submitted - could show a notification or process the value
    }

    private void OnExampleClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string example)
        {
            MainInput.Value = example;
            MainInput.Focus();
        }
    }
}

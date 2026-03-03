using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = (MainViewModel)DataContext;
        this.InitializeComponent();
    }
}

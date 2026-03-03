using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class HockeyPage : Page
{
    public HockeyViewModel ViewModel { get; }

    public HockeyPage()
    {
        ViewModel = new HockeyViewModel();
        this.InitializeComponent();
    }
}

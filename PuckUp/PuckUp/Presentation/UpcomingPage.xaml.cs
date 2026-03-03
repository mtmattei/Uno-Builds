using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class UpcomingPage : Page
{
    public UpcomingViewModel ViewModel { get; }

    public UpcomingPage()
    {
        ViewModel = new UpcomingViewModel();
        this.InitializeComponent();
    }
}

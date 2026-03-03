using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class TeamsPage : Page
{
    public TeamsViewModel ViewModel { get; }

    public TeamsPage()
    {
        ViewModel = new TeamsViewModel();
        this.InitializeComponent();
    }
}

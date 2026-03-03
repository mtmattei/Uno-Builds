using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class FindPlayersView : UserControl
{
    public FindPlayersViewModel ViewModel { get; }
    
    public FindPlayersView()
    {
        this.DataContext = ViewModel = new FindPlayersViewModel();
        this.InitializeComponent();
    }
}

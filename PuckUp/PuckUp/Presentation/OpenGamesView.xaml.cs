using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class OpenGamesView : UserControl
{
    public OpenGamesViewModel ViewModel { get; }
    
    public OpenGamesView()
    {
        this.DataContext = ViewModel = new OpenGamesViewModel();
        this.InitializeComponent();
    }
}

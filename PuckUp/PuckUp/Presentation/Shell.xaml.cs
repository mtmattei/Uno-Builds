using Microsoft.UI.Xaml.Controls;
using PuckUp.ViewModels;

namespace PuckUp.Presentation;

public sealed partial class Shell : UserControl
{
    public ShellViewModel ViewModel { get; }

    public Shell()
    {
        ViewModel = (ShellViewModel)DataContext;
        this.InitializeComponent();
    }
}

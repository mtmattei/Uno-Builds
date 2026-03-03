namespace Pens.Presentation;

public sealed partial class PlayerPickerPage : Page
{
    public PlayerPickerViewModel ViewModel { get; }

    public PlayerPickerPage(PlayerPickerViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}

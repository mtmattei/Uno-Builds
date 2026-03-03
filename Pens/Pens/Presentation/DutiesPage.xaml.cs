using Pens.Models;

namespace Pens.Presentation;

public sealed partial class DutiesPage : Page
{
    public DutiesPage()
    {
        this.InitializeComponent();
    }

    private void OnPlayerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && 
            comboBox.DataContext is Duty duty && 
            comboBox.SelectedValue is int playerId &&
            DataContext is DutiesViewModel viewModel)
        {
            var dutyTypeStr = duty.Type.ToString().ToLowerInvariant();
            _ = viewModel.AssignPlayerToDutyCommand.ExecuteAsync(new object[] { dutyTypeStr, playerId });
        }
    }
}

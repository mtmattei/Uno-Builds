namespace GridForm.Controls;

public sealed partial class CommandPaletteDialog : UserControl
{
	public event EventHandler<string>? ResultSelected;

	public CommandPaletteDialog()
	{
		this.InitializeComponent();
	}

	public void Show()
	{
		Overlay.Visibility = Visibility.Visible;
		SearchInput.Focus(FocusState.Programmatic);
	}

	public void Hide()
	{
		Overlay.Visibility = Visibility.Collapsed;
		SearchInput.Text = string.Empty;
	}

	public bool IsOpen => Overlay.Visibility == Visibility.Visible;

	private void OnResultClick(object sender, RoutedEventArgs e)
	{
		Hide();
	}
}

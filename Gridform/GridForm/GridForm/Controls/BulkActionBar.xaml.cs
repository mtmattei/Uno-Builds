using System.Windows.Input;

namespace GridForm.Controls;

public sealed partial class BulkActionBar : UserControl
{
	public static readonly DependencyProperty SelectedCountProperty =
		DependencyProperty.Register(nameof(SelectedCount), typeof(int), typeof(BulkActionBar),
			new PropertyMetadata(0, OnCountChanged));

	public static readonly DependencyProperty ApproveCommandProperty =
		DependencyProperty.Register(nameof(ApproveCommand), typeof(ICommand), typeof(BulkActionBar), new PropertyMetadata(null));

	public static readonly DependencyProperty ExportCommandProperty =
		DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(BulkActionBar), new PropertyMetadata(null));

	public static readonly DependencyProperty ClearCommandProperty =
		DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(BulkActionBar), new PropertyMetadata(null));

	public int SelectedCount { get => (int)GetValue(SelectedCountProperty); set => SetValue(SelectedCountProperty, value); }
	public ICommand? ApproveCommand { get => (ICommand?)GetValue(ApproveCommandProperty); set => SetValue(ApproveCommandProperty, value); }
	public ICommand? ExportCommand { get => (ICommand?)GetValue(ExportCommandProperty); set => SetValue(ExportCommandProperty, value); }
	public ICommand? ClearCommand { get => (ICommand?)GetValue(ClearCommandProperty); set => SetValue(ClearCommandProperty, value); }

	public new Visibility IsVisible => SelectedCount > 0 ? Visibility.Visible : Visibility.Collapsed;

	public BulkActionBar()
	{
		this.InitializeComponent();
	}

	private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is BulkActionBar bar)
			bar.Bindings.Update();
	}
}

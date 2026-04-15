namespace GridForm.Controls;

public sealed partial class AiChip : UserControl
{
	public static readonly DependencyProperty NoteProperty =
		DependencyProperty.Register(nameof(Note), typeof(string), typeof(AiChip), new PropertyMetadata(null));

	public string? Note
	{
		get => (string?)GetValue(NoteProperty);
		set => SetValue(NoteProperty, value);
	}

	public Visibility HasNote => string.IsNullOrEmpty(Note) ? Visibility.Collapsed : Visibility.Visible;

	public AiChip()
	{
		this.InitializeComponent();
	}
}

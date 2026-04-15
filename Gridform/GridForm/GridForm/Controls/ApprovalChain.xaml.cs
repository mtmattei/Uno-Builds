namespace GridForm.Controls;

public sealed partial class ApprovalChain : UserControl
{
	public static readonly DependencyProperty StepsProperty =
		DependencyProperty.Register(nameof(Steps), typeof(ImmutableList<ApprovalStep>), typeof(ApprovalChain),
			new PropertyMetadata(null));

	public ImmutableList<ApprovalStep>? Steps
	{
		get => (ImmutableList<ApprovalStep>?)GetValue(StepsProperty);
		set => SetValue(StepsProperty, value);
	}

	public ApprovalChain()
	{
		this.InitializeComponent();
	}
}

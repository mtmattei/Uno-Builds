namespace GridForm.Controls;

public sealed partial class PipelineBar : UserControl
{
	public static readonly DependencyProperty PendingCountProperty =
		DependencyProperty.Register(nameof(PendingCount), typeof(int), typeof(PipelineBar), new PropertyMetadata(0, OnCountChanged));
	public static readonly DependencyProperty ReviewCountProperty =
		DependencyProperty.Register(nameof(ReviewCount), typeof(int), typeof(PipelineBar), new PropertyMetadata(0, OnCountChanged));
	public static readonly DependencyProperty ApprovedCountProperty =
		DependencyProperty.Register(nameof(ApprovedCount), typeof(int), typeof(PipelineBar), new PropertyMetadata(0, OnCountChanged));
	public static readonly DependencyProperty FlaggedCountProperty =
		DependencyProperty.Register(nameof(FlaggedCount), typeof(int), typeof(PipelineBar), new PropertyMetadata(0, OnCountChanged));

	public int PendingCount { get => (int)GetValue(PendingCountProperty); set => SetValue(PendingCountProperty, value); }
	public int ReviewCount { get => (int)GetValue(ReviewCountProperty); set => SetValue(ReviewCountProperty, value); }
	public int ApprovedCount { get => (int)GetValue(ApprovedCountProperty); set => SetValue(ApprovedCountProperty, value); }
	public int FlaggedCount { get => (int)GetValue(FlaggedCountProperty); set => SetValue(FlaggedCountProperty, value); }

	private int Total => Math.Max(1, PendingCount + ReviewCount + ApprovedCount + FlaggedCount);
	public GridLength PendingWidth => new(PendingCount, GridUnitType.Star);
	public GridLength ReviewWidth => new(ReviewCount, GridUnitType.Star);
	public GridLength ApprovedWidth => new(ApprovedCount, GridUnitType.Star);
	public GridLength FlaggedWidth => new(FlaggedCount, GridUnitType.Star);

	public PipelineBar()
	{
		this.InitializeComponent();
	}

	private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is PipelineBar bar)
			bar.Bindings.Update();
	}
}

namespace GridForm.Controls;

public sealed partial class SlaBadge : UserControl
{
	public static readonly DependencyProperty DeadlineProperty =
		DependencyProperty.Register(nameof(Deadline), typeof(DateTimeOffset?), typeof(SlaBadge),
			new PropertyMetadata(null, OnDeadlineChanged));

	public DateTimeOffset? Deadline
	{
		get => (DateTimeOffset?)GetValue(DeadlineProperty);
		set => SetValue(DeadlineProperty, value);
	}

	public string SlaText
	{
		get
		{
			if (Deadline is not { } d) return "—";
			var remaining = d - DateTimeOffset.Now;
			if (remaining.TotalMinutes < 0) return "OVERDUE";
			if (remaining.TotalHours < 1) return $"{(int)remaining.TotalMinutes}m";
			if (remaining.TotalDays < 1) return $"{(int)remaining.TotalHours}h";
			return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
		}
	}

	public SlaBadge()
	{
		this.InitializeComponent();
	}

	private static void OnDeadlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is SlaBadge badge)
			badge.Bindings.Update();
	}
}

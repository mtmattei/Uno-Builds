namespace GridForm.Controls;

public sealed partial class StatusBadge : UserControl
{
	public static readonly DependencyProperty StatusProperty =
		DependencyProperty.Register(nameof(Status), typeof(OrderStatus), typeof(StatusBadge),
			new PropertyMetadata(OrderStatus.Pending, OnStatusChanged));

	public OrderStatus Status
	{
		get => (OrderStatus)GetValue(StatusProperty);
		set => SetValue(StatusProperty, value);
	}

	public string StatusText => Status switch
	{
		OrderStatus.Pending => "PENDING",
		OrderStatus.InReview => "IN REVIEW",
		OrderStatus.Approved => "APPROVED",
		OrderStatus.Flagged => "FLAGGED",
		_ => "UNKNOWN"
	};

	public StatusBadge()
	{
		this.InitializeComponent();
	}

	private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StatusBadge badge)
			badge.Bindings.Update();
	}
}

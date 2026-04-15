namespace GridForm.Controls;

public sealed partial class RiskBadge : UserControl
{
	public static readonly DependencyProperty RiskProperty =
		DependencyProperty.Register(nameof(Risk), typeof(RiskLevel), typeof(RiskBadge),
			new PropertyMetadata(RiskLevel.Low, OnRiskChanged));

	public RiskLevel Risk
	{
		get => (RiskLevel)GetValue(RiskProperty);
		set => SetValue(RiskProperty, value);
	}

	public string RiskText => Risk switch
	{
		RiskLevel.Low => "LOW",
		RiskLevel.Medium => "MEDIUM",
		RiskLevel.High => "HIGH",
		_ => ""
	};

	public RiskBadge()
	{
		this.InitializeComponent();
	}

	private static void OnRiskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is RiskBadge badge)
			badge.Bindings.Update();
	}
}

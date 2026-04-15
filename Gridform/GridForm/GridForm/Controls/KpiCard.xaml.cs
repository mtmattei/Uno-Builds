using Microsoft.UI.Xaml.Media;

namespace GridForm.Controls;

public sealed partial class KpiCard : UserControl
{
	public static readonly DependencyProperty LabelProperty =
		DependencyProperty.Register(nameof(Label), typeof(string), typeof(KpiCard), new PropertyMetadata(""));

	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(nameof(Value), typeof(string), typeof(KpiCard), new PropertyMetadata(""));

	public static readonly DependencyProperty DeltaProperty =
		DependencyProperty.Register(nameof(Delta), typeof(string), typeof(KpiCard), new PropertyMetadata(""));

	public static readonly DependencyProperty SubtitleProperty =
		DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(KpiCard), new PropertyMetadata(""));

	public static readonly DependencyProperty AccentBrushProperty =
		DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(KpiCard), new PropertyMetadata(null));

	public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
	public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
	public string Delta { get => (string)GetValue(DeltaProperty); set => SetValue(DeltaProperty, value); }
	public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
	public Brush AccentBrush { get => (Brush)GetValue(AccentBrushProperty); set => SetValue(AccentBrushProperty, value); }

	public bool HasDelta => !string.IsNullOrEmpty(Delta);

	public KpiCard()
	{
		this.InitializeComponent();
	}
}

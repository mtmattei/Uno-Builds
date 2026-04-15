namespace GridForm.Controls;

public sealed partial class LayerScrubber : UserControl
{
	public static readonly DependencyProperty CurrentLayerProperty =
		DependencyProperty.Register(nameof(CurrentLayer), typeof(int), typeof(LayerScrubber),
			new PropertyMetadata(0, OnLayerChanged));

	public static readonly DependencyProperty MaxLayerProperty =
		DependencyProperty.Register(nameof(MaxLayer), typeof(int), typeof(LayerScrubber),
			new PropertyMetadata(WarehouseState.MaxLayers - 1));

	public int CurrentLayer { get => (int)GetValue(CurrentLayerProperty); set => SetValue(CurrentLayerProperty, value); }
	public int MaxLayer { get => (int)GetValue(MaxLayerProperty); set => SetValue(MaxLayerProperty, value); }

	public event EventHandler<int>? LayerChanged;

	public string LayerLabel => $"H{CurrentLayer}";

	public ImmutableList<bool> LayerIndicators
	{
		get
		{
			var list = ImmutableList.CreateBuilder<bool>();
			for (var i = MaxLayer; i >= 0; i--)
				list.Add(i == CurrentLayer);
			return list.ToImmutable();
		}
	}

	public LayerScrubber()
	{
		this.InitializeComponent();
	}

	private void OnLayerUp(object sender, RoutedEventArgs e)
	{
		if (CurrentLayer < MaxLayer)
		{
			CurrentLayer++;
			LayerChanged?.Invoke(this, CurrentLayer);
		}
	}

	private void OnLayerDown(object sender, RoutedEventArgs e)
	{
		if (CurrentLayer > 0)
		{
			CurrentLayer--;
			LayerChanged?.Invoke(this, CurrentLayer);
		}
	}

	private static void OnLayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is LayerScrubber s)
			s.Bindings.Update();
	}
}

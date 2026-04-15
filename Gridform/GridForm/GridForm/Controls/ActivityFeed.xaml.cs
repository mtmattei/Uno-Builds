namespace GridForm.Controls;

public sealed partial class ActivityFeed : UserControl
{
	public static readonly DependencyProperty EventsProperty =
		DependencyProperty.Register(nameof(Events), typeof(ImmutableList<ActivityEvent>), typeof(ActivityFeed),
			new PropertyMetadata(null));

	public ImmutableList<ActivityEvent>? Events
	{
		get => (ImmutableList<ActivityEvent>?)GetValue(EventsProperty);
		set => SetValue(EventsProperty, value);
	}

	public ActivityFeed()
	{
		this.InitializeComponent();
	}
}

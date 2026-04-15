using Microsoft.UI.Xaml.Input;
using Windows.UI;

namespace CameraCaptureUISample.Controls;

public sealed partial class AnnotationBadge : UserControl
{
	public static readonly DependencyProperty LabelProperty =
		DependencyProperty.Register(nameof(Label), typeof(string), typeof(AnnotationBadge),
			new PropertyMetadata(string.Empty, OnLabelChanged));

	public static readonly DependencyProperty IconGlyphProperty =
		DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(AnnotationBadge),
			new PropertyMetadata(string.Empty, OnIconGlyphChanged));

	public static readonly DependencyProperty BadgeColorProperty =
		DependencyProperty.Register(nameof(BadgeColor), typeof(Color), typeof(AnnotationBadge),
			new PropertyMetadata(default(Color), OnBadgeColorChanged));

	public static readonly DependencyProperty AnnotationKeyProperty =
		DependencyProperty.Register(nameof(AnnotationKey), typeof(string), typeof(AnnotationBadge),
			new PropertyMetadata(string.Empty));

	public string Label
	{
		get => (string)GetValue(LabelProperty);
		set => SetValue(LabelProperty, value);
	}

	public string IconGlyph
	{
		get => (string)GetValue(IconGlyphProperty);
		set => SetValue(IconGlyphProperty, value);
	}

	public Color BadgeColor
	{
		get => (Color)GetValue(BadgeColorProperty);
		set => SetValue(BadgeColorProperty, value);
	}

	public string AnnotationKey
	{
		get => (string)GetValue(AnnotationKeyProperty);
		set => SetValue(AnnotationKeyProperty, value);
	}

	public event EventHandler<string>? BadgeTapped;

	public AnnotationBadge()
	{
		this.InitializeComponent();
		this.Tapped += OnBadgeTapped;
	}

	/// <summary>
	/// Plays the entrance animation with an optional stagger delay
	/// for cascading badge reveals (0ms, 100ms, 200ms per brief §4.5).
	/// Also starts the pulse ring animation.
	/// </summary>
	public void PlayEntrance(TimeSpan staggerDelay)
	{
		// Reset to pre-animation state
		Root.Opacity = 0;
		RootTranslate.Y = 8;

		// Stop any running animations
		EntranceAnimation.Stop();
		PulseAnimation.Stop();

		// Set the staggered BeginTime
		EntranceAnimation.BeginTime = staggerDelay;

		EntranceAnimation.Begin();
		PulseAnimation.Begin();
	}

	/// <summary>
	/// Stops animations and resets to hidden state (for when badge collapses).
	/// </summary>
	public void StopAnimations()
	{
		EntranceAnimation.Stop();
		PulseAnimation.Stop();
		Root.Opacity = 0;
		RootTranslate.Y = 8;
	}

	private void OnBadgeTapped(object sender, TappedRoutedEventArgs e)
	{
		BadgeTapped?.Invoke(this, AnnotationKey);
	}

	private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is AnnotationBadge badge)
			badge.BadgeLabel.Text = e.NewValue as string ?? string.Empty;
	}

	private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is AnnotationBadge badge)
			badge.BadgeIcon.Glyph = e.NewValue as string ?? string.Empty;
	}

	private static void OnBadgeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is AnnotationBadge badge && e.NewValue is Color color)
		{
			var brush = new SolidColorBrush(color);
			badge.BadgePill.Background = brush;
			badge.PulseRing.BorderBrush = brush;
		}
	}
}

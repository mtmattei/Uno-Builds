using SkiaSharp;
using SkiaSharp.Views.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace test.Presentation;

public class ChartRenderer : UserControl, IDisposable
{
	private bool _disposed = false;
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<double>), typeof(ChartRenderer),
			new PropertyMetadata(null, OnDataPropertyChanged));

	private SKXamlCanvas _canvas;

	// Cached paint objects
	private readonly SKPaint _fillPaint;
	private readonly SKPaint _linePaint;
	private readonly SKPaint _monthLabelPaint;
	private readonly SKPaint _yAxisLabelPaint;
	private readonly SKPaint _gridLinePaint;

	// Cached fonts
	private readonly SKFont _monthLabelFont;
	private readonly SKFont _yAxisLabelFont;

	// Cached colors for gradient
	private readonly SKColor[] _gradientColors;

	// Cached calculation results
	private SKPoint[]? _cachedPoints;
	private double _cachedMaxValue;
	private bool _isDirty = true;

	// Animation
	private float _animationProgress = 0f;
	private bool _isAnimating = false;
	private DateTime _animationStartTime;
	private readonly TimeSpan _animationDuration = TimeSpan.FromMilliseconds(2500);
	private DispatcherTimer? _animationTimer;

	// Month labels (12 months)
	private readonly string[] _monthLabels = new[] { "J", "F", "M", "A", "M", "J", "J", "A", "S", "O", "N", "D" };

	public ObservableCollection<double> Data
	{
		get => (ObservableCollection<double>)GetValue(DataProperty);
		set => SetValue(DataProperty, value);
	}

	public ChartRenderer()
	{
		_canvas = new SKXamlCanvas();
		_canvas.PaintSurface += OnPaintSurface;
		_canvas.PointerEntered += OnPointerEntered;
		_canvas.PointerExited += OnPointerExited;
		Content = _canvas;
		MinHeight = 180;
		HorizontalAlignment = HorizontalAlignment.Stretch;

		// Initialize cached fonts
		_monthLabelFont = new SKFont
		{
			Size = 10
		};

		_yAxisLabelFont = new SKFont
		{
			Size = 10
		};

		// Initialize cached paint objects
		_fillPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			IsAntialias = true
		};

		_linePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColor.Parse("#4ADE80"),
			StrokeWidth = 3,
			IsAntialias = true
		};

		_monthLabelPaint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			IsAntialias = true
		};

		_yAxisLabelPaint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			IsAntialias = true
		};

		_gridLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColor.Parse("#2A2A3E").WithAlpha(80),
			StrokeWidth = 1,
			IsAntialias = true
		};

		// Initialize gradient colors
		_gradientColors = new[]
		{
			SKColor.Parse("#4ADE80").WithAlpha(100),
			SKColor.Parse("#4ADE80").WithAlpha(0)
		};

		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		StartAnimation();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		StopAnimation();
	}

	private void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		StartAnimation();
	}

	private void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		// Animation completes on its own
	}

	private void StartAnimation()
	{
		// Stop any existing animation
		StopAnimation();

		_isAnimating = true;
		_animationProgress = 0f;
		_animationStartTime = DateTime.Now;

		_animationTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(16)
		};

		_animationTimer.Tick += OnAnimationTick;
		_animationTimer.Start();
	}

	private void OnAnimationTick(object? sender, object e)
	{
		var elapsed = DateTime.Now - _animationStartTime;
		var linearProgress = Math.Min(1f, (float)(elapsed.TotalMilliseconds / _animationDuration.TotalMilliseconds));

		// Ease in-out cubic easing for more dramatic effect
		if (linearProgress < 0.5f)
		{
			_animationProgress = 4f * linearProgress * linearProgress * linearProgress;
		}
		else
		{
			var p = 2f * linearProgress - 2f;
			_animationProgress = 1f + (p * p * p) / 2f;
		}

		_canvas.Invalidate();

		if (linearProgress >= 1f)
		{
			_isAnimating = false;
			StopAnimation();
		}
	}

	private void StopAnimation()
	{
		if (_animationTimer != null)
		{
			_animationTimer.Stop();
			_animationTimer.Tick -= OnAnimationTick;
			_animationTimer = null;
		}
	}

	private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ChartRenderer renderer)
		{
			// Unsubscribe from old collection
			if (e.OldValue is ObservableCollection<double> oldCollection)
			{
				oldCollection.CollectionChanged -= renderer.OnDataCollectionChanged;
			}

			// Subscribe to new collection
			if (e.NewValue is ObservableCollection<double> newCollection)
			{
				newCollection.CollectionChanged += renderer.OnDataCollectionChanged;
			}

			renderer.MarkDirty();
			renderer._canvas.Invalidate();
		}
	}

	private void OnDataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		MarkDirty();
		_canvas.Invalidate();
	}

	private void MarkDirty()
	{
		_isDirty = true;
		_cachedPoints = null;
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		if (Data == null || Data.Count < 2)
			return;

		var info = e.Info;
		var leftPadding = 40f;
		var rightPadding = 20f;
		var topPadding = 20f;
		var bottomPadding = 30f;
		var width = info.Width - leftPadding - rightPadding;
		var height = info.Height - topPadding - bottomPadding;

		// Recalculate points only if data changed
		if (_isDirty || _cachedPoints == null)
		{
			RecalculatePoints(leftPadding, width, height, topPadding);
			_isDirty = false;
		}

		if (_cachedPoints == null || _cachedPoints.Length < 2)
			return;

		// Draw Y-axis grid lines and labels
		DrawYAxis(canvas, leftPadding, topPadding, height, width);

		// Apply animation progress
		var animatedPoints = _isAnimating ? ApplyAnimation(_cachedPoints, height, topPadding) : _cachedPoints;

		// Draw filled area
		DrawFilledArea(canvas, animatedPoints, leftPadding, height, topPadding);

		// Draw line
		DrawLine(canvas, animatedPoints);

		// Draw month labels
		DrawMonthLabels(canvas, leftPadding, width, info.Height - 10);
	}

	private void DrawYAxis(SKCanvas canvas, float leftPadding, float topPadding, float height, float width)
	{
		// Calculate nice round numbers for Y-axis based on max value
		var maxValue = _cachedMaxValue;
		var roundedMax = Math.Ceiling(maxValue / 10) * 10;
		var step = roundedMax / 4; // 5 levels (0, 25%, 50%, 75%, 100%)

		for (int i = 0; i <= 4; i++)
		{
			var value = step * i;
			var normalizedY = i / 4f;
			var y = topPadding + height - (normalizedY * height);

			// Draw grid line
			canvas.DrawLine(leftPadding, y, leftPadding + width, y, _gridLinePaint);

			// Draw Y-axis label
			var label = $"{value:F0}";
			canvas.DrawText(label, leftPadding - 5, y + 3, SKTextAlign.Right, _yAxisLabelFont, _yAxisLabelPaint);
		}

		// Draw "kWh" unit label at top (with more spacing from values)
		canvas.DrawText("kWh", leftPadding - 5, topPadding - 10, SKTextAlign.Right, _yAxisLabelFont, _yAxisLabelPaint);
	}

	private SKPoint[] ApplyAnimation(SKPoint[] points, float height, float topPadding)
	{
		var animatedPoints = new SKPoint[points.Length];
		var baselineY = topPadding + height;

		for (int i = 0; i < points.Length; i++)
		{
			var targetY = points[i].Y;
			var currentY = baselineY - ((baselineY - targetY) * _animationProgress);
			animatedPoints[i] = new SKPoint(points[i].X, currentY);
		}

		return animatedPoints;
	}

	private void RecalculatePoints(float leftPadding, float width, float height, float topPadding)
	{
		if (Data == null || Data.Count < 2)
		{
			_cachedPoints = null;
			return;
		}

		_cachedPoints = new SKPoint[Data.Count];
		_cachedMaxValue = Data.Max();
		var stepX = width / (Data.Count - 1);

		for (int i = 0; i < Data.Count; i++)
		{
			var x = leftPadding + (i * stepX);
			var normalizedValue = (float)(Data[i] / _cachedMaxValue);
			var y = topPadding + height - (normalizedValue * height);
			_cachedPoints[i] = new SKPoint(x, y);
		}
	}

	private void DrawFilledArea(SKCanvas canvas, SKPoint[] points, float leftPadding, float height, float topPadding)
	{
		using var path = new SKPath();
		path.MoveTo(points[0].X, topPadding + height);
		path.LineTo(points[0].X, points[0].Y);

		for (int i = 1; i < points.Length; i++)
		{
			var prevPoint = points[i - 1];
			var currentPoint = points[i];
			var controlX = (prevPoint.X + currentPoint.X) / 2;

			path.CubicTo(controlX, prevPoint.Y, controlX, currentPoint.Y, currentPoint.X, currentPoint.Y);
		}

		path.LineTo(points[points.Length - 1].X, topPadding + height);
		path.Close();

		using var shader = SKShader.CreateLinearGradient(
			new SKPoint(0, topPadding),
			new SKPoint(0, topPadding + height),
			_gradientColors,
			null,
			SKShaderTileMode.Clamp
		);

		_fillPaint.Shader = shader;
		canvas.DrawPath(path, _fillPaint);
		_fillPaint.Shader = null;
	}

	private void DrawLine(SKCanvas canvas, SKPoint[] points)
	{
		using var path = new SKPath();
		path.MoveTo(points[0]);

		for (int i = 1; i < points.Length; i++)
		{
			var prevPoint = points[i - 1];
			var currentPoint = points[i];
			var controlX = (prevPoint.X + currentPoint.X) / 2;

			path.CubicTo(controlX, prevPoint.Y, controlX, currentPoint.Y, currentPoint.X, currentPoint.Y);
		}

		canvas.DrawPath(path, _linePaint);
	}

	private void DrawMonthLabels(SKCanvas canvas, float leftPadding, float width, float yPosition)
	{
		if (_cachedPoints == null || _cachedPoints.Length != 12)
			return;

		for (int i = 0; i < _monthLabels.Length && i < _cachedPoints.Length; i++)
		{
			var x = _cachedPoints[i].X;
			canvas.DrawText(_monthLabels[i], x, yPosition, SKTextAlign.Center, _monthLabelFont, _monthLabelPaint);
		}
	}

	public new void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Stop and dispose animation timer
			StopAnimation();

			// Unsubscribe from events
			Loaded -= OnLoaded;
			Unloaded -= OnUnloaded;
			_canvas.PaintSurface -= OnPaintSurface;
			_canvas.PointerEntered -= OnPointerEntered;
			_canvas.PointerExited -= OnPointerExited;

			// Unsubscribe from collection changes to prevent memory leaks
			if (Data != null)
			{
				Data.CollectionChanged -= OnDataCollectionChanged;
			}

			// Dispose managed resources
			_fillPaint?.Dispose();
			_linePaint?.Dispose();
			_monthLabelPaint?.Dispose();
			_yAxisLabelPaint?.Dispose();
			_gridLinePaint?.Dispose();

			_monthLabelFont?.Dispose();
			_yAxisLabelFont?.Dispose();
		}

		_disposed = true;
	}
}

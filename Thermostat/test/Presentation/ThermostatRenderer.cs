using SkiaSharp;
using SkiaSharp.Views.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.Devices.Haptics;

namespace test.Presentation;

public class ThermostatRenderer : UserControl, IDisposable
{
	private bool _disposed = false;
	public static readonly DependencyProperty ProgressProperty =
		DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ThermostatRenderer),
			new PropertyMetadata(0.0, OnPropertyChanged));

	public static readonly DependencyProperty CurrentTemperatureProperty =
		DependencyProperty.Register(nameof(CurrentTemperature), typeof(double), typeof(ThermostatRenderer),
			new PropertyMetadata(21.8, OnPropertyChanged));

	public static readonly DependencyProperty StatusTextProperty =
		DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ThermostatRenderer),
			new PropertyMetadata("Heating to 24°", OnPropertyChanged));

	private SKXamlCanvas _canvas;
	private float _centerX;
	private float _centerY;
	private float _radius;
	private bool _isTracking;

	// Haptic feedback
	private SimpleHapticsController? _hapticsController;
	private SimpleHapticsControllerFeedback? _clickFeedback;
	private double _lastAngleDegrees = 0;

	// Cached paint objects
	private readonly SKPaint _concentricCirclePaint;
	private readonly SKPaint _backgroundArcPaint;
	private readonly SKPaint _progressArcPaint;
	private readonly SKPaint _handlePaint;
	private readonly SKPaint _iconPaint;
	private readonly SKPaint _indoorLabelPaint;
	private readonly SKPaint _temperaturePaint;
	private readonly SKPaint _statusPaint;

	// Cached fonts
	private readonly SKFont _iconFont;
	private readonly SKFont _indoorLabelFont;
	private readonly SKFont _temperatureFont;
	private readonly SKFont _statusFont;

	// Cached typefaces
	private readonly SKTypeface _iconTypeface;
	private readonly SKTypeface _temperatureTypeface;

	// Cached colors for gradient
	private readonly SKColor[] _gradientColors;
	private readonly float[] _gradientPositions;

	public double Progress
	{
		get => (double)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public double CurrentTemperature
	{
		get => (double)GetValue(CurrentTemperatureProperty);
		set => SetValue(CurrentTemperatureProperty, value);
	}

	public string StatusText
	{
		get => (string)GetValue(StatusTextProperty);
		set => SetValue(StatusTextProperty, value);
	}

	public ThermostatRenderer()
	{
		_canvas = new SKXamlCanvas();
		_canvas.PaintSurface += OnPaintSurface;
		_canvas.PointerPressed += OnPointerPressed;
		_canvas.PointerMoved += OnPointerMoved;
		_canvas.PointerReleased += OnPointerReleased;
		Content = _canvas;

		// Initialize cached typefaces
		_iconTypeface = SKTypeface.FromFamilyName("Segoe MDL2 Assets", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
		_temperatureTypeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Light, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

		// Initialize cached fonts
		_iconFont = new SKFont(_iconTypeface, 14);
		_indoorLabelFont = new SKFont(null, 12);
		_temperatureFont = new SKFont(_temperatureTypeface, 58);
		_statusFont = new SKFont(null, 12);

		// Initialize cached paint objects
		_concentricCirclePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1,
			Color = SKColor.Parse("#2A2A3E").WithAlpha(40),
			IsAntialias = true
		};

		_backgroundArcPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 20,
			Color = SKColor.Parse("#2A2A3E"),
			IsAntialias = true,
			StrokeCap = SKStrokeCap.Round
		};

		_progressArcPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 20,
			IsAntialias = true,
			StrokeCap = SKStrokeCap.Round
		};

		_handlePaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.White,
			IsAntialias = true
		};

		_iconPaint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			IsAntialias = true
		};

		_indoorLabelPaint = new SKPaint
		{
			Color = SKColor.Parse("#A0A0A0"),
			IsAntialias = true
		};

		_temperaturePaint = new SKPaint
		{
			Color = SKColors.White,
			IsAntialias = true
		};

		_statusPaint = new SKPaint
		{
			Color = SKColor.Parse("#FF8C42"),
			IsAntialias = true
		};

		// Initialize gradient colors and positions
		_gradientColors = new[]
		{
			SKColor.Parse("#00D9FF"), // Cyan
			SKColor.Parse("#0099FF"), // Blue
			SKColor.Parse("#6B4FBB"), // Purple
			SKColor.Parse("#FF8C42"), // Orange
			SKColor.Parse("#FFD700")  // Yellow
		};

		_gradientPositions = new[] { 0f, 0.25f, 0.5f, 0.75f, 1.0f };

		// Initialize haptic feedback
		InitializeHaptics();
	}

	private async void InitializeHaptics()
	{
		try
		{
			var vibrationDevice = await VibrationDevice.GetDefaultAsync();
			if (vibrationDevice != null)
			{
				_hapticsController = vibrationDevice.SimpleHapticsController;
				_clickFeedback = _hapticsController.SupportedFeedback.FirstOrDefault(
					feedback => feedback.Waveform == KnownSimpleHapticsControllerWaveforms.Click);
			}
		}
		catch
		{
			// Haptics not supported on this platform
			_hapticsController = null;
			_clickFeedback = null;
		}
	}

	private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ThermostatRenderer renderer)
		{
			renderer._canvas.Invalidate();
		}
	}

	private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.Transparent);

		var info = e.Info;
		_centerX = info.Width / 2f;
		_centerY = info.Height / 2f;
		_radius = Math.Min(_centerX, _centerY) - 40;

		// Draw concentric circles for depth
		DrawConcentricCircles(canvas, _centerX, _centerY, _radius);

		// Draw background arc
		DrawBackgroundArc(canvas, _centerX, _centerY, _radius);

		// Draw progress arc with gradient
		DrawProgressArc(canvas, _centerX, _centerY, _radius);

		// Draw handle
		DrawHandle(canvas, _centerX, _centerY, _radius);

		// Draw center text
		DrawCenterText(canvas, _centerX, _centerY);
	}

	private void DrawConcentricCircles(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		// Draw 3 concentric circles
		for (int i = 1; i <= 3; i++)
		{
			var circleRadius = radius - (i * 30);
			if (circleRadius > 0)
			{
				canvas.DrawCircle(centerX, centerY, circleRadius, _concentricCirclePaint);
			}
		}
	}

	private void DrawBackgroundArc(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
		canvas.DrawArc(rect, 135, 270, false, _backgroundArcPaint);
	}

	private void DrawProgressArc(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var sweepAngle = (float)(270 * Progress);

		if (sweepAngle <= 0)
			return;

		var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

		using var shader = SKShader.CreateSweepGradient(
			new SKPoint(centerX, centerY),
			_gradientColors,
			_gradientPositions,
			SKShaderTileMode.Clamp,
			135,
			135 + sweepAngle
		);

		_progressArcPaint.Shader = shader;
		canvas.DrawArc(rect, 135, sweepAngle, false, _progressArcPaint);
		_progressArcPaint.Shader = null;
	}

	private void DrawHandle(SKCanvas canvas, float centerX, float centerY, float radius)
	{
		var angle = 135 + (270 * Progress);
		var radians = angle * Math.PI / 180;

		var handleX = centerX + (float)(radius * Math.Cos(radians));
		var handleY = centerY + (float)(radius * Math.Sin(radians));

		canvas.DrawCircle(handleX, handleY, 12, _handlePaint);
	}

	private void DrawCenterText(SKCanvas canvas, float centerX, float centerY)
	{
		// Draw home icon (U+E80F)
		canvas.DrawText("\uE80F", centerX - 30, centerY - 45, SKTextAlign.Center, _iconFont, _iconPaint);

		// Draw "INDOOR" label
		canvas.DrawText("INDOOR", centerX + 10, centerY - 45, SKTextAlign.Center, _indoorLabelFont, _indoorLabelPaint);

		// Draw temperature
		canvas.DrawText($"{CurrentTemperature:F1}°", centerX, centerY + 10, SKTextAlign.Center, _temperatureFont, _temperaturePaint);

		// Draw status text
		canvas.DrawText(StatusText, centerX, centerY + 65, SKTextAlign.Center, _statusFont, _statusPaint);
	}

	private void OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		_isTracking = true;
		UpdateProgressFromPointer(e);
		((UIElement)sender).CapturePointer(e.Pointer);
	}

	private void OnPointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		if (_isTracking)
		{
			UpdateProgressFromPointer(e);
		}
	}

	private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		_isTracking = false;
		((UIElement)sender).ReleasePointerCapture(e.Pointer);
	}

	private void UpdateProgressFromPointer(Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		var point = e.GetCurrentPoint(_canvas);
		var x = (float)point.Position.X;
		var y = (float)point.Position.Y;

		// Calculate angle from center
		var dx = x - _centerX;
		var dy = y - _centerY;
		var angleRad = Math.Atan2(dy, dx);
		var angleDeg = angleRad * 180 / Math.PI;

		// Normalize angle to 0-360
		if (angleDeg < 0)
			angleDeg += 360;

		// Calculate the actual rotation angle (accounting for the arc's 135° start)
		var rotationAngle = angleDeg >= 135 ? angleDeg - 135 : angleDeg + 225;

		// Trigger haptic feedback if angle changed by at least 1 degree
		if (Math.Abs(rotationAngle - _lastAngleDegrees) >= 1.0)
		{
			TriggerHapticFeedback();
			_lastAngleDegrees = rotationAngle;
		}

		// Convert to progress (135° to 405° maps to 0 to 1)
		// 135° is the start, 405° (45°) is the end
		if (angleDeg >= 135)
		{
			Progress = (angleDeg - 135) / 270.0;
		}
		else if (angleDeg <= 45)
		{
			Progress = (angleDeg + 225) / 270.0;
		}
		else
		{
			// In the dead zone (45° to 135°), snap to nearest end
			if (angleDeg < 90)
				Progress = 1.0;
			else
				Progress = 0.0;
		}

		// Clamp to 0-1
		Progress = Math.Clamp(Progress, 0.0, 1.0);
	}

	private void TriggerHapticFeedback()
	{
		if (_hapticsController != null && _clickFeedback != null)
		{
			try
			{
				_hapticsController.SendHapticFeedback(_clickFeedback);
			}
			catch
			{
				// Ignore errors - haptic feedback is optional
			}
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
			// Unsubscribe from canvas events
			_canvas.PaintSurface -= OnPaintSurface;
			_canvas.PointerPressed -= OnPointerPressed;
			_canvas.PointerMoved -= OnPointerMoved;
			_canvas.PointerReleased -= OnPointerReleased;

			// Dispose managed resources
			_concentricCirclePaint?.Dispose();
			_backgroundArcPaint?.Dispose();
			_progressArcPaint?.Dispose();
			_handlePaint?.Dispose();
			_iconPaint?.Dispose();
			_indoorLabelPaint?.Dispose();
			_temperaturePaint?.Dispose();
			_statusPaint?.Dispose();

			_iconFont?.Dispose();
			_indoorLabelFont?.Dispose();
			_temperatureFont?.Dispose();
			_statusFont?.Dispose();

			_iconTypeface?.Dispose();
			_temperatureTypeface?.Dispose();
		}

		_disposed = true;
	}
}

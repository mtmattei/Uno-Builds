using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AdTokensIDE.Controls;

public sealed partial class TokenCounter : UserControl
{
    private readonly TextBlock[] _digitTexts;
    private readonly Border[] _digitBorders;
    private int _displayedValue;
    private DispatcherTimer? _rollTimer;
    private int _targetValue;
    private bool _isRollingDown;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(TokenCounter),
            new PropertyMetadata(0, OnValueChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TokenCounter()
    {
        InitializeComponent();

        _digitTexts = new[] { DigitText0, DigitText1, DigitText2, DigitText3, DigitText4 };
        _digitBorders = new[] { Digit0, Digit1, Digit2, Digit3, Digit4 };

        UpdateDisplay(0);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TokenCounter counter)
        {
            var newValue = (int)e.NewValue;
            var oldValue = (int)e.OldValue;
            counter.AnimateToValue(newValue, newValue < oldValue);
        }
    }

    private void AnimateToValue(int targetValue, bool isRollingDown)
    {
        _targetValue = targetValue;
        _isRollingDown = isRollingDown;

        // Stop any existing animation
        _rollTimer?.Stop();

        // Calculate step size based on difference
        var difference = Math.Abs(targetValue - _displayedValue);

        if (difference == 0) return;

        int interval;
        int step;

        if (isRollingDown)
        {
            // Show earned label during roll-down
            EarnedLabel.Text = $"-{difference} earned";
            EarnedLabel.Visibility = Visibility.Visible;

            // Slower, more dramatic roll down when earning tokens
            // Always step by 1 for satisfying countdown effect
            interval = Math.Max(15, 800 / Math.Max(1, difference)); // Spread over ~800ms
            step = 1;
        }
        else
        {
            // Hide earned label during roll-up
            EarnedLabel.Visibility = Visibility.Collapsed;

            // Faster rolling up during generation
            interval = difference > 100 ? 5 :
                       difference > 50 ? 10 :
                       difference > 20 ? 20 : 30;
            step = Math.Max(1, difference / 20);
        }

        _rollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(interval)
        };

        _rollTimer.Tick += (s, e) =>
        {
            if (_isRollingDown)
            {
                _displayedValue = Math.Max(_targetValue, _displayedValue - step);
            }
            else
            {
                _displayedValue = Math.Min(_targetValue, _displayedValue + step);
            }

            UpdateDisplay(_displayedValue);

            // Flash effect on digit change
            FlashChangedDigits();

            if (_displayedValue == _targetValue)
            {
                _rollTimer.Stop();

                // Highlight effect when rolling down (earning tokens)
                if (_isRollingDown)
                {
                    FlashAllDigitsGreen();
                }
            }
        };

        _rollTimer.Start();
    }

    private void UpdateDisplay(int value)
    {
        var valueStr = value.ToString("D5"); // Pad to 5 digits

        for (int i = 0; i < 5; i++)
        {
            _digitTexts[i].Text = valueStr[i].ToString();
        }
    }

    private void FlashChangedDigits()
    {
        // Brief brightness flash on active digits
        var valueStr = _displayedValue.ToString("D5");

        for (int i = 0; i < 5; i++)
        {
            // Leading zeros are dimmer
            if (valueStr[i] == '0' && i < 4 && _displayedValue < Math.Pow(10, 4 - i))
            {
                _digitTexts[i].Opacity = 0.3;
            }
            else
            {
                _digitTexts[i].Opacity = 1.0;
            }
        }
    }

    private async void FlashAllDigitsGreen()
    {
        // Quick flash animation when tokens are earned
        foreach (var border in _digitBorders)
        {
            border.Background = new SolidColorBrush(Color.FromArgb(255, 50, 205, 50)); // Lime green tint
        }

        await Task.Delay(150);

        foreach (var border in _digitBorders)
        {
            border.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26)); // Back to #1A1A1A
        }
    }
}

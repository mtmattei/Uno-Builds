using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;

namespace AdaptiveInputDemo.Controls;

public sealed partial class RangePickerPanel : UserControl
{
    private bool _isUpdating;
    private double _absoluteMax = 200;

    public event EventHandler<string>? RangeSelected;

    public RangePickerPanel()
    {
        InitializeComponent();
        UpdateRangeVisual();
    }

    public void UpdateValue(string value)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            // Parse range like "50-100" or "50 to 100"
            var match = Regex.Match(value, @"^(\d+)\s*(?:-|–|to|through)\s*(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var min = double.Parse(match.Groups[1].Value);
                var max = double.Parse(match.Groups[2].Value);

                // Adjust absolute max if needed
                if (max > _absoluteMax)
                {
                    _absoluteMax = max * 1.5;
                    MinSlider.Maximum = _absoluteMax;
                    MaxSlider.Maximum = _absoluteMax;
                    MinInput.Maximum = _absoluteMax;
                    MaxInput.Maximum = _absoluteMax;
                }

                MinInput.Value = min;
                MaxInput.Value = max;
                MinSlider.Value = min;
                MaxSlider.Value = max;
            }
            else if (double.TryParse(value, out var singleValue))
            {
                // Single number - use as starting point
                MinInput.Value = singleValue;
                MaxInput.Value = singleValue;
                MinSlider.Value = singleValue;
                MaxSlider.Value = singleValue;
            }

            UpdateRangeVisual();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnMinValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            var newMin = args.NewValue;
            if (double.IsNaN(newMin)) newMin = 0;

            // Ensure min <= max
            if (newMin > MaxInput.Value)
            {
                MaxInput.Value = newMin;
                MaxSlider.Value = newMin;
            }

            MinSlider.Value = newMin;
            UpdateRangeVisual();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnMaxValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            var newMax = args.NewValue;
            if (double.IsNaN(newMax)) newMax = 100;

            // Ensure max >= min
            if (newMax < MinInput.Value)
            {
                MinInput.Value = newMax;
                MinSlider.Value = newMax;
            }

            MaxSlider.Value = newMax;
            UpdateRangeVisual();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnMinSliderChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            var newMin = e.NewValue;

            // Ensure min <= max
            if (newMin > MaxSlider.Value)
            {
                MaxSlider.Value = newMin;
                MaxInput.Value = newMin;
            }

            MinInput.Value = newMin;
            UpdateRangeVisual();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnMaxSliderChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            var newMax = e.NewValue;

            // Ensure max >= min
            if (newMax < MinSlider.Value)
            {
                MinSlider.Value = newMax;
                MinInput.Value = newMax;
            }

            MaxInput.Value = newMax;
            UpdateRangeVisual();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateRangeVisual()
    {
        if (RangeFill == null) return;

        var min = MinSlider.Value;
        var max = MaxSlider.Value;
        var range = MaxSlider.Maximum - MinSlider.Minimum;

        if (range <= 0) return;

        var leftPercent = (min - MinSlider.Minimum) / range;
        var widthPercent = (max - min) / range;

        if (RangeFill.Parent is Grid parentGrid)
        {
            RangeFill.Margin = new Thickness(leftPercent * parentGrid.ActualWidth, 0, 0, 0);
            RangeFill.Width = widthPercent * parentGrid.ActualWidth;
        }
    }

    private void OnPresetClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string preset)
        {
            var parts = preset.Split('-');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var min) &&
                double.TryParse(parts[1], out var max))
            {
                _isUpdating = true;
                MinInput.Value = min;
                MaxInput.Value = max;
                MinSlider.Value = min;
                MaxSlider.Value = max;
                _isUpdating = false;
                UpdateRangeVisual();

                RangeSelected?.Invoke(this, preset);
            }
        }
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        var min = (int)MinInput.Value;
        var max = (int)MaxInput.Value;
        RangeSelected?.Invoke(this, $"{min}-{max}");
    }
}

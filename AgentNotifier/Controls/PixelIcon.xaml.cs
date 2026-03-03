using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace AgentNotifier.Controls;

public sealed partial class PixelIcon : UserControl
{
    private Rectangle[,] _pixels = new Rectangle[8, 8];

    public static readonly DependencyProperty PatternProperty =
        DependencyProperty.Register(nameof(Pattern), typeof(string[]), typeof(PixelIcon),
            new PropertyMetadata(null, OnPatternChanged));

    public static readonly DependencyProperty PixelColorProperty =
        DependencyProperty.Register(nameof(PixelColor), typeof(Brush), typeof(PixelIcon),
            new PropertyMetadata(new SolidColorBrush(Colors.Cyan), OnColorChanged));

    public static readonly DependencyProperty IsBlinkingProperty =
        DependencyProperty.Register(nameof(IsBlinking), typeof(bool), typeof(PixelIcon),
            new PropertyMetadata(true, OnBlinkingChanged));

    public string[]? Pattern
    {
        get => (string[]?)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public Brush PixelColor
    {
        get => (Brush)GetValue(PixelColorProperty);
        set => SetValue(PixelColorProperty, value);
    }

    public bool IsBlinking
    {
        get => (bool)GetValue(IsBlinkingProperty);
        set => SetValue(IsBlinkingProperty, value);
    }

    public PixelIcon()
    {
        this.InitializeComponent();
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        PixelGrid.RowDefinitions.Clear();
        PixelGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < 8; i++)
        {
            PixelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            PixelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var pixel = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Margin = new Thickness(0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5
                };
                Grid.SetRow(pixel, row);
                Grid.SetColumn(pixel, col);
                PixelGrid.Children.Add(pixel);
                _pixels[row, col] = pixel;
            }
        }
    }

    private static void OnPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PixelIcon icon)
            icon.UpdatePattern();
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PixelIcon icon)
            icon.UpdatePattern();
    }

    private static void OnBlinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PixelIcon icon)
            icon.UpdateOpacity();
    }

    private void UpdatePattern()
    {
        var pattern = Pattern;
        if (pattern == null || pattern.Length != 8) return;

        var color = PixelColor;

        for (int row = 0; row < 8; row++)
        {
            var rowPattern = pattern[row];
            for (int col = 0; col < 8 && col < rowPattern.Length; col++)
            {
                var isFilled = rowPattern[col] == '#';
                _pixels[row, col].Fill = isFilled ? color : new SolidColorBrush(Colors.Transparent);
            }
        }
    }

    private void UpdateOpacity()
    {
        Opacity = IsBlinking ? 1.0 : 0.3;
    }
}

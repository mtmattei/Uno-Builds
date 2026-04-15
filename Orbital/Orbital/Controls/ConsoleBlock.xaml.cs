using Microsoft.UI.Xaml.Media;
using Orbital.Helpers;

namespace Orbital.Controls;

public sealed partial class ConsoleBlock : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(ConsoleBlock),
            new PropertyMetadata("Console", OnTitleChanged));

    public ConsoleBlock()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => ScanlineAnimation.Begin();
        this.Unloaded += (_, _) => ScanlineAnimation.Stop();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConsoleBlock block)
            block.TitleText.Text = (string)e.NewValue;
    }

    public void SetLines(IEnumerable<ConsoleLine> lines)
    {
        LinesPanel.Children.Clear();
        var lineNum = 1;
        foreach (var line in lines)
        {
            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(24) },
                    new ColumnDefinition { Width = new GridLength(12) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                Margin = new Thickness(0, 0, 0, 2),
            };

            var numText = new TextBlock
            {
                Text = lineNum.ToString(),
                Style = (Style)Application.Current.Resources["OrbitalMonoLineNumber"],
            };
            Grid.SetColumn(numText, 0);
            row.Children.Add(numText);

            var contentText = new TextBlock
            {
                Text = line.Text,
                FontFamily = (FontFamily)Application.Current.Resources["OrbitalMonoFont"],
                FontSize = 12,
                Foreground = GetLineBrush(line.Type),
                TextWrapping = TextWrapping.NoWrap,
                LineHeight = 20.4, // 12px * 1.7
            };
            Grid.SetColumn(contentText, 2);
            row.Children.Add(contentText);

            LinesPanel.Children.Add(row);
            lineNum++;
        }
    }

    private static readonly SolidColorBrush _successBrush = new(OrbitalColors.Success);
    private static readonly SolidColorBrush _errorBrush = new(OrbitalColors.Error);
    private static readonly SolidColorBrush _warnBrush = new(OrbitalColors.Warn);
    private static readonly SolidColorBrush _dimBrush = new(OrbitalColors.Dim);
    private static readonly SolidColorBrush _infoBrush = new(OrbitalColors.Info);

    private static SolidColorBrush GetLineBrush(string type) => type switch
    {
        "success" => _successBrush,
        "error" => _errorBrush,
        "warn" => _warnBrush,
        "dim" => _dimBrush,
        _ => _infoBrush,
    };
}

public record ConsoleLine(string Text, string Type = "info");

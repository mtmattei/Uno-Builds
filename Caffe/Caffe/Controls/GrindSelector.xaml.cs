using Caffe.Models;

namespace Caffe.Controls;

public sealed partial class GrindSelector : UserControl
{
    private readonly SolidColorBrush _selectedBrush;
    private readonly SolidColorBrush _unselectedBrush;
    private readonly SolidColorBrush _particleBrush;
    private readonly Random _random = new(42); // Fixed seed for consistent layout
    private GrindLevel _lastParticleLevel = (GrindLevel)(-1);

    public static readonly DependencyProperty GrindLevelProperty =
        DependencyProperty.Register(
            nameof(GrindLevel),
            typeof(GrindLevel),
            typeof(GrindSelector),
            new PropertyMetadata(GrindLevel.Fine, OnGrindLevelChanged));

    public GrindLevel GrindLevel
    {
        get => (GrindLevel)GetValue(GrindLevelProperty);
        set => SetValue(GrindLevelProperty, value);
    }

    public event EventHandler<GrindLevel>? GrindLevelChanged;

    public GrindSelector()
    {
        this.InitializeComponent();

        _selectedBrush = (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"];
        _unselectedBrush = (SolidColorBrush)Application.Current.Resources["CaffeBorderBrush"];
        _particleBrush = (SolidColorBrush)Application.Current.Resources["CaffeParticleBrush"];

        UpdateVisual(GrindLevel.Fine);
    }

    private static void OnGrindLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GrindSelector selector)
        {
            selector.UpdateVisual((GrindLevel)e.NewValue);
        }
    }

    private void OnFineClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Fine);
    private void OnMediumClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Medium);
    private void OnCoarseClick(object sender, RoutedEventArgs e) => SetGrindLevel(GrindLevel.Coarse);

    private void SetGrindLevel(GrindLevel level)
    {
        GrindLevel = level;
        UpdateVisual(level);
        GrindLevelChanged?.Invoke(this, level);
    }

    private void UpdateVisual(GrindLevel level)
    {
        // Update labels
        GrindLabelText.Text = level.ToLabel();
        GrindHintText.Text = level.ToHint();

        // Update button states
        FineButton.Background = level == GrindLevel.Fine ? _selectedBrush : _unselectedBrush;
        MediumButton.Background = level == GrindLevel.Medium ? _selectedBrush : _unselectedBrush;
        CoarseButton.Background = level == GrindLevel.Coarse ? _selectedBrush : _unselectedBrush;

        // Update particle display
        UpdateParticles(level);
    }

    private void UpdateParticles(GrindLevel level)
    {
        if (_lastParticleLevel == level) return;
        _lastParticleLevel = level;

        ParticleGrid.Children.Clear();
        ParticleGrid.RowDefinitions.Clear();
        ParticleGrid.ColumnDefinitions.Clear();

        var (count, size) = level switch
        {
            GrindLevel.Fine => (12, 6.0),
            GrindLevel.Medium => (9, 9.0),
            GrindLevel.Coarse => (6, 13.0),
            _ => (9, 9.0)
        };

        var cols = level switch
        {
            GrindLevel.Fine => 4,
            GrindLevel.Medium => 3,
            GrindLevel.Coarse => 3,
            _ => 3
        };

        var rows = (int)Math.Ceiling((double)count / cols);

        for (int c = 0; c < cols; c++)
            ParticleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (int r = 0; r < rows; r++)
            ParticleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        for (int i = 0; i < count; i++)
        {
            var row = i / cols;
            var col = i % cols;

            var ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = _particleBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetRow(ellipse, row);
            Grid.SetColumn(ellipse, col);
            ParticleGrid.Children.Add(ellipse);
        }
    }
}

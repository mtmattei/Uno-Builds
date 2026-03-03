using System.ComponentModel;
using Caffe.Models;
using Caffe.ViewModels;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Caffe;

public record ParticleInfo(double X, double Y, double Size);

public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    public MainViewModel ViewModel { get; } = new();

    private List<ParticleInfo> _particles = new();
    public List<ParticleInfo> Particles
    {
        get => _particles;
        private set
        {
            _particles = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Particles)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPage()
    {
        this.InitializeComponent();
        UpdateParticles();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.GrindLevel))
        {
            UpdateParticles();
        }
    }

    private void UpdateParticles()
    {
        var level = ViewModel.GrindLevel;
        var particles = new List<ParticleInfo>();
        var count = level.GetParticleCount();
        var size = level.GetParticleSize();

        // Clean grid layout per brief:
        // Fine: 12 dots in 4x3 grid (2-3px)
        // Medium: 9 dots in 3x3 grid (4-5px)
        // Coarse: 6 dots in 3x2 grid (7-9px)
        var cols = level switch
        {
            GrindLevel.Fine => 4,   // 4x3 = 12
            GrindLevel.Medium => 3, // 3x3 = 9
            GrindLevel.Coarse => 3, // 3x2 = 6
            _ => 3
        };

        var rows = level switch
        {
            GrindLevel.Fine => 3,
            GrindLevel.Medium => 3,
            GrindLevel.Coarse => 2,
            _ => 3
        };

        // Calculate spacing for even distribution in 50x50 area
        var areaSize = 50.0;
        var spacingX = areaSize / cols;
        var spacingY = areaSize / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (particles.Count >= count) break;

                // Center each dot in its cell
                var x = col * spacingX + (spacingX - size) / 2;
                var y = row * spacingY + (spacingY - size) / 2;
                particles.Add(new ParticleInfo(x, y, size));
            }
        }

        Particles = particles;
    }

    // Get time gauge size based on extraction time (20-35 maps to 10-50)
    public double GetTimeGaugeSize(int extractionTime)
    {
        // Map 20-35 to 10-50 for the inner circle size
        var normalized = (extractionTime - 20) / 15.0;
        return 10 + (normalized * 40);
    }

    // Get grind visual size - center dot that represents particle size
    public double GetGrindVisualSize(GrindLevel level)
    {
        return level switch
        {
            GrindLevel.Fine => 8,    // Small center dot for fine grind
            GrindLevel.Medium => 16, // Medium center dot
            GrindLevel.Coarse => 28, // Large center dot for coarse grind
            _ => 16
        };
    }

    // Get grind dot color based on selection
    public Brush GetGrindDotColor(GrindLevel currentLevel, int buttonLevel)
    {
        if ((int)currentLevel == buttonLevel)
        {
            return (Brush)Application.Current.Resources["CaffePrimaryBrush"];
        }
        return (Brush)Application.Current.Resources["CaffeBorderBrush"];
    }

    // Get grind button background fill (for selected state highlight)
    public Brush GetGrindButtonFill(GrindLevel currentLevel, int buttonLevel)
    {
        if ((int)currentLevel == buttonLevel)
        {
            return (Brush)Application.Current.Resources["CaffePrimaryBrush"];
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    // Static helper methods for card selection
    public static Brush GetCardBorderBrush(EspressoItem? selected, EspressoItem card)
    {
        if (selected is not null && selected.Name == card.Name)
        {
            return (Brush)Application.Current.Resources["CaffePrimaryBrush"];
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public static Visibility IsSelected(EspressoItem? selected, EspressoItem card)
    {
        if (selected is not null && selected.Name == card.Name)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    // Grind level tap handlers
    private void OnGrindFineTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.SetGrindLevelCommand.Execute(GrindLevel.Fine);
    }

    private void OnGrindMediumTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.SetGrindLevelCommand.Execute(GrindLevel.Medium);
    }

    private void OnGrindCoarseTapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel.SetGrindLevelCommand.Execute(GrindLevel.Coarse);
    }
}

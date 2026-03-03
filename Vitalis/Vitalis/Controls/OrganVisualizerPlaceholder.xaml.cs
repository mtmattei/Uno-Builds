using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Vitalis.Models;
using Windows.UI;

namespace Vitalis.Controls;

public sealed partial class OrganVisualizerPlaceholder : UserControl
{
    public event EventHandler<Organ>? OrganSelected;
    private DispatcherTimer? _rotationTimer;
    private double _rotation = 0;
    private string _selectedOrganId = "heart";

    public OrganVisualizerPlaceholder()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start selection ring rotation animation
        _rotationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _rotationTimer.Tick += (s, args) =>
        {
            _rotation += 1;
            if (_rotation >= 360) _rotation = 0;
            RingRotation.Angle = _rotation;
        };
        _rotationTimer.Start();

        // Set initial selection
        UpdateSelectionRing("heart");
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _rotationTimer?.Stop();
    }

    private void OnOrganClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        var organId = button.Name switch
        {
            "HeartButton" => "heart",
            "BrainButton" => "brain",
            "LungsButton" => "lungs",
            "LiverButton" => "liver",
            _ => "heart"
        };

        _selectedOrganId = organId;
        UpdateSelectionRing(organId);

        var organ = organId switch
        {
            "heart" => OrganData.Heart,
            "brain" => OrganData.Brain,
            "lungs" => OrganData.Lungs,
            "liver" => OrganData.Liver,
            _ => OrganData.Heart
        };

        OrganSelected?.Invoke(this, organ);
    }

    private void UpdateSelectionRing(string organId)
    {
        var (margin, color) = organId switch
        {
            "heart" => (new Thickness(-145, -75, 0, 0), Color.FromArgb(255, 239, 68, 68)),
            "brain" => (new Thickness(145, -75, 0, 0), Color.FromArgb(255, 168, 85, 247)),
            "lungs" => (new Thickness(-145, 115, 0, 0), Color.FromArgb(255, 59, 130, 246)),
            "liver" => (new Thickness(145, 115, 0, 0), Color.FromArgb(255, 234, 179, 8)),
            _ => (new Thickness(-145, -75, 0, 0), Color.FromArgb(255, 239, 68, 68))
        };

        SelectionRing.Margin = margin;
        SelectionRing.Stroke = new SolidColorBrush(color);
    }
}

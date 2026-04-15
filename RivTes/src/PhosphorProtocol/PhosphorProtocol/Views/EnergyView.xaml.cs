using Microsoft.UI.Xaml.Controls;

namespace PhosphorProtocol.Views;

public sealed partial class EnergyView : UserControl
{
    private readonly DispatcherTimer _chevronTimer;
    private int _chevronStep;
    private Border[] _chevrons = [];
    private TextBlock? _chevronArrow;

    // Each element peaks at a different step to create left-to-right flow.
    // 5 elements, 5 steps per cycle. Element i peaks at step i.
    private static readonly double[][] _opacityTable =
    [
        // Step 0: Chev1 peak
        [1.0, 0.4, 0.1, 0.1, 0.1],
        // Step 1: Chev2 peak
        [0.4, 1.0, 0.4, 0.1, 0.1],
        // Step 2: Chev3 peak
        [0.1, 0.4, 1.0, 0.4, 0.1],
        // Step 3: Chev4 peak
        [0.1, 0.1, 0.4, 1.0, 0.4],
        // Step 4: Arrow peak
        [0.1, 0.1, 0.1, 0.4, 1.0],
    ];

    public EnergyView()
    {
        this.InitializeComponent();

        _chevronTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(240) // 1.2s / 5 steps = 240ms per step
        };
        _chevronTimer.Tick += OnChevronTick;

        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _chevrons = [Chev1, Chev2, Chev3, Chev4];
        _chevronArrow = ChevArrow;
        _chevronStep = 0;
        ApplyChevronOpacities();
        _chevronTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _chevronTimer.Stop();
    }

    private void OnChevronTick(object? sender, object e)
    {
        _chevronStep = (_chevronStep + 1) % _opacityTable.Length;
        ApplyChevronOpacities();
    }

    private void ApplyChevronOpacities()
    {
        var row = _opacityTable[_chevronStep];
        for (int i = 0; i < _chevrons.Length; i++)
        {
            _chevrons[i].Opacity = row[i];
        }

        if (_chevronArrow is not null)
        {
            _chevronArrow.Opacity = row[4];
        }
    }
}

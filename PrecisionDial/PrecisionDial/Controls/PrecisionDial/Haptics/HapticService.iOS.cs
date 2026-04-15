using UIKit;

namespace PrecisionDial.Controls;

public partial class HapticService
{
    private UIImpactFeedbackGenerator? _lightGenerator;
    private UIImpactFeedbackGenerator? _mediumGenerator;
    private UIImpactFeedbackGenerator? _heavyGenerator;
    private UISelectionFeedbackGenerator? _selectionGenerator;

    public partial void Prepare()
    {
        _lightGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
        _mediumGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);
        _heavyGenerator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Heavy);
        _selectionGenerator = new UISelectionFeedbackGenerator();
        _lightGenerator.Prepare();
        _mediumGenerator.Prepare();
        _heavyGenerator.Prepare();
        _selectionGenerator.Prepare();
    }

    public partial void FireDetentClick() => _lightGenerator?.ImpactOccurred();
    public partial void FireMajorDetentClick() => _mediumGenerator?.ImpactOccurred();
    public partial void FireBoundaryStop() => _heavyGenerator?.ImpactOccurred();
    public partial void FireSelectionTick() => _selectionGenerator?.SelectionChanged();

    public partial void Release()
    {
        _lightGenerator?.Dispose(); _mediumGenerator?.Dispose();
        _heavyGenerator?.Dispose(); _selectionGenerator?.Dispose();
        _lightGenerator = null; _mediumGenerator = null;
        _heavyGenerator = null; _selectionGenerator = null;
    }
}

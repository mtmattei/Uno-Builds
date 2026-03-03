using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;

namespace UnoVox.Presentation;

public sealed partial class VoxelEditorPage : Page
{
    public VoxelEditorViewModel? ViewModel { get; private set; }

    public VoxelEditorPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] DataContextChanged - new value: {args.NewValue?.GetType().Name ?? "null"}");
        WireUpViewModel();
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] OnPageLoaded - DataContext: {DataContext?.GetType().Name ?? "null"}");
        WireUpViewModel();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[VoxelEditorPage] OnPageUnloaded - unwiring events");
        UnwireViewModel();

        this.Loaded -= OnPageLoaded;
        this.Unloaded -= OnPageUnloaded;
        this.DataContextChanged -= OnDataContextChanged;
    }

    private bool _eventsWired = false;

    private void WireUpViewModel()
    {
        // Avoid wiring up multiple times
        if (_eventsWired)
        {
            System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] Events already wired, skipping");
            return;
        }

        if (DataContext is VoxelEditorViewModel vm)
        {
            ViewModel = vm;
            _eventsWired = true;

            // Wire up canvas events programmatically (x:Bind doesn't work when ViewModel is null at creation)
            System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] Wiring up canvas events to ViewModel");
            VoxelCanvas.PaintSurface += vm.OnPaintSurface;
            VoxelCanvas.PointerPressed += vm.OnPointerPressed;
            VoxelCanvas.PointerMoved += vm.OnPointerMoved;
            VoxelCanvas.PointerReleased += vm.OnPointerReleased;
            VoxelCanvas.PointerWheelChanged += vm.OnPointerWheelChanged;

            // Pass canvas reference to ViewModel for invalidation
            System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] Setting canvas reference on ViewModel");
            vm.SetCanvas(VoxelCanvas);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] DataContext is not VoxelEditorViewModel: {DataContext?.GetType().Name ?? "null"}");
        }
    }

    private void UnwireViewModel()
    {
        if (!_eventsWired || ViewModel is not { } vm)
            return;

        VoxelCanvas.PaintSurface -= vm.OnPaintSurface;
        VoxelCanvas.PointerPressed -= vm.OnPointerPressed;
        VoxelCanvas.PointerMoved -= vm.OnPointerMoved;
        VoxelCanvas.PointerReleased -= vm.OnPointerReleased;
        VoxelCanvas.PointerWheelChanged -= vm.OnPointerWheelChanged;

        _eventsWired = false;
        ViewModel = null;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        System.Diagnostics.Debug.WriteLine($"[VoxelEditorPage] OnNavigatedTo - DataContext type: {DataContext?.GetType().Name ?? "null"}");
    }

    private void OnDeleteKeyPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel?.DeleteSelectedCommand.Execute(null);
        args.Handled = true;
    }
}

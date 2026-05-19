using System.ComponentModel;
using Composer.Models;
using Composer.Services;
using Composer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Composer;

/// <summary>
/// Shell host for the context-engine composer. Three-column workspace: left
/// rail (composition stack), center (active layer canvas), right rail (live
/// files). The center column is the surface that will become a
/// <c>uen:Region.Attached</c> navigation region in the next pass (per
/// <c>docs/ENGINEERING-BRIEF-page-and-flow-breakdown.md</c> §2.2). Today it
/// hosts <see cref="Composer.Views.Controls.ActiveCanvas"/>, which dispatches
/// to per-layer canvas UserControls based on the active layer index.
/// </summary>
public sealed partial class Shell : Page
{
    private INotifyPropertyChanged? _vm;
    private bool _railsVisibleApplied;

    public Shell()
    {
        this.InitializeComponent();
        this.Loaded   += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var services = App.Host?.Services;
        if (services is null) return;

        DataContext = new ComposerViewModel(
            services.GetRequiredService<Composer.Models.Presentation.ShellModel>(),
            services.GetRequiredService<Composer.Models.Presentation.IntentModel>(),
            services.GetRequiredService<Composer.Models.Presentation.UXModel>(),
            services.GetRequiredService<Composer.Models.Presentation.ArchitectureModel>(),
            services.GetRequiredService<Composer.Models.Presentation.DesignModel>(),
            services.GetRequiredService<Composer.Models.Presentation.InteractionsModel>(),
            services.GetRequiredService<Composer.Models.Presentation.DataModel>(),
            services.GetRequiredService<Composer.Models.Presentation.ImplementationModel>(),
            services.GetRequiredService<IBundleExporter>(),
            services.GetRequiredService<IUnoSdkVersionService>(),
            services.GetRequiredService<ILayerPreviewService>(),
            services.GetRequiredService<IContextDeriver>(),
            services.GetRequiredService<IOptions<AnthropicConfig>>());

        if (DataContext is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }

        // Initial sync — the model starts with RailsVisible=false (Stack
        // canvas, no locks, ActiveIndex=0). This is a no-op for the storyboard
        // but locks in the rails-applied flag so the first true→ transition
        // animates correctly.
        SyncRailsAnimation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "RailsVisible")
            DispatcherQueue?.TryEnqueue(SyncRailsAnimation);
    }

    private void SyncRailsAnimation()
    {
        var dc = DataContext;
        if (dc is null) return;
        var visible = (dc.GetType().GetProperty("RailsVisible")?.GetValue(dc) as bool?) ?? false;

        // Skip if no transition.
        if (visible == _railsVisibleApplied) return;
        _railsVisibleApplied = visible;

        // Snap the column widths — Grid columns don't smoothly re-measure
        // under DoubleAnimation on Skia desktop, so instead we animate the
        // rail content (translate + opacity) inside fixed columns.
        var width = visible ? new GridLength(280, GridUnitType.Pixel)
                            : new GridLength(0,   GridUnitType.Pixel);
        LeftRailColumn.Width  = width;
        RightRailColumn.Width = width;

        // Reduced motion: snap content as well.
        if (!Composer.Views.MotionPreferences.AnimationsEnabled)
        {
            LeftRailContainer.Opacity  = visible ? 1 : 0;
            RightRailContainer.Opacity = visible ? 1 : 0;
            ((TranslateTransform)LeftRailContainer.RenderTransform).X  = visible ?  0 : -40;
            ((TranslateTransform)RightRailContainer.RenderTransform).X = visible ?  0 :  40;
            return;
        }

        var key = visible ? "RailsRevealStoryboard" : "RailsHideStoryboard";
        if (Resources[key] is Storyboard sb) sb.Begin();
    }
}

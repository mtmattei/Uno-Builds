using PhosphorProtocol.Models;
using PhosphorProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PhosphorProtocol.Views;

public sealed partial class AutopilotView : UserControl
{
    private readonly DispatcherTimer _feedTimer;
    private IAutopilotService? _service;

    public AutopilotView()
    {
        this.InitializeComponent();

        // Poll the service directly to push state to the Skia canvas at high frequency
        _feedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _feedTimer.Tick += async (_, _) =>
        {
            _service ??= ((App)Application.Current).Host?.Services.GetService<IAutopilotService>();
            if (_service is null) return;

            try
            {
                var state = await _service.GetCurrentState(CancellationToken.None);
                PerceptionCanvas.UpdateState(state);
            }
            catch
            {
                // Service not yet available
            }
        };

        Loaded += (_, _) => _feedTimer.Start();
        Unloaded += (_, _) => _feedTimer.Stop();
    }

    private void OnExitAutopilotClick(object sender, RoutedEventArgs e)
    {
        // Walk up visual tree to find DashboardShell and select the Nav tab
        DependencyObject? current = this;
        while (current is not null)
        {
            if (current is DashboardShell shell)
            {
                shell.NavigateToNav();
                return;
            }
            current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
        }
    }
}

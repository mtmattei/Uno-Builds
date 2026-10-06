namespace SmartCity.Presentation;

/// <summary>
/// Chrome shell for the dashboard: hosts the top nav (regions), the icon rail and the
/// content area. Mostly static console chrome; the live data lives in <see cref="EnergyDashboardModel"/>.
/// </summary>
public partial record MainModel
{
    // Right-cluster status text (static console chrome for this build).
    public string Location => "Singapore";
    public string Weather => "75°F, 11:00 PM";
}

namespace Orbital.Presentation;

public class ShellModel
{
    public ShellModel(INavigator navigator)
    {
        // The navigator is injected for parity with route registration; routing
        // happens via the RouteMap, not by code in this constructor.
        _ = navigator;
    }
}

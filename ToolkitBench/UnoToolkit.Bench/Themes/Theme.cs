using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace UnoToolkit.Bench;

// Lazy-cached references to commonly-accessed theme brushes — avoids the
// hot-path dictionary lookup + cast on every animation/state transition.
internal static class Theme
{
    public static Brush Fg1         => _fg1         ??= Get("Fg1Brush");
    public static Brush Fg2         => _fg2         ??= Get("Fg2Brush");
    public static Brush Fg3         => _fg3         ??= Get("Fg3Brush");
    public static Brush Fg4         => _fg4         ??= Get("Fg4Brush");
    public static Brush Fg5         => _fg5         ??= Get("Fg5Brush");
    public static Brush Bg0         => _bg0         ??= Get("Bg0Brush");
    public static Brush Bg1         => _bg1         ??= Get("Bg1Brush");
    public static Brush Bg2         => _bg2         ??= Get("Bg2Brush");
    public static Brush Bg3         => _bg3         ??= Get("Bg3Brush");
    public static Brush Bg4         => _bg4         ??= Get("Bg4Brush");
    public static Brush Rule        => _rule        ??= Get("RuleBrush");
    public static Brush Accent      => _accent      ??= Get("AccentBrush");
    public static Brush AccentSoft  => _accentSoft  ??= Get("AccentSoftBrush");
    public static Brush Ok          => _ok          ??= Get("OkBrush");
    public static Brush OkSoft      => _okSoft      ??= Get("OkSoftBrush");
    public static Brush DrawerTab   => _drawerTab   ??= Get("DrawerTabGradientBrush");

    private static Brush? _fg1, _fg2, _fg3, _fg4, _fg5;
    private static Brush? _bg0, _bg1, _bg2, _bg3, _bg4;
    private static Brush? _rule, _accent, _accentSoft, _ok, _okSoft, _drawerTab;

    private static Brush Get(string key) => (Brush)Application.Current.Resources[key];
}

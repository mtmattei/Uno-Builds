using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views;

/// <summary>
/// Lazy-cached app-level <see cref="Brush"/> singletons. Looking up a brush
/// via <c>Application.Current.Resources["InkBrush"]</c> is fast (dictionary
/// hit) but not free — each lookup boxes the key, hashes it, and walks the
/// merged dictionary chain. Controls that mutate brushes on every user
/// interaction (PlatformTile selection, hero runtime pill swap, sidebar
/// tab strip active-state, etc.) hit the lookup dozens of times per minute.
/// Caching these references once amortizes the cost to a single resolution
/// per brush per session.
///
/// Safe because the app is light-only (no theme switch); if a future theme
/// flip is added, this needs to be invalidated on theme change.
/// </summary>
internal static class AppBrushes
{
    private static Brush? _ink, _paper, _paper2, _paper3,
                          _ink2, _ink3, _ink4,
                          _hairline, _checkGreen, _transparent,
                          _amber, _indigo;

    public static Brush Ink         => _ink         ??= Resolve("InkBrush");
    public static Brush Paper       => _paper       ??= Resolve("PaperBrush");
    public static Brush Paper2      => _paper2      ??= Resolve("Paper2Brush");
    public static Brush Paper3      => _paper3      ??= Resolve("Paper3Brush");
    public static Brush Ink2        => _ink2        ??= Resolve("Ink2Brush");
    public static Brush Ink3        => _ink3        ??= Resolve("Ink3Brush");
    public static Brush Ink4        => _ink4        ??= Resolve("Ink4Brush");
    public static Brush Hairline    => _hairline    ??= Resolve("HairlineBrush");
    public static Brush CheckGreen  => _checkGreen  ??= Resolve("CheckGreenBrush");
    public static Brush Transparent => _transparent ??= new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    public static Brush Amber       => _amber       ??= Resolve("AmberBrush");
    public static Brush Indigo      => _indigo      ??= Resolve("IndigoBrush");

    private static Brush Resolve(string key) => (Brush)Application.Current.Resources[key];
}

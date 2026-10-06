using Microsoft.UI.Xaml;
using Windows.Storage;

namespace FreewriteUno.Services;

public sealed class SettingsStore : ISettingsStore
{
    private const string ThemeKey = "Theme";

    public ElementTheme Theme
    {
        get
        {
            var values = ApplicationData.Current.LocalSettings.Values;
            if (values.TryGetValue(ThemeKey, out var raw) && raw is string s
                && Enum.TryParse<ElementTheme>(s, out var parsed))
            {
                return parsed;
            }
            return ElementTheme.Light;
        }
        set
        {
            ApplicationData.Current.LocalSettings.Values[ThemeKey] = value.ToString();
        }
    }
}

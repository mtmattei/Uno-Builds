using Microsoft.UI.Xaml;

namespace FreewriteUno.Services;

public interface ISettingsStore
{
    ElementTheme Theme { get; set; }
}

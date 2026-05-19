using System;
using Microsoft.UI.Xaml.Data;

namespace Composer.Views;

public sealed class PlatformKindToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is PlatformKind kind ? kind.DisplayName().ToUpperInvariant() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

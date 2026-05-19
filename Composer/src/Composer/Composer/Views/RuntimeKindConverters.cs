using System;
using Microsoft.UI.Xaml.Data;

namespace Composer.Views;

public sealed class RuntimeKindToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is RuntimeKind kind ? kind.DisplayName() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class RuntimeKindToShortGlyphConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is RuntimeKind kind ? kind.ShortGlyph() : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}


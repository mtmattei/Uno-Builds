using System;
using Microsoft.UI.Xaml.Media;

namespace FieldOpsPro.Presentation.Utils
{
    public static class ColorUtils
    {
        public static Windows.UI.Color ParseColor(string hex, double opacity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Microsoft.UI.Colors.Transparent;
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                byte a = (byte)(255 * Math.Max(0, Math.Min(1.0, opacity)));
                return Windows.UI.Color.FromArgb(a, r, g, b);
            }
            return Microsoft.UI.Colors.Transparent;
        }

        public static SolidColorBrush ToBrush(string hex, double opacity = 1.0)
        {
            return new SolidColorBrush(ParseColor(hex, opacity));
        }
    }
}

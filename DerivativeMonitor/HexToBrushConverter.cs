using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DerivativeMonitor
{
    // Converts a hex string ("#RRGGBB") into a Brush for Fill/Background/Foreground.
    public class HexToBrushConverter : IValueConverter
    {
        // value  = the bound hex string (e.g. the TextBox's Text)
        // return = a Brush WPF can paint with
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hex = value as string;
            if (string.IsNullOrWhiteSpace(hex))
                return System.Windows.Media.Brushes.Transparent;

            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                return new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                // half-typed or invalid hex (e.g. "#FF") — don't crash, just show nothing
                return System.Windows.Media.Brushes.Transparent;
            }
        }

        // Not needed: the swatch/preview only READ the color (one-way binding).
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Windows.Data.Binding.DoNothing;
    }
}
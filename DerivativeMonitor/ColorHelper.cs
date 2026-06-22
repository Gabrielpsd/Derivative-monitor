public static class ColorHelper
{
    public static System.Windows.Media.Brush FromHex(string hex)
    {
        try
        {
            return (System.Windows.Media.SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFrom(hex)!);
        }
        catch
        {
            return System.Windows.Media.Brushes.Transparent; // fallback safety
        }
    }
}

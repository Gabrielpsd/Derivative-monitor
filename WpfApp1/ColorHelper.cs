using System.Windows.Media;

public static class ColorHelper
{
    public static Brush FromHex(string hex)
    {
        try
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex)!);
        }
        catch
        {
            return Brushes.Transparent; // fallback safety
        }
    }
}

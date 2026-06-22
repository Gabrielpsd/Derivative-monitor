using System.Windows;
public class CreateCellStyleHelper
{

    public static Style CreateCellStyle(System.Windows.Media.Brush normalBrush)
    {
        var style = new Style(typeof(System.Windows.Controls.DataGridCell));

        // Normal background
        style.Setters.Add(
            new Setter(
                System.Windows.Controls.DataGridCell.BackgroundProperty,
                normalBrush));

        style.Setters.Add(
            new Setter(
                System.Windows.Controls.DataGridCell.BorderThicknessProperty,
                new Thickness(0.5)));

        style.Setters.Add(
            new Setter(
                System.Windows.Controls.DataGridCell.BorderBrushProperty,
                System.Windows.Media.Brushes.Gray));

        return style;
    }
}

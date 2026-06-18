using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
public class CreateCellStyleHelper
{

    public static Style CreateCellStyle(Brush normalBrush, Brush alertBrush)
    {
        var style = new Style(typeof(DataGridCell));

        // Normal background
        style.Setters.Add(
            new Setter(
                DataGridCell.BackgroundProperty,
                normalBrush));

        style.Setters.Add(
            new Setter(
                DataGridCell.BorderThicknessProperty,
                new Thickness(0.5)));

        style.Setters.Add(
            new Setter(
                DataGridCell.BorderBrushProperty,
                Brushes.Gray));

        // ALERT TRIGGER
        var trigger = new DataTrigger
        {
            Binding = new Binding("IsAlert"),
            Value = true
        };

        trigger.Setters.Add(
            new Setter(
                DataGridCell.BackgroundProperty,
               alertBrush));

        style.Triggers.Add(trigger);

        return style;
    }
}

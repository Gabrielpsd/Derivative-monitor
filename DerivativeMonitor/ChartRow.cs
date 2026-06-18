using System.ComponentModel;

public class ChartRow : INotifyPropertyChanged
{
    public decimal Strike { get; set; }
    public string Parameter { get; set; } = "money_br";

    private double _callValue;
    public double CallValue
    {
        get => _callValue;
        set
        {
            _callValue = value;
            OnPropertyChanged(nameof(CallValue));
        }
    }

    private double _putOption;
    public double PutValue
    {
        get => _putOption;
        set
        {
            _putOption = value;
            OnPropertyChanged(nameof(PutValue));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

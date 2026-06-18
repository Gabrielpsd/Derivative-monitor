using System.ComponentModel;

public class StockData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _lastPrice;

    private string? _openingPrice;

    public string OpeningPrice
    {
        get => _openingPrice != null ? _openingPrice : "0";
        set
        {
            _openingPrice = value;
            OnPropertyChanged(nameof(OpeningPrice));
        }
    }
    public string LastPrice
    {
        get => _lastPrice != null ? _lastPrice : "0";
        set
        {
            _lastPrice = value;
            OnPropertyChanged(nameof(LastPrice));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

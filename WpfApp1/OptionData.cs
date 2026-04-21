using System.Collections.ObjectModel;
using System.ComponentModel;

public class OptionData : INotifyPropertyChanged
{
    public string? ISIN { get; set; }
    public string? Especificacao { get; set; }
    public string? Codigo { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal PrecoExercicio { get; set; }
    public string? Referencia { get; set; }
    public bool Protegida { get; set; }
    public string? Estilo { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class Options
{
    public ObservableCollection<OptionData>? CallOptions { get; set; } = new() { };
    public ObservableCollection<OptionData>? PutOptions { get; set; } = new() { };
}

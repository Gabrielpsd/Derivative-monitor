using System.Collections.ObjectModel;
using System.ComponentModel;

/* This object represents the data structure for an option that comes from the B3 website, 
 * including properties such as ISIN, specification, code, 
 * expiration date, exercise price, reference, protection status, 
 * and style. It implements the INotifyPropertyChanged interface to allow for property change notifications, 
 * which is useful for data binding in WPF applications. Not all the information is used in the program, but is coming from the request anyway
 * so for now is being maintened in the class. */
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

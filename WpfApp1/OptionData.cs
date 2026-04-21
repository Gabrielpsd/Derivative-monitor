using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class OptionData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Helper method to raise the PropertyChanged event
    protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    //codigo ISIN
    private string? _isin;
    public string? ISIN {
        get => _isin;
        set { _isin = value; OnPropertyChanged(); } 
    }

    //especificação
    private string? _Especificacao;
    public string? Especificacao
    {
        get => Especificacao;
        set { Especificacao = value; OnPropertyChanged(); }
    }
    //codigo 
    private string? _Codigo;
    public string? Codigo
    {
        get => _Codigo;
        set { _Codigo = value; OnPropertyChanged(); }
    }

    //Data de vencimento
    private DateTime _Vencimento;
    public DateTime Vencimento
    {
        get => _Vencimento;
        set { _Vencimento = value; OnPropertyChanged(); }
    }

    // preco de exercício
    private decimal _PrecoExercicio;
    public decimal PrecoExercicio { 
        get => _PrecoExercicio;
        set { _PrecoExercicio = value; OnPropertyChanged(); }
    }

    //referencia
    private string? _Referencia;
    public string? Referencia
    {
        get => _Referencia;
        set { _Referencia = value; OnPropertyChanged(); }
    }

    // proteção
    private bool _protegida;
    public bool Protegida
    {
        get => _protegida;
        set { _protegida = value; OnPropertyChanged(); }
    }
    //estilo (AMERI ou EURO)
    private string? _Estilo;
    public string? Estilo
    {
        get => _Estilo;
        set { _Estilo = value; OnPropertyChanged(); }
    }

}

public class Options
{
    public ObservableCollection<OptionData>? CallOptions { get; set; }
    public ObservableCollection<OptionData>? PutOptions { get; set; }
}

public class AppConfig
{
    // ticker do ativo que se deseja obter as opções, ex: AAPL, MSFT, etc.
    public string Ticker { get; set; } = "";

    // nome da tabela onde estão as opções de venda (put) no site, ex: "puts-table", "put-options-table", etc.
    public string PutOptionTableOnWeb { get; set; } = "";

    // nome da tabela onde estão as opções de compra (call) no site, ex: "calls-table", "call-options-table", etc.
    public string CallOptionTableOnWeb { get; set; } = "";

    // URL base do site onde as opções estão listadas, ex: "https://finance.yahoo.com/quote/AAPL/options"
    public string BaseUrl { get; set; } = "";

    // caminho do banco de dados onde as opções serão salvas, ex: "options.db", "C:\\data\\options.db", etc.
    public string DatabasePath { get; set; } = "";

    // No sistema do Profit os ativos são identificados por um código que é composto pelo ticker do ativo seguido de um sufixo específico para opções,
    // ex: "PETR" + "_B_0" = "PETR_B_0" para opções da Apple. O sufixo pode variar dependendo do sistema ou corretora, então é importante configurá-lo corretamente para garantir que as opções sejam identificadas corretamente no banco de dados e no sistema de negociação.
    public string TicketSuffix { get; set; } = "";

    // No Profit o sufixo das opções é diferente do sufixo das ações
    public string DerivativesSuffix { get; set; } = "";

    // intervalo de tempo em milisegundos para atualizar os dados das opções, ex: 60000 para atualizar a cada 60 segundos.
    public int RefreshIntervalMilliseconds { get; set; } = 60000;

    // intervalo de tempo em minutos para buscar novas opções, ex: 60 para buscar novas opções a cada 60 minutos.
    public int ScraperIntervalMinutes { get; set; } = 60;

    // opção para habilitar ou desabilitar logs das opções (opções retornadas no WebScraping), ex: true para habilitar logs detalhados, false para desabilitar.
    public bool debugOptions { get; set; } = false;

    // opção para habilitar ou desabilitar logs dos passos do processo (início do scraping, início da atualização, etc.), ex: true para habilitar logs detalhados, false para desabilitar.
    public bool debugSteps { get; set; } = false;

    public Dictionary<string, string> CallParametersToMonitor { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, string> PutParametersToMonitor { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, string> FieldFormats { get; set; } = new Dictionary<string, string>();

    public UiColors Colors { get; set; } = new UiColors();

    public Dictionary<string, decimal> AlertThresholdsPercentage { get; set; } = new Dictionary<string, decimal>();
    public Dictionary<string, string> Chart { get; set; } = new Dictionary<string, string>();
}

public class UiColors
{   public string OpenPrice { get; set; } = "#4CAF50";     // Green
    public string ClosePrice { get; set; } = "#F44336";     // Red
    public string StrikeColumn { get; set; } = "#D3D3D3"; // LightGray

    public string StrikeFontColor { get; set; } = "#000000"; // Black
    public string StrikeFontColorBar { get; set; } = "#000000"; // Black
    public string CallLine { get; set; } = "#E0E0E0";     // Light border

    public string CallFontColor { get; set; } = "#000000"; // Black
    public string CallFontColorBar { get; set; } = "#000000"; // Black
    public string PutLine { get; set; } = "#E0E0E0";     // Light border

    public string PutFontColor { get; set; } = "#000000"; // Black
    public string PutFontColorBar { get; set; } = "#000000"; // Black
    public string GridLine { get; set; } = "#E0E0E0";     // Light border

    public string ChartCall { get; set; } = "#4CAF50";     // Green

    public string ChartPut { get; set; } = "#F44336";     // Red

    public string ChartActualPrice { get; set; } = "#2196F3";     // Blue

}
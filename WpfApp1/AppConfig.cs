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

    // intervalo de tempo em milisegundos para atualizar os dados das opções, ex: 60000 para atualizar a cada 60 segundos.
    public int RefreshIntervalMilliseconds { get; set; } = 60000;

    // intervalo de tempo em minutos para buscar novas opções, ex: 60 para buscar novas opções a cada 60 minutos.
    public int ScraperIntervalMinutes { get; set; } = 60;
}

public static class ProgramInterface
{
    public static string readTicker()
    {
        string? ticker;

        do
        {
            Console.Write("Enter a ticker: ");
            ticker = Console.ReadLine();
            if (ticker == null || ticker.Length != 4)
            {
                Console.WriteLine("****--------------- Ticket digitado invalido ---------------------**** ");
                Console.WriteLine("Formatos (exemplos):    PETR   |    RANI   |    BBAS |   ITUB    |");

            }
            else
                break;

        } while (true);

        return ticker;
    }
    public static async Task<Options> PopulatesOptionObject(string ticker, AppConfig config)
    {
        var URLRequest = $"{config.BaseUrl}{ticker}";
        // requesting data form web page
        Logger.Log("Requesting data from URL: " + URLRequest);
        var data = await Web_Scrapper.GetData(URLRequest);
        Logger.Log("Data received from URL: " + data.Substring(0, Math.Min(100, data.Length)) + "..."); // Log the first 100 characters of the data
        var options = OptionMapper.extractDataFromHTML(data, config.PutOptionTableOnWeb, config.CallOptionTableOnWeb);
        Logger.Log("Options data extracted successfully for ticker: " + ticker);

        return options;
    }

    public static void printOptions(Options options)
    {
        if (options.CallOptions != null)
            foreach (var callOption in options.CallOptions)
            {
                Console.WriteLine($"Call Option: {callOption.Codigo}, Price: {callOption.PrecoExercicio}");
            }
        if (options.PutOptions != null)
            foreach (var putOption in options.PutOptions)
            {
                Console.WriteLine($"Put Option: {putOption.Codigo}, Price: {putOption.PrecoExercicio}");
            }
    }
}

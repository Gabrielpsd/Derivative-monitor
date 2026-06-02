using System.Collections.ObjectModel;
/* Some maniputalations shoulb be done on the program , such manipualtes the HTML and the returns of the RTD 
 * the role of this class is modify the data and let it useful for the other classes of the program */
public static class OptionMapper
{
    /* This function shoul be used to extract the data within the HTML request made from the site that have all the options listed, 
     * the function receives the HTML as a string and the name of the tables where the call and put options are listed, 
     * then it returns an Options object with two lists of OptionData, one for call options and another for put options */
    public static Options extractDataFromHTML(string data, string CallOptionTableName, string PutOptionTableName)
    {

        Logger.Log("Extracting call options from HTML...");

        var table = Web_Scrapper.ExtractTableById(data, CallOptionTableName);
        var CallOptionsList = Web_Scrapper.MapToOptions(table);

        Logger.LogOptions(CallOptionsList, "Call Options");
        table = Web_Scrapper.ExtractTableById(data, PutOptionTableName);
        var PutOptionsList = Web_Scrapper.MapToOptions(table);

        Logger.LogOptions(PutOptionsList, "Put Options");

        Options options = new Options
        {
            CallOptions = new ObservableCollection<OptionData>(CallOptionsList),
            PutOptions = new ObservableCollection<OptionData>(PutOptionsList)
        };

        return options;
    }

    /* The options should be showed gruped by the strike price, so this function receives an Options object and returns a list of OptionRow, 
     * where each OptionRow represents a strike price and contains the call and put options that have that strike price */
    public static List<OptionsMonitored> BuildOptionRows(Options options, AppConfig _appconfig)
    {
        Logger.Log("Building option rows from options...");
        var dict = new Dictionary<decimal, OptionsMonitored>();

        // CALLS
        if (options.CallOptions != null)
        {
            foreach (var call in options.CallOptions)
            {
                if (!dict.TryGetValue(call.PrecoExercicio, out var row))
                {
                    row = new OptionsMonitored { Strike = call.PrecoExercicio };
                    dict[call.PrecoExercicio] = row;
                }

                row.CallCodigo = call.Codigo;

            }
        }

        // PUTS
        if (options.PutOptions != null)
        {
            foreach (var put in options.PutOptions)
            {
                if (!dict.TryGetValue(put.PrecoExercicio, out var row))
                {
                    row = new OptionsMonitored { Strike = put.PrecoExercicio };
                    dict[put.PrecoExercicio] = row;
                }

                row.PutCodigo = put.Codigo;
            }
        }

        Logger.Log("All the option rows: " + string.Join(", ", dict.Values.Select(r => $"Strike: {r.Strike}, Call: {r.CallCodigo}, Put: {r.PutCodigo}")));
        return dict.Values
                   .OrderBy(x => x.Strike)
                   .ToList();
    }
}
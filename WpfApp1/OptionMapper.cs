using System;
using System.Collections.ObjectModel;

public static class OptionMapper
{
    public static Options extractDataFromHTML(string data,string CallOptionTableName, string PutOptionTableName)
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
}
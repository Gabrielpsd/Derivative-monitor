using System.IO;

public static class Logger
{
    private static readonly string _filePath = "debug.log";
    private static Boolean LoggOptions = true;
    private static Boolean Logging = true;

    public static void configureLogging(bool LogOptions, bool logSteps)
    {
        Logger.Log($"Logging configured. LogOptions: {LogOptions}, LogSteps: {logSteps}");

        LoggOptions = LogOptions;
        Logging = logSteps;

    }
    public static void Log(string message)
    {
        if (Logging is false)
            return;

        var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

        File.AppendAllText(_filePath, logLine + Environment.NewLine);
    }

    public static void LogOptions(this IEnumerable<OptionData> options, string label)
    {
        if (LoggOptions is false)
            return;

        var list = options as IList<OptionData> ?? options.ToList();

        Logger.Log($"--- {label} ({list.Count} items) ---");

        foreach (var option in list)
        {
            Logger.Log($"{option.Codigo.ToString()}|{option.PrecoExercicio.ToString()}|{option.Vencimento.ToString()}");
        }

        Logger.Log($"--- End of {label} ---");
    }
}
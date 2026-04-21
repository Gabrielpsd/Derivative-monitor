using System;
using System.IO;
using System.Text.Json;

public static class ConfigManager
{
    private const string FilePath = "config.json";

    public static AppConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            var defaultConfig = new AppConfig();

            var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);

            Console.WriteLine("Config file created. Please fill it and restart the application.");

            return defaultConfig;
        }

        var fileContent = File.ReadAllText(FilePath);

        var config = JsonSerializer.Deserialize<AppConfig>(fileContent) ?? new AppConfig();

        return config;
    }

    public static void Validate(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Ticker))
            throw new Exception("Ticker is missing in config.json");

        if (string.IsNullOrWhiteSpace(config.PutOptionTableOnWeb))
            throw new Exception("PutOptionTableOnWeb is missing in config.json");

        if (string.IsNullOrWhiteSpace(config.CallOptionTableOnWeb))
            throw new Exception("CallOptionTableOnWeb is missing in config.json");

        if (string.IsNullOrWhiteSpace(config.BaseUrl))
            throw new Exception("BaseUrl is missing in config.json");

        if (string.IsNullOrWhiteSpace(config.DatabasePath))
            throw new Exception("DatabasePath is missing in config.json");
    }
}

public static class MockDataGenerator
{
    private static Random _rand = new();

    public static List<OptionsMonitored> Generate(int count, Dictionary<string, string> parameters)
    {
        var list = new List<OptionsMonitored>();

        decimal baseStrike = 20;

        for (int i = 0; i < count; i++)
        {
            var strike = baseStrike + i;

            var row = new OptionsMonitored
            {
                Strike = strike,
                CallCodigo = $"CALL{strike}",
                PutCodigo = $"PUT{strike}"
            };


            list.Add(row);
        }

        return list;
    }

    private static string RandomValue(string param)
    {
        return param switch
        {
            "ULT" => (_rand.NextDouble() * 10).ToString("0.00"),
            "ABE" => (_rand.NextDouble() * 10).ToString("0.00"),
            "FEC" => (_rand.NextDouble() * 10).ToString("0.00"),
            "DAT" => DateTime.Now.ToString("dd/MM"),
            "HOR" => DateTime.Now.ToString("HH:mm:ss"),
            _ => _rand.Next(1, 1000).ToString()
        };
    }
}
using System;
using RTDTrading;
public class RTDClient
{
    private IRtdServer? _rtdServer;

    private int _serverState = -1;

    private readonly RtdUpdateEvent _updateEvent = new();
    public bool IsRtdConnected => _serverState == 1;

    public RTDClient() 
    {
        //empty constructor
    }

    public async Task InitializeRtdServerAsync()
    {

        try
        {
            // Create the RTD server
            _rtdServer = new RtdServer();

            // Start RTD server
            _serverState = _rtdServer.ServerStart(_updateEvent);

        }
        catch (Exception e)
        {
            throw new Exception("RTD server initialization failed");


        }
    }
    public MarketData? RequestDataFromRtd(string ticker)
    {
        if (_rtdServer == null) return null;

        object? results;
        // Abertura do Dia Atual
        Array topicAbe = new object[] { ticker, "ABE" };
        results = _rtdServer.ConnectData(901, topicAbe, false);
        string openPrice = results.ToString() ?? "Indisponível";

        // Fechamento do Dia Anterior
        Array topicFec = new object[] { ticker, "FEC" };
        results = _rtdServer.ConnectData(902, topicFec, false);
        string closePrice = results.ToString() ?? "Indisponível";

        // Último Preço do Dia Atual
        Array topicUlt = new object[] { ticker, "ULT" };
        results = _rtdServer.ConnectData(903, topicUlt, true);
        string lastPrice = results.ToString() ?? "Indisponível";

        // Média Móvel
        Array topicMa = new object[] { ticker, "3" };
        results = _rtdServer.ConnectData(904, topicMa, true);
        string movingAverage = results.ToString() ?? "Indisponível";

        MarketData marketData = new()
        {
            Ticker = ticker,
            OpenPrice = openPrice,
            ClosePrice = closePrice,
            LastPrice = lastPrice,
            MovingAverage = movingAverage
        };

        return marketData;
    }

    public MarketData? UpdateRtdData()
    {
        if (_rtdServer == null) return null;

        Console.WriteLine("Im here");
        int topicsCount = 0;
        object[,]? results = (object[,])_rtdServer.RefreshData(903);
        Console.WriteLine(results);
        Console.WriteLine("Im here (0)");

        if (topicsCount == 0) return null;
        Console.WriteLine("Im here (1)");

        if (results.GetLength(1) <= 0) return null;
        Console.WriteLine("Im here (2)");

        string last = string.Empty, ma = string.Empty;

        for (int columnIndex = 0; columnIndex < results.GetLength(1); columnIndex++)
        {
            
        Console.WriteLine("Im here(3)");

            object topicId = results[0, columnIndex];
            object topicValue = results[1, columnIndex];

            switch ((int)topicId)
            {
                case 903:
                    last = topicValue.ToString() ?? string.Empty;
                    break;
                case 904:
                    ma = topicValue.ToString() ?? string.Empty;
                    break;
            }
        }

        MarketData marketData = new()
        {
            LastPrice = last,
            MovingAverage = ma
        };

        return marketData;

    }

}

public class RtdUpdateEvent : IRTDUpdateEvent
{
    public long Count { get; set; }
    public int HeartbeatInterval { get; set; }

    public RtdUpdateEvent()
    {
        // Do not call the RTD Heartbeat() method.
        HeartbeatInterval = 100;
    }

    public void Disconnect()
    {
        // Do nothing.
    }

    public void UpdateNotify()
    {
        Count++;
    }
}
public class MarketData
    {
        public string? Ticker { get; set; }

        public string? OpenPrice { get; set; }

        public string? ClosePrice { get; set; }

        public string? LastPrice { get; set; }

        public string? MovingAverage { get; set; }
    }
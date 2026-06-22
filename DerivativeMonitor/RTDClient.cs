using RTDTrading;
using System.Collections.ObjectModel;

public class RTDClient
{
    private IRtdServer? _rtdServer;

    private int _serverState = -1;

    private readonly RtdUpdateEvent _updateEvent = new();
    public bool IsRtdConnected => _serverState == 1;

    private Dictionary<int, TopicBinding> _topicBindings = new();
    public RTDClient()
    {
        //empty constructor
    }

    public async Task InitializeRtdServerAsync()
    {


        if (_rtdServer != null && _serverState == 1)
        {
            Logger.Log("RTD server already running.");
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                _rtdServer = new RtdServer();
                _rtdServer.ServerStart(_updateEvent);
                _serverState = 1;
                Logger.Log("RTD server initialized successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to initialize RTD server: {ex.Message}");
                return false;
            }
        });
    }

    public async Task StopAsync()
    {
        if (_rtdServer == null)
            return;

        await Task.Run(() =>
        {
            _rtdServer.ServerTerminate();
            _rtdServer = null;
            _serverState = -1;
        });
    }

    public async Task ConnectDataToRTd(List<OptionsMonitored> optionsToConnect, AppConfig _appConfig, Dictionary<decimal, ChartRow> _chartLookup, StockData _stockData)
    {
        if (_rtdServer == null) return;

        int topicId = 3; // Start topic IDs from 3

        object? result;
        Array topicStock = new object[] { _appConfig.Ticker + _appConfig.TicketSuffix, "ABE" };
        result = _rtdServer.ConnectData(1, topicStock, true);
        _stockData.OpeningPrice = FieldFormatter.Format(result.ToString(), "money_br");

        topicStock = new object[] { _appConfig.Ticker + _appConfig.TicketSuffix, "ULT" };
        result = _rtdServer.ConnectData(2, topicStock, true);
        _stockData.LastPrice = FieldFormatter.Format(result.ToString(), "money_br");

        foreach (var option in optionsToConnect)
        {
            _chartLookup[option.Strike] = new ChartRow{ Strike = option.Strike };

            if (!string.IsNullOrEmpty(option.CallCodigo))
                topicId = ConnectSide(option, isCall: true, _appConfig, _chartLookup, topicId);

            if (!string.IsNullOrEmpty(option.PutCodigo))
                topicId = ConnectSide(option, isCall: false, _appConfig, _chartLookup, topicId);

        }
    }

    private int ConnectSide(OptionsMonitored option, bool isCall, AppConfig appConfig, Dictionary<decimal, ChartRow> chartLookup, int topicId)
    {

        string side = isCall ? option.CallCodigo : option.PutCodigo;
        // these are the parameters that should ploted on chart
        var parametersToMonitor = isCall ? appConfig.CallParametersToMonitor : appConfig.PutParametersToMonitor;
        var topics = isCall ? option.CallTopics : option.PutTopics;
        var parameters = isCall ? option.CallParameters : option.PutParameters;

        parameters.Clear();
        topics.Clear();

        foreach(var parameter in parametersToMonitor)
        {
            Array topic = new object[] { side + appConfig.DerivativesSuffix, parameter.Value };
            object? raw = _rtdServer!.ConnectData(topicId, topic, true);
            string formatted = FieldFormatter.Format(raw?.ToString(), appConfig.FieldFormats[parameter.Value]);

            parameters[parameter.Value] = formatted;
            topics[topicId.ToString()] = parameter.Value;

            if (parameter.Value == appConfig.Chart["Parameter"].ToString())
            {
                var chartRow = chartLookup[option.Strike];
                chartRow.Parameter = parameter.Value;
                if (isCall) chartRow.CallValue = Convert.ToDouble(formatted);
                else chartRow.PutValue = Convert.ToDouble(formatted);
            }

            _topicBindings[topicId] = new TopicBinding
            {
                Option = option,
                IsCall = isCall,
                Parameter = parameter.Value
            };

            topicId++;
        }

        // Implementation for connecting side
        return topicId;
    }

    public void UpdateRtdData(ObservableCollection<OptionsMonitored> optionsBeingMonitored, AppConfig _appConfig, StockData _stockData)
    {
        if (_rtdServer == null) return;


        int topicsToRefresh = 0;

        object[,]? result_justCallingRefresh = (object[,]?)_rtdServer.RefreshData(ref topicsToRefresh);
        Logger.Log("Called RefreshData with topic ID: 0 to refresh all topics, RTD result: " + result_justCallingRefresh);
        Logger.Log($"Number of topics to refresh: {topicsToRefresh}");
        if (topicsToRefresh > 0)
        {
            Logger.Log($"Received some topic: {result_justCallingRefresh}");
            if (result_justCallingRefresh.GetLength(1) != 0)
            {
                for (int columnIndex = 0; columnIndex < result_justCallingRefresh.GetLength(1); columnIndex++)
                {
                    object topicId = Convert.ToInt32(result_justCallingRefresh[0, columnIndex]);
                    object topicValue = result_justCallingRefresh[1, columnIndex]?.ToString() ?? "";

                    Logger.Log($"Received topic ID: {topicId}, value: {topicValue} from RefreshData");
                    if ((int)topicId == 2)
                    {
                        _stockData.LastPrice = FieldFormatter.Format(topicValue.ToString(), _appConfig.FieldFormats["money_br"]);
                        Logger.Log($"Updated stock price with value: {_stockData.LastPrice}");
                    }
                    else if (_topicBindings.TryGetValue((int)topicId, out var binding))
                    {
                        if (binding.IsCall)
                        {
                            binding.Option.CallParameters[binding.Parameter] = FieldFormatter.Format(topicValue.ToString(), _appConfig.FieldFormats[binding.Parameter]);
                            Logger.Log($"Updated option {binding.Option.CallCodigo} CallParameters[{binding.Parameter}] with value: {binding.Option.CallParameters[binding.Parameter]}");
                        }
                        else
                        {
                            binding.Option.PutParameters[binding.Parameter] = FieldFormatter.Format(topicValue.ToString(), _appConfig.FieldFormats[binding.Parameter]);
                            Logger.Log($"Updated option {binding.Option.PutCodigo} PutParameters[{binding.Parameter}] with value: {binding.Option.PutParameters[binding.Parameter]}");
                        }
                    }
                    else
                    {
                        Logger.Log($"No binding found for topic ID: {topicId}");
                    }
                }

            }
        }

    }

    public class TopicBinding
    {
        public OptionsMonitored Option { get; set; }

        public bool IsCall { get; set; }

        public string Parameter { get; set; }
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
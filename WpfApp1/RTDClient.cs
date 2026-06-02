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

    public async Task ConnectDataToRTd(List<OptionsMonitored> optionsToConnect, ObservableCollection<OptionsMonitored> optionsBeingMonitored, AppConfig _appConfig, Dictionary<decimal, ChartRow> _chartLookup, ObservableCollection<ChartRow> _chartRows, StockData _stockData)
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
            var chartRow = new ChartRow
            {
                Strike = option.Strike
            };

            _chartRows.Add(chartRow);
            _chartLookup[option.Strike] = chartRow;

            Logger.Log($"Processing option: {option}");
            // Connect Call option
            if (!string.IsNullOrEmpty(option.CallCodigo))
            {
                option.CallParameters = new Dictionary<string, string> { };
                option.CallTopics = new Dictionary<string, string> { };
                foreach (var parameter in _appConfig.CallParametersToMonitor)
                {
                    object? results;
                    Array topic = new object[] { option.CallCodigo + _appConfig.TicketSuffix, parameter.Value };
                    results = _rtdServer.ConnectData(topicId, topic, true);
                    // parameter is a key value pair in format "ParameterName": "FormatType",
                    // like "Abertura":"ABE", and parameter.Value is ABE 
                    option.CallParameters[parameter.Value] = FieldFormatter.Format(results.ToString(), _appConfig.FieldFormats[parameter.Value]);
                    Logger.Log($"Updated option CallParameters with topic ID: {topicId}, value: {option.CallParameters[parameter.Value]}");
                    option.CallTopics[topicId.ToString()] = parameter.Value;

                    if (parameter.Value == _appConfig.Chart["Parameter"].ToString())
                    {
                        _chartLookup[option.Strike].Parameter = parameter.Value;
                        _chartLookup[option.Strike].CallValue = Convert.ToDouble(option.CallParameters[parameter.Value]);
                    }

                    // Store the binding of topic ID to option and parameter for later updates
                    // When the update is called the return value is only the topic ID and the new value,
                    // so we need to know which option and parameter to update
                    _topicBindings[topicId] = new TopicBinding
                    {
                        Option = option,
                        IsCall = true,
                        Parameter = parameter.Value
                    };

                    topicId++;
                }
            }
            // Connect Put option
            if (!string.IsNullOrEmpty(option.PutCodigo))
            {
                option.PutParameters = new Dictionary<string, string> { };
                option.PutTopics = new Dictionary<string, string> { };
                foreach (var parameter in _appConfig.PutParametersToMonitor)
                {
                    object? results;
                    Array topic = new object[] { option.PutCodigo + _appConfig.TicketSuffix, parameter.Value };
                    results = _rtdServer.ConnectData(topicId, topic, true);
                    option.PutParameters[parameter.Value] = FieldFormatter.Format(results.ToString(), _appConfig.FieldFormats[parameter.Value]);
                    Logger.Log($"Updated option PutParameters with topic ID: {topicId}, value: {option.PutParameters[parameter.Value]}");
                    option.PutTopics[topicId.ToString()] = parameter.Value;

                    if (parameter.Value == _appConfig.Chart["Parameter"].ToString())
                    {
                        _chartLookup[option.Strike].Parameter = parameter.Value;
                        _chartLookup[option.Strike].PutValue = Convert.ToDouble(option.PutParameters[parameter.Value]);
                    }

                    _topicBindings[topicId] = new TopicBinding
                    {
                        Option = option,
                        IsCall = false,
                        Parameter = parameter.Value
                    };
                    topicId++;
                }
            }
            optionsBeingMonitored.Add(option);
        }
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
            if (result_justCallingRefresh.GetLength(1) != null)
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
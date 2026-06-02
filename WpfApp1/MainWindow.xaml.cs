using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Options options = new Options();
        private CancellationTokenSource _cts = new();
        private AppConfig _appConfig = new AppConfig();
        private bool loopingIsRunning = false;
        private ObservableCollection<OptionsMonitored> _optionsMonitored { get; set; } = new();
        private RTDClient? _rtdClient = new RTDClient();
        private StockData _stockData = new();
        // when I update the _chartLookup, automatically update the _chartRows collection that is binded to the chart,
        // so I can keep the chart updated with the latest data without having to manually refresh it

        //I'll only handle and modify the _charLookup
        private Dictionary<decimal, ChartRow> _chartLookup = new();

        // the _chartRows is the collection that is binded to the chart, so when I update it, the chart will be updated automatically
        private ObservableCollection<ChartRow> _chartRows = new();

        public Brush GridLineBrush { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BuildDynamicColumns()
        {
            Logger.Log("Building dynamic columns for DataGrid based on monitored parameters...");
            OptionsGrid.Columns.Clear();

            // ------------------------
            // CALL SIDE
            // ------------------------

            Logger.Log("Adding CALL side columns...");
            var reversedParams = _appConfig.CallParametersToMonitor.AsEnumerable().Reverse().ToList();
            var callBrush = ColorHelper.FromHex(_appConfig.Colors.CallLine);
            var putBrush = ColorHelper.FromHex(_appConfig.Colors.PutLine);
            var strikeBrush = ColorHelper.FromHex(_appConfig.Colors.StrikeColumn);
            var alertBrush = ColorHelper.FromHex(_appConfig.Colors.Alert);

            var callStyle = new Style(typeof(TextBlock));

            //callStyle.Setters.Add(new Setter(TextBlock.BackgroundProperty, Brushes.Transparent));
            callStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, ColorHelper.FromHex(_appConfig.Colors.CallFontColor)));
            callStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

            Logger.Log("Adding CALL parameter columns in reverse order...");
            foreach (var param in reversedParams)
            {

                OptionsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = param.Key,
                    Binding = new Binding($"CallParameters[{param.Value}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellStyle = CreateCellStyleHelper.CreateCellStyle(callBrush, alertBrush)
                });
            }

            Logger.Log("Adding CALL code column...");
            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Call Código",
                Binding = new Binding("CallCodigo"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellStyle = CreateCellStyleHelper.CreateCellStyle(callBrush, alertBrush)
            });

            Logger.Log("Adding STRIKE column...");
            var strikeStyle = new Style(typeof(TextBlock));
            strikeStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            strikeStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            strikeStyle.Setters.Add(new Setter(TextBlock.BackgroundProperty, strikeBrush));

            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "STRIKE",
                Binding = new Binding("Strike"),
                Width = new DataGridLength(1.2, DataGridLengthUnitType.Star),
                ElementStyle = strikeStyle
            });


            // ------------------------
            // PUT SIDE
            // ------------------------
            Logger.Log("Adding PUT side columns...");
            var putStyle = new Style(typeof(TextBlock));
            //putStyle.Setters.Add(new Setter(TextBlock.BackgroundProperty, putBrush));
            putStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, ColorHelper.FromHex(_appConfig.Colors.PutFontColor)));
            putStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Put Código",
                Binding = new Binding("PutCodigo"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellStyle = CreateCellStyleHelper.CreateCellStyle(putBrush, alertBrush)
            });

            Logger.Log("Adding PUT parameter columns...");
            foreach (var param in _appConfig.PutParametersToMonitor)
            {

                OptionsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = param.Key,
                    Binding = new Binding($"PutParameters[{param.Value}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellStyle = CreateCellStyleHelper.CreateCellStyle(putBrush, alertBrush)
                });
            }



        }

        private async void ConnectDataLoop(object sender, RoutedEventArgs e)
        {
            if (!loopingIsRunning)
            {
                Logger.Log("Starting loops...");
                if (_rtdClient == null || !_rtdClient.IsRtdConnected)
                {
                    Logger.Log("Loop was called but server is not initialized.");
                    return;
                }

                if (options == null || options.CallOptions == null || options.PutOptions == null || (options.CallOptions.Count == 0 && options.PutOptions.Count == 0))
                {
                    Logger.Log("Connect was called, populating options object");
                    options = await ProgramInterface.PopulatesOptionObject(_appConfig.Ticker, _appConfig);
                }

                var data = OptionMapper.BuildOptionRows(options, _appConfig);
                Logger.Log("Building data rows for RTD connection...");
                Logger.Log("Connecting data to RTD server...");
                await _rtdClient.ConnectDataToRTd(data, _optionsMonitored, _appConfig, _chartLookup, _chartRows, _stockData);
                OptionsGrid.ItemsSource = _optionsMonitored;
                LabelClose.Content = _stockData.LastPrice;
                LabelOpening.Content = _stockData.OpeningPrice;

                ChartHandler.BuildChart(_chartLookup, _appConfig, WpfPlot1, _stockData);

                _cts = new CancellationTokenSource();
                loopingIsRunning = true;
                ConnectDataLoopButtonTextBlock.Foreground = new SolidColorBrush(Colors.White);
                ConnectDataLoopButton.Background = new SolidColorBrush(Colors.Green);
                StartLoops();
            }
            else
            {
                Logger.Log("Stopping loops...");
                StopLoops();
                loopingIsRunning = false;
                ConnectDataLoopButtonTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                ConnectDataLoopButton.Background = new SolidColorBrush(Colors.Gray);
            }
        }

        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            // temporary empty handler
            LoadButton.IsEnabled = false; // prevent spam clicks

            await ShowLoadingAnimation(LoadButton);

            LoadButton.IsEnabled = true;
        }

        private async Task loadDataAsync()
        {
            Logger.Log("Application started. Loading configuration and data...");
            //RTDClient RtdClientObj = new RTDClient();
            _appConfig = ConfigManager.Load();

            Logger.Log("Configuration loaded successfully.");
            ConfigManager.Validate(_appConfig);

            Logger.Log("Configuration validated successfully. Ticker: " + _appConfig.Ticker);
            Logger.configureLogging(_appConfig.debugOptions, _appConfig.debugSteps);
            Logger.Log($"Configuration loaded: {JsonSerializer.Serialize(_appConfig, new JsonSerializerOptions { WriteIndented = true })}");

            TickerInput.Text = _appConfig.Ticker;

            //OptionsGrid.ItemsSource = data;

            Logger.Log("Building dynamic columns for DataGrid based on monitored parameters...");
            BuildDynamicColumns();

            GridLineBrush = ColorHelper.FromHex(_appConfig.Colors.GridLine);

            Logger.Log("Options data populated successfully for ticker: " + _appConfig.Ticker);
            DataBaseManager.Initialize(_appConfig.DatabasePath);

            Logger.Log("Database initialized successfully at path: " + _appConfig.DatabasePath);
            DataBaseManager.SaveOptions(_appConfig.DatabasePath, _appConfig.Ticker, options);

            Logger.Log("Options data saved to database successfully for ticker: " + _appConfig.Ticker);

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _rtdClient?.StopAsync();
            Logger.Log("RTD server disposed");
            StopLoops();
        }
        private async void ServerStart_click(object sender, RoutedEventArgs e)
        {
            // temporary empty handler
            Logger.Log("Starting RTD server...");
            if (_rtdClient == null)
            {
                Logger.Log("RTD client instance is null. Creating new instance...");
                _rtdClient = new RTDClient();
            }

            if (!_rtdClient.IsRtdConnected)
            {
                Logger.Log("RTD server is currently inactive. Activating...");
                await _rtdClient.InitializeRtdServerAsync();
                ConnectServerButtonTextBlock.Text = "\uE7E8";
                ConnectServerButtonTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                Logger.Log("RTD server is currently active. Deactivating...");
                await _rtdClient.StopAsync();
                ConnectServerButtonTextBlock.Text = "\uF5E7";
                ConnectServerButtonTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                StopLoops();
            }


        }

        private async Task ShowLoadingAnimation(Button button, int durationMs = 3000)
        {
            string baseText = "Loading";
            int steps = durationMs / 500; // update every 500ms

            for (int i = 0; i < steps; i++)
            {
                int dots = (i % 3) + 1;
                button.Content = baseText + new string('.', dots);

                await Task.Delay(500);
            }

            button.Content = "Load";
        }

        private async void StartLoops()
        {

            _ = StartRtdLoop();
            _ = StartScraperLoop();
        }

        private async Task StartRtdLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (_rtdClient != null && _rtdClient.IsRtdConnected)
                        _rtdClient.UpdateRtdData(_optionsMonitored, _appConfig, _stockData);

                }
                catch (Exception ex)
                {
                    Logger.Log("RTD loop error: " + ex.Message);
                }

                await Task.Delay(_appConfig.RefreshIntervalMilliseconds, _cts.Token);
            }
        }

        private async Task StartScraperLoop()
        {

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    Logger.Log("Refreshing options from website...");

                    var options = await ProgramInterface.PopulatesOptionObject(_appConfig.Ticker, _appConfig);

                    if (options != null)
                    {

                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Scraper loop error: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromMinutes(_appConfig.ScraperIntervalMinutes), _cts.Token);
            }
        }

        private void StopLoops()
        {
            _cts.Cancel();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await loadDataAsync();
            DataContext = this;
        }
    }


}
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DerivativeMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
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
        private VerticalLine? _priceLine;
        // the _chartRows is the collection that is binded to the chart, so when I update it, the chart will be updated automatically
        private ObservableCollection<ChartRow> _chartRows = new();
        public record LoadStatus(int Percent, string Message);

        // this crosshair is used to show the values of the bars when the user hover them, it is created once and updated with the new values when the user hover another bar, this way I can avoid creating and destroying multiple crosshairs that can cause performance issues
        private ScottPlot.Plottables.Crosshair _hoverCrosshair;
        private List<ChartRow> _orderedChartRows = new(); // bars in the same order as the chart
        private int _lastHoverIndex = -1;                 // for the performance guard below

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void ApplySettings()
        {
            TickerInput.Text = _appConfig.Ticker;
            BuildDynamicColumns();   // columns depend on CallParametersToMonitor + colors
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
                    Binding = new System.Windows.Data.Binding($"CallParameters[{param.Value}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellStyle = CreateCellStyleHelper.CreateCellStyle(callBrush),
                    Foreground = ColorHelper.FromHex(_appConfig.Colors.CallFontColor)
                });
            }

            Logger.Log("Adding CALL code column...");
            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Call Código",
                Binding = new System.Windows.Data.Binding("CallCodigo"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellStyle = CreateCellStyleHelper.CreateCellStyle(callBrush),
                Foreground = ColorHelper.FromHex(_appConfig.Colors.CallFontColor)
            });

            Logger.Log("Adding STRIKE column...");
            var strikeStyle = new Style(typeof(System.Windows.Controls.TextBlock));
            strikeStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.TextAlignmentProperty, System.Windows.TextAlignment.Center));
            strikeStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.FontWeightProperty, System.Windows.FontWeights.Bold));
            strikeStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.BackgroundProperty, strikeBrush));

            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "STRIKE",
                Binding = new System.Windows.Data.Binding("Strike"),
                Width = new DataGridLength(1.2, DataGridLengthUnitType.Star),
                ElementStyle = strikeStyle,
                Foreground = ColorHelper.FromHex(_appConfig.Colors.StrikeFontColor)
            });


            // ------------------------
            // PUT SIDE
            // ------------------------
            Logger.Log("Adding PUT side columns...");
            var putStyle = new Style(typeof(System.Windows.Controls.TextBlock));
            //putStyle.Setters.Add(new Setter(TextBlock.BackgroundProperty, putBrush));
            putStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.ForegroundProperty, ColorHelper.FromHex(_appConfig.Colors.PutFontColor)));
            putStyle.Setters.Add(new Setter(System.Windows.Controls.TextBlock.TextAlignmentProperty, System.Windows.TextAlignment.Center));

            OptionsGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Put Código",
                Binding = new System.Windows.Data.Binding("PutCodigo"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                CellStyle = CreateCellStyleHelper.CreateCellStyle(putBrush),
                Foreground = ColorHelper.FromHex(_appConfig.Colors.PutFontColor)
            });

            Logger.Log("Adding PUT parameter columns...");
            foreach (var param in _appConfig.PutParametersToMonitor)
            {

                OptionsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = param.Key,
                    Binding = new System.Windows.Data.Binding($"PutParameters[{param.Value}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    CellStyle = CreateCellStyleHelper.CreateCellStyle(putBrush),
                    Foreground = ColorHelper.FromHex(_appConfig.Colors.PutFontColor)
                });
            }



        }

        private async void ConnectDataLoop(object sender, RoutedEventArgs e)
        {

                LoadingProgressBar.Value = 0;
                LoadingOverlay.Visibility = Visibility.Visible;

                // Created on the UI thread → every Report(...) runs this lambda back on the UI thread,
                // even when called from inside Task.Run.
                var progress = new Progress<LoadStatus>(status =>
                {
                    LoadingProgressBar.Value = status.Percent;
                    LoadingStatusText.Text = status.Message;
                });

                try
                {
                    await RunLoadPipelineAsync(progress);
                }
                catch (Exception ex)
                {
                    Logger.Log("Load pipeline error: " + ex.Message);
                    LoadingStatusText.Text = "Erro ao carregar: " + ex.Message;
                    await Task.Delay(2500);
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
        }
        private async void ConfigureColoursButton(object sender, RoutedEventArgs e)
        {

            AppConfig draft = ConfigManager.Clone(_appConfig);
            var dialog = new ColourSettingsWindow(draft) { Owner = this };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                ConfigManager.Save(draft);
                _appConfig = draft;
                ApplySettings();
                setColours(_appConfig);
            }
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
            setColours(_appConfig);
            BuildDynamicColumns();

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
        private async Task RunLoadPipelineAsync(IProgress<LoadStatus> progress)
        {
            if (loopingIsRunning) { 
                Logger.Log("Stopping loops before reloading data...");
                progress.Report(new LoadStatus(25, "Parando loops..."));
                StopLoops();
                ConnectionStatusIndicator.Fill = new SolidColorBrush(Colors.Red);
                Status.Text = "RTD Desconectado";
                Status.Foreground = new SolidColorBrush(Colors.Red);
                loopingIsRunning = false;
                ConnectDataLoopButtonTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                ConnectDataLoopButton.Background = new SolidColorBrush(Colors.Gray);
                progress.Report(new LoadStatus(100, "Parada Excutada"));
                return;
            }

            // 1) Ensure the RTD server is up
            progress.Report(new LoadStatus(5, "Iniciando servidor RTD..."));
            if (_rtdClient == null) _rtdClient = new RTDClient();
            if (!_rtdClient.IsRtdConnected) await _rtdClient.InitializeRtdServerAsync();
            if (!_rtdClient.IsRtdConnected)
                throw new InvalidOperationException("Não foi possível iniciar o servidor RTD.");

            // 2) Download the option chain (network I/O)
            progress.Report(new LoadStatus(25, "Baixando cadeia de opções..."));
            options = await ProgramInterface.PopulatesOptionObject(_appConfig.Ticker, _appConfig);

            // 3) Build grid rows (CPU) on a background thread
            progress.Report(new LoadStatus(45, "Montando as linhas de opções..."));
            var rows = await Task.Run(() => OptionMapper.BuildOptionRows(options, _appConfig));

            // 4) Connect topics (heavy COM) on a background thread — this is what used to freeze
            progress.Report(new LoadStatus(65, "Conectando dados ao RTD..."));
            _chartLookup.Clear();
            await Task.Run(() => _rtdClient.ConnectDataToRTd(rows, _appConfig, _chartLookup, _stockData));

            // 5) Fill the UI — we are back on the UI thread after the await, so this is safe
            progress.Report(new LoadStatus(85, "Preenchendo a grade e o gráfico..."));
            _optionsMonitored.Clear();
            foreach (var row in rows) _optionsMonitored.Add(row);
            _chartRows.Clear();
            foreach (var chartRow in _chartLookup.Values) _chartRows.Add(chartRow);

            OptionsGrid.ItemsSource = _optionsMonitored;
            LabelClose.Content = _stockData.LastPrice;
            LabelOpening.Content = _stockData.OpeningPrice;
            Logger.Log("Stock Data" + _stockData);
            ChartHandler.BuildChart(_chartLookup, _appConfig, WpfPlot1, _stockData, _priceLine);
            _orderedChartRows = _chartLookup.OrderBy(x => x.Key).Select(x => x.Value).ToList(); ;

            _hoverCrosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);
            _hoverCrosshair.IsVisible = false;          // hidden until the mouse is over a bar
            _hoverCrosshair.MarkerShape = ScottPlot.MarkerShape.OpenCircle;
            _hoverCrosshair.MarkerSize = 15;
            WpfPlot1.MouseMove += WpfPlot1_MouseMove;
            // 6) Start the live loops
            ConnectionStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            Status.Text = "RTD Conectado";
            Status.Foreground = new SolidColorBrush(Colors.Green);

            progress.Report(new LoadStatus(90, "Iniciando atualizações em tempo real..."));
            StartLoops();
            progress.Report(new LoadStatus(100, "Servidor conectado"));
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

        private void setColours(AppConfig _appConfig)
        {
            LabelClose.Foreground = ColorHelper.FromHex(_appConfig.Colors.ClosePrice);
            LabelOpening.Foreground = ColorHelper.FromHex(_appConfig.Colors.OpenPrice);
            StrikeSelected.Foreground = ColorHelper.FromHex(_appConfig.Colors.StrikeFontColorBar);
            PutValue.Foreground = ColorHelper.FromHex(_appConfig.Colors.PutFontColorBar);
            CallValue.Foreground = ColorHelper.FromHex(_appConfig.Colors.CallFontColorBar);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            AppConfig draft = ConfigManager.Clone(_appConfig);
            var dialog = new SettingsWindow(draft) { Owner = this };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                ConfigManager.Save(draft);
                _appConfig = draft;
                ApplySettings();
            }
        }

        private async void StartLoops()
        {

            _ = StartRtdLoop();
            _ = StartScraperLoop();
            loopingIsRunning = true;
        }

        private async Task StartRtdLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (_rtdClient != null && _rtdClient.IsRtdConnected)
                    { 
                        _rtdClient.UpdateRtdData(_optionsMonitored, _appConfig, _stockData);
                        Decimal.TryParse(
                                _stockData.LastPrice,
                                NumberStyles.Any,
                                new CultureInfo("pt-BR"),
                                out decimal lastPriceDecimal);
                        ChartHandler.UpdatePriceLine(lastPriceDecimal, _chartLookup, _priceLine);
                        ChartHandler.RefreshChart(WpfPlot1);
                        LabelClose.Content = _stockData.LastPrice;
                    }

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

        private void WpfPlot1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Logger.Log("Mouse moved over chart at position: " + e.GetPosition(WpfPlot1));
            Logger.Log("Current hover index: " + _lastHoverIndex);
            Logger.Log("Total chart rows: " + _orderedChartRows.Count);
            if (_orderedChartRows.Count == 0) return;

            // (a) WPF mouse position -> ScottPlot pixel. DisplayScale is the WPF-specific bit.
            System.Windows.Point p = e.GetPosition(WpfPlot1);
            ScottPlot.Pixel mousePixel = new(p.X * WpfPlot1.DisplayScale, p.Y * WpfPlot1.DisplayScale);

            // (b) pixel -> chart coordinates
            ScottPlot.Coordinates mouse = WpfPlot1.Plot.GetCoordinates(mousePixel);

            // (c) bars sit at integer X (0,1,2,...), so the nearest bar is the rounded X
            int index = (int)System.Math.Round(mouse.X);

            // (d) mouse is off any bar -> hide and bail
            if (index < 0 || index >= _orderedChartRows.Count)
            {
                if (_hoverCrosshair.IsVisible)
                {
                    _hoverCrosshair.IsVisible = false;
                    StrikeSelected.Content = "";
                    PutValue.Content = "";
                    CallValue.Content = "";
                    WpfPlot1.Refresh();
                    _lastHoverIndex = -1;
                }
                return;
            }

            // (e) PERFORMANCE GUARD: MouseMove fires constantly. Only redraw when the
            //     bar under the cursor actually changes, not on every pixel of movement.
            if (index == _lastHoverIndex) return;
            _lastHoverIndex = index;

            ChartRow row = _orderedChartRows[index];
            _hoverCrosshair.IsVisible = true;
            _hoverCrosshair.Position = new ScottPlot.Coordinates(index, mouse.Y);
            StrikeSelected.Content = $"R$ {row.Strike}";
            PutValue.Content = $"{row.PutValue:0.##}";
            CallValue.Content = $"{row.CallValue:0.##}";
            WpfPlot1.Refresh();
        }

    }


}
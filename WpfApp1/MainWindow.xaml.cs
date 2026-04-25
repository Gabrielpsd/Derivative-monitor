using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

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

        private RTDClient? _rtdClient = new RTDClient();
        public  MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectDataLoop(object sender, RoutedEventArgs e)
        {
            if (!loopingIsRunning)
            {
                Logger.Log("Starting loops...");
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
            TickerInput.Text = _appConfig.Ticker;
            var optionsRequest = await ProgramInterface.PopulatesOptionObject(_appConfig.Ticker, _appConfig);

            if(optionsRequest != null) {

                if(options.PutOptions != null)
                    options.PutOptions.Clear();
                
                if(options.CallOptions != null)
                    options.CallOptions.Clear();
            
                foreach (var callOption in optionsRequest.CallOptions)
                {
                    options.CallOptions?.Add(callOption);
                }
            
                foreach (var putOption in optionsRequest.PutOptions)
                {
                    options.PutOptions?.Add(putOption);
                }
            }
            
            Logger.Log("Options data populated successfully for ticker: " + _appConfig.Ticker);
            DataBaseManager.Initialize(_appConfig.DatabasePath);

            Logger.Log("Database initialized successfully at path: " + _appConfig.DatabasePath);
            DataBaseManager.SaveOptions(_appConfig.DatabasePath, _appConfig.Ticker, options);

            Logger.Log("Options data saved to database successfully for ticker: " + _appConfig.Ticker);

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _rtdClient?.DisposeRtdServer();
            Logger.Log("RTD server disposed");
            StopLoops();
        }
        private async void ServerStart_click(object sender, RoutedEventArgs e)
        {
            // temporary empty handler
            Logger.Log("Starting RTD server...");
            if(_rtdClient == null)
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
                    var data = _rtdClient.UpdateRtdData();

                    if (data != null)
                    {
                        // update UI here
                        // Example:
                        // UpdateGrid(data);
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

        private async Task UpdatePriceAsync(Label label, string newValue)
        {
            var oldValue = label.Content?.ToString();

            if (oldValue == newValue)
                return;

            label.Content = newValue;

            // Change color (green for example)
            label.Background = new SolidColorBrush(Colors.LightGreen);

            await Task.Delay(3000);

            // Restore original color
            label.Background = new SolidColorBrush(Colors.WhiteSmoke);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await loadDataAsync();
        }
    }


}
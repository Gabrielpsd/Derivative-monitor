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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Options options = new Options();

        public  MainWindow()
        {
            InitializeComponent();
            DataContext = options;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await loadDataAsync();
        }
        private async Task loadDataAsync()
        {
            Logger.Log("Application started. Loading configuration and data...");
            //RTDClient RtdClientObj = new RTDClient();
            var config = ConfigManager.Load();

            Logger.Log("Configuration loaded successfully.");
            ConfigManager.Validate(config);

            Logger.Log("Configuration validated successfully. Ticker: " + config.Ticker);
            TickerInput.Text = config.Ticker;
            var optionsRequest = await ProgramInterface.PopulatesOptionObject(config.Ticker, config);

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

            
            Logger.Log("Options data populated successfully for ticker: " + config.Ticker);
            DataBaseManager.Initialize(config.DatabasePath);

            Logger.Log("Database initialized successfully at path: " + config.DatabasePath);
            DataBaseManager.SaveOptions(config.DatabasePath, config.Ticker, options);

            Logger.Log("Options data saved to database successfully for ticker: " + config.Ticker);

            /*
            if (RtdClientObj == null)
            {
                Console.Error.WriteLine("Error initializing RTD client.");
                return;
            }

            await RtdClientObj.InitializeRtdServerAsync();

            if (!RtdClientObj.IsRtdConnected)
            {
                Console.Error.WriteLine("Error connecting to RTD server.");
                return;
            }

            if (options == null || options.CallOptions == null)
            {
                Console.Error.WriteLine("Error populating options data.");
                return;
            }


            foreach (var item in options.CallOptions)
            {
                var MarketData = RtdClientObj.RequestDataFromRtd(item.Codigo + config.TicketSuffix);

                if (MarketData == null)
                {
                    Console.Error.WriteLine("Error retrieving market data from RTD server.");
                    return;
                }

                
                Console.Write($"Ticker:{MarketData.Ticker} |   ");
                Console.Write($"OpenPrice:{MarketData.OpenPrice} |   ");
                Console.Write($"ClosePrice:{MarketData.ClosePrice} |   ");
                Console.Write($"MarketData:{MarketData.LastPrice} |   ");
                Console.WriteLine($"MovingAverage:{MarketData.MovingAverage} |   ");
                CreateTickerUI(MarketData.Ticker);

            }


            foreach (var item in options.PutOptions)
            {
                var MarketData = RtdClientObj.RequestDataFromRtd(item.Codigo + config.TicketSuffix);

                if (MarketData == null)
                {
                    Console.Error.WriteLine("Error retrieving market data from RTD server.");
                    return;
                }

                
                Console.WriteLine($"Ticker:{MarketData.Ticker} |   ");
                Console.Write($"OpenPrice:{MarketData.OpenPrice} |   ");
                Console.Write($"ClosePrice:{MarketData.ClosePrice} |   ");
                Console.Write($"MarketData:{MarketData.LastPrice} |   ");
                Console.WriteLine($"MovingAverage:{MarketData.MovingAverage} |   ");

                CreateTickerUI(MarketData.Ticker);

            }*/

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

        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            // temporary empty handler
            LoadButton.IsEnabled = false; // prevent spam clicks

            await ShowLoadingAnimation(LoadButton);

            LoadButton.IsEnabled = true;
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
    }

}
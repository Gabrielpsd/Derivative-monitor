using RTDTrading;
using System;
using System.IO;
using System.Reflection.Emit;
using System.Timers;

namespace HelloWorld
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Options options = new Options();
            RTDClient RtdClientObj = new RTDClient();
            var config = ConfigManager.Load();

            ConfigManager.Validate(config);

            string ticker = ProgramInterface.readTicker();

            options = await ProgramInterface.PopulatesOptionObject(ticker, config);

            ProgramInterface.printOptions(options);

            DataBaseManager.Initialize(config.DatabasePath);
            DataBaseManager.SaveOptions(config.DatabasePath,ticker, options);

            if(RtdClientObj == null)
            {
                Console.Error.WriteLine("Error initializing RTD client.");
                return;
            }
            Console.WriteLine("Suffix: " + config.TicketSuffix);

            await RtdClientObj.InitializeRtdServerAsync();
            if(!RtdClientObj.IsRtdConnected)
            {     Console.Error.WriteLine("Error connecting to RTD server.");
                return;
            }

            if(options == null || options.CallOptions == null)
            {
                Console.Error.WriteLine("Error populating options data.");
                return;
            }


            Console.WriteLine($"Calling all Call OPtions");
            
            foreach (var item in options.CallOptions)
            {
                var MarketData = RtdClientObj.RequestDataFromRtd(item.Codigo + config.TicketSuffix);

                if(MarketData == null)
                {
                    Console.Error.WriteLine("Error retrieving market data from RTD server.");
                    return;
                }

            Console.Write($"Ticker:{MarketData.Ticker} |   ");
            Console.Write($"OpenPrice:{MarketData.OpenPrice} |   ");
            Console.Write($"ClosePrice:{MarketData.ClosePrice} |   ");
            Console.Write($"MarketData:{MarketData.LastPrice} |   ");
            Console.WriteLine($"MovingAverage:{MarketData.MovingAverage} |   ");

            }

            Console.WriteLine($"Calling all Put OPtions");

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

            }
            Console.WriteLine("Data Connected");

            Console.ReadLine();

            Console.WriteLine("Data saved to database successfully.");
            Console.ReadLine();
        }

    }
}



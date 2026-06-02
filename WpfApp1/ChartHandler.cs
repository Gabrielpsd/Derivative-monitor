using ScottPlot;
using ScottPlot.WPF;
public static class ChartHandler
{
    public static void BuildChart(Dictionary<decimal, ChartRow> chartLookup, AppConfig _appConfig, WpfPlot wpfPlot, StockData stockData)
    {
        string[] categorieNames = { "Put", "Call" };
        string[] categoryColours = { _appConfig.Colors.ChartPut, _appConfig.Colors.ChartCall };
        ScottPlot.TickGenerators.NumericManual tickGen = new();

        for (int i = 0; i < chartLookup.Count; i++)
        {
            var chartRow = chartLookup.ElementAt(i).Value;
            double[] values = { chartRow.PutValue, chartRow.CallValue };

            if(chartRow.PutValue > 0)
            {
                // creating put value bar
                ScottPlot.Bar PutBar = new()
                {
                    Value = chartRow.PutValue,
                    FillColor = new Color(_appConfig.Colors.ChartPut),
                    ValueBase = 0,
                    Position = i,
                };

                wpfPlot.Plot.Add.Bar(PutBar);
            }

            if (chartRow.CallValue > 0)
            {
                // creating call value bar
                ScottPlot.Bar CallBar = new()
                {
                    Value = chartRow.CallValue,
                    FillColor = new Color(_appConfig.Colors.ChartCall),
                    ValueBase = chartRow.PutValue,
                    Position = i,
                };

            wpfPlot.Plot.Add.Bar(CallBar);
            }

            tickGen.AddMajor(i, FieldFormatter.Format(chartRow.Strike.ToString(), _appConfig.FieldFormats[chartRow.Parameter])); ;
        }

        wpfPlot.Plot.Axes.Bottom.TickGenerator = tickGen;
        var axLine = wpfPlot.Plot.Add.VerticalLine(Double.TryParse(stockData.OpeningPrice, out double openingPrice) ? openingPrice : 0);
        axLine.Color = new Color(_appConfig.Colors.ChartActualPrice);
        axLine.Text = $"{stockData.OpeningPrice}";
        axLine.LabelAlignment = Alignment.MiddleRight;

    }
}

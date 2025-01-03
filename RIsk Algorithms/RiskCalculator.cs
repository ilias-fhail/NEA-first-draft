using APIcalls;
using static APIcalls.AlphaVantage;
namespace RIsk_Algorithms
{
    public class RiskCalculator
    {
        async Task<double> StockVariance(string symbol)
        {
            string apiKey = "MUY2SK806D7KE5WM";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            List<StockData2> stockDataList = await alphaVantage.FetchStockData(symbol);

            double total = 0;
            double average = 0;
            int count = 0;
            double variance = 0;
            double tempVar = 0;

            foreach (var stockData in stockDataList)
            {
                total = total + stockData.Close;
                count++;
            }
            average = total / count;
            foreach (var stockData in stockDataList)
            {
                tempVar =tempVar +(( stockData.Close - average)* (stockData.Close - average)) ;
            }
            variance = 1/(count-1) * tempVar;
            return variance;
        }
    }
}

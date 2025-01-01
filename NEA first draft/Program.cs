using StockProSim.Data;
using APIcalls;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using static APIcalls.AlphaVantage;

namespace NEA_first_draft
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Enter a stock ticker symbol (e.g., AAPL, MSFT):");
            //string ticker = Console.ReadLine();

            //await APICalls.FetchAndDisplayStockInfo(ticker);
            MyServerDb db = new MyServerDb();
            var watchlist = await db.GetWatchlistAsync();
            foreach (var item in watchlist)
            {
                Console.WriteLine(item);
            }
            try
            {
                var apiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
                var historicalData = await apiCalls.FetchHistoricalDataAsync("TSLA");

                Console.WriteLine($"Historical Data for Tesla (Last 2 Years):");
                foreach ((DateTime Date, decimal ClosePrice) in historicalData)
                {
                    Console.WriteLine($"{Date:yyyy-MM-dd}: {ClosePrice:C}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            string apiKey = "MUY2SK806D7KE5WM";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            string symbol = "AAPL";

            List<StockData2> stockDataList = await alphaVantage.FetchDailyStockDataForLast300Days(symbol);

            foreach (var stockData in stockDataList)
            {
                Console.WriteLine($"{stockData.Date:yyyy-MM-dd}: Open={stockData.Open}, High={stockData.High}, Low={stockData.Low}, Close={stockData.Close}, Volume={stockData.Volume}");
            }
        }
    }
}

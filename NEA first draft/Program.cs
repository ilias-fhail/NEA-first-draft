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
            string apiKey = "MUY2SK806D7KE5WM";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            string symbol = "AAPL";

            List<StockData2> stockDataList = await alphaVantage.FetchStockData(symbol);
            int count = 0;
            foreach (var stockData in stockDataList)
            {
                Console.WriteLine($"Close={stockData.Close}");
                count ++;
            }
            Console.WriteLine(count);
        }
    }
}

using StockProSim.Data;
using APIcalls;
using RiskCalculations;
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
            string apiKey = "BGJXL2TUCOT3RB2K";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            string symbol = "AAPL";

            /*List<StockData2> stockDataList = await alphaVantage.FetchStockData(symbol);
            int count = 0;
            foreach (var stockData in stockDataList)
            {
                Console.WriteLine($"Close={stockData.Close}");
                count ++;
            }
            Console.WriteLine(count);*/
            RiskCalculator rc = new RiskCalculator();

            string filePath = "test_data.csv"; // Change this to the correct path of your CSV file
            List<decimal> closeValues = new List<decimal>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string? line;
                    bool isHeader = true;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isHeader)
                        {
                            // Skip the header row
                            isHeader = false;
                            continue;
                        }

                        var values = line.Split(',');
                        if (values.Length > 1)
                        {
                            if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue))
                            {
                                closeValues.Add(closeValue);
                            }
                            else
                            {
                                Console.WriteLine($"Failed to parse Close/Last value: {values[1]}");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            decimal total = 0;
            foreach (var item in closeValues)
            {
                Console.WriteLine(item);
                total += item;
            }
            Console.WriteLine(total / closeValues.Count);
        }
    }
}

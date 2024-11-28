using StockProSim.Data;
using API_Calls;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace NEA_first_draft
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter a stock ticker symbol (e.g., AAPL, MSFT):");
            string ticker = Console.ReadLine();

            await APICalls.FetchAndDisplayStockInfo(ticker);
        }
    }
}

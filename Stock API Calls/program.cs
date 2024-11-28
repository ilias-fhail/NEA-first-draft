using Newtonsoft.Json;
using Stock_API_Calls;
using BlazorApp1;
using static BlazorApp1.Components.Pages.Weather;
class Program
{
    private static async Task FetchAndDisplayStockInfo(string ticker)
    {
        string baseApi = "https://finnhub.io/api/v1";
        string apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";

        APICalls client = new APICalls(baseApi, apiKey);

        StockData stockInfo = await client.FetchStockInfoAsync(ticker);

        if (stockInfo != null)
        {
            Console.WriteLine($"Stock Data for {stockInfo.Ticker}:");
            Console.WriteLine($"- Current Price: {stockInfo.CurrentPrice}");
            Console.WriteLine($"- High Price: {stockInfo.HighPrice}");
            Console.WriteLine($"- Low Price: {stockInfo.LowPrice}");
            Console.WriteLine($"- Open Price: {stockInfo.OpenPrice}");
            Console.WriteLine($"- Previous Close: {stockInfo.PreviousClose}");
            Console.WriteLine($"- Price Change: {stockInfo.PriceChange}");
            Console.WriteLine($"- Last Update: {stockInfo.LastUpdate}");
        }
        else
        {
            Console.WriteLine($"Failed to fetch data for {ticker}.");
        }
    }
}
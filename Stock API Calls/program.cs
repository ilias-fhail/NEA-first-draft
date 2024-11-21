using Newtonsoft.Json;
using Stock_API_Calls;
class Program
{
    private static async Task FetchAndDisplayStockInfo(string ticker)
    {
        string baseApi = "https://finnhub.io/api/v1";
        string apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";

        APICalls client = new APICalls(baseApi, apiKey);

        var stockInfo = await client.FetchStockInfoAsync(ticker);

        if (stockInfo != null)
        {
            Console.WriteLine($"Stock Data for {ticker}:");
            Console.WriteLine(JsonConvert.SerializeObject(stockInfo, Formatting.Indented));
        }
        else
        {
            Console.WriteLine($"Failed to fetch data for {ticker}.");
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter a stock ticker symbol:");
        string ticker = Console.ReadLine();

        await FetchAndDisplayStockInfo(ticker);
    }
}
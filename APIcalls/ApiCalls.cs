namespace APIcalls
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    public class StockData
    {
        public string Ticker { get; set; }

        [JsonProperty("c")]
        public decimal CurrentPrice { get; set; }

        [JsonProperty("h")]
        public decimal HighPrice { get; set; }

        [JsonProperty("l")]
        public decimal LowPrice { get; set; }

        [JsonProperty("o")]
        public decimal OpenPrice { get; set; }

        [JsonProperty("pc")]
        public decimal PreviousClose { get; set; }

        [JsonProperty("t")]
        public long LastUpdateUnix { get; set; }

        public DateTime LastUpdate => DateTimeOffset.FromUnixTimeSeconds(LastUpdateUnix).UtcDateTime;

        public decimal PriceChange => Math.Round(100 * (CurrentPrice - OpenPrice) / OpenPrice, 2);
    }

    public class APICalls
    {
        private readonly string _baseApi;
        private readonly string _apiKey;

        public APICalls(string baseApi, string apiKey)
        {
            _baseApi = baseApi;
            _apiKey = apiKey;
        }

        public async Task<StockData?> FetchStockInfoAsync(string ticker)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"{_baseApi}/quote?symbol={ticker}&token={_apiKey}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var stockData = JsonConvert.DeserializeObject<StockData>(jsonResponse);
                        if (stockData != null)
                        {
                            stockData.Ticker = ticker;
                        }
                        return stockData;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    return null;
                }
            }
        }


        public static async Task FetchAndDisplayStockInfo(string ticker)
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
}

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

        public decimal PriceChange => (CurrentPrice - OpenPrice);
    }

    public class APICalls
    {
        private readonly string _baseApi;
        private readonly string _apiKey;

        public APICalls(string baseApi, string apiKey)
        {
            _baseApi = "https://finnhub.io/api/v1";
            _apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";
        }

        public async Task<StockData?> FetchStockInfoAsync(string ticker)
        {
            using (HttpClient client = new HttpClient())
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
                     Console.WriteLine("Could not get data.");
                      return null;
                 }
            }
        }
        public async Task<decimal> FetchCurrentPriceAsync(string ticker)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{_baseApi}/quote?symbol={ticker}&token={_apiKey}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    if (json != null && json["c"] != null)
                    {
                        return (decimal)json["c"];
                    }
                    else { return 0; }
                }
                else { return 0; }
            }
        }


    }
}

namespace APIcalls
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
    public class NewsArticle
    {
        public string Headline { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
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
    public class AlphaVantage
    {
        private string apiKey;
        private string baseUrl;

        public AlphaVantage(string apiKey)
        {
            this.apiKey = apiKey;
            this.baseUrl = "https://www.alphavantage.co/query";
        }

        public async Task<List<StockData2>> FetchStockData(string symbol)
        {
            List<StockData2> allStockData = new List<StockData2>();
            string function = "TIME_SERIES_WEEKLY";
            string url = $"{baseUrl}?function={function}&symbol={symbol}&apikey={apiKey}&outputsize=compact";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(url);
                    JObject data = JObject.Parse(response);

                    if (data.ContainsKey("Weekly Time Series"))
                    {
                        var timeSeries = data["Weekly Time Series"] as JObject;
                        if (timeSeries != null)
                        {
                            int count = 0;

                            foreach (var entry in timeSeries.Properties())
                            {
                                if (count >= 100) break;

                                string date = entry.Name;
                                DateTime dateTime = DateTime.Parse(date);

                                var values = entry.Value as JObject;
                                if (values != null)
                                {
                                    decimal open = Convert.ToDecimal(values["1. open"]);
                                    decimal high = Convert.ToDecimal(values["2. high"]);
                                    decimal low = Convert.ToDecimal(values["3. low"]);
                                    decimal close = Convert.ToDecimal(values["4. close"]);
                                    long volume = Convert.ToInt64(values["5. volume"]);

                                    allStockData.Add(new StockData2
                                    {
                                        Date = dateTime,
                                        Open = open,
                                        High = high,
                                        Low = low,
                                        Close = close,
                                        Volume = volume
                                    });

                                    count++;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("The JSON response does not contain 'Weekly Time Series' data.");
                        Console.WriteLine(response);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching stock data for {symbol}: {ex.Message}");
                }
            }

            return allStockData;
        }

        public class StockData2
        {
            public DateTime Date { get; set; }
            public decimal Open { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Close { get; set; }
            public long Volume { get; set; }
        }
    }
    public class StockNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public StockNewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";
        }

        public async Task<List<NewsArticle>> GetStockNewsAsync()
        {
            string url = $"https://finnhub.io/api/v1/news?category=general&token={_apiKey}";

            var response = await _httpClient.GetFromJsonAsync<List<NewsArticle>>(url);
            return response?.Take(5).ToList() ?? new List<NewsArticle>();
        }
        public async Task<NewsArticle?> GetStockNewsByTickerAsync(string ticker)
        {
            string fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
            string toDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string url = $"https://finnhub.io/api/v1/company-news?symbol={ticker}&from={fromDate}&to={toDate}&token={_apiKey}";

            var response = await _httpClient.GetFromJsonAsync<List<NewsArticle>>(url);
            return response?.FirstOrDefault();
        }


    }
}

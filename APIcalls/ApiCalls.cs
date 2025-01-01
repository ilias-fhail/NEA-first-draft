namespace APIcalls
{
    using System;
    using System.Net.Http;
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
        public async Task<List<(DateTime Date, decimal ClosePrice)>> FetchHistoricalDataAsync(string ticker)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{_baseApi}/stock/candle";

                var now = DateTime.UtcNow;
                var twoYearsAgo = now.AddYears(-2);
                long fromTimestamp = new DateTimeOffset(twoYearsAgo).ToUnixTimeSeconds();
                long toTimestamp = new DateTimeOffset(now).ToUnixTimeSeconds();

                string queryString = $"?symbol={ticker}&resolution=D&from={fromTimestamp}&to={toTimestamp}&token={_apiKey}";
                HttpResponseMessage response = await client.GetAsync(url + queryString);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    if (json != null && json["c"] != null && json["t"] != null)
                    {
                        var closePrices = ((IEnumerable<dynamic>)json["c"]).Select(c => (decimal)c).ToList();
                        var timestamps = ((IEnumerable<dynamic>)json["t"]).Select(t => (long)t).ToList();

                        var historicalData = timestamps.Select((timestamp, index) =>
                            (
                                Date: DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
                                ClosePrice: closePrices[index]
                            )).ToList();

                        return historicalData;
                    }
                    else
                    {
                        Console.WriteLine("Invalid data in the response.");
                        return new List<(DateTime, decimal)>();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to fetch historical data.");
                    return new List<(DateTime, decimal)>();
                }
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

        public async Task<List<StockData2>> FetchDailyStockDataForLast300Days(string symbol)
        {
            List<StockData2> allStockData = new List<StockData2>();

            int dataFetched = 0;
            string function = "TIME_SERIES_DAILY";
            string url = $"{baseUrl}?function={function}&symbol={symbol}&apikey={apiKey}&outputsize=full";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(url);

                    JObject data = JObject.Parse(response);

                    if (data.ContainsKey("Time Series (Daily)"))
                    {
                        var timeSeries = data["Time Series (Daily)"] as JObject;
                        if (timeSeries != null)
                        {
                            foreach (var dayData in timeSeries.Properties())
                            {
                                if (dataFetched >= 300) break;

                                string date = dayData.Name; 
                                DateTime dateTime = DateTime.Parse(date);

                                var values = dayData.Value as JObject;
                                if (values != null)
                                {
                                    double open = Convert.ToDouble(values["1. open"]);
                                    double high = Convert.ToDouble(values["2. high"]);
                                    double low = Convert.ToDouble(values["3. low"]);
                                    double close = Convert.ToDouble(values["4. close"]);
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

                                    dataFetched++;
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("The JSON response does not contain 'Time Series (Daily)' data.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return allStockData;
        }


        public class StockData2
        {
            public DateTime Date { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public long Volume { get; set; }
        }
    }
}

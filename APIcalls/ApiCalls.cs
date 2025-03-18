namespace APIcalls
{

    // This area is where my API calls are which have been split by their function into current data, historical data, and fninancial news.

    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class StockData // This is the custom object class for data coming from Finhub API
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

        public DateTime LastUpdate => DateTimeOffset.FromUnixTimeSeconds(LastUpdateUnix).UtcDateTime; // changes it from Unix timestamp to DateTime data type.

        public decimal PriceChange => (CurrentPrice - OpenPrice);
    }
    public class NewsArticle // this is the custom object class for the stocknews API
    {
        public string Headline { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
    }
    public class StockData2 // this is the custom object class for the historical stock data coming from the alphavantage API
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class APICalls // class for the API calls from Finhub API
    {
        private readonly string _baseApi;
        private readonly string _apiKey;

        public APICalls(string baseApi, string apiKey) // the constructor here sets the API key and base API where the API is one that any user can use in this case as it doesnt have a daily limit
        {
            _baseApi = "https://finnhub.io/api/v1";
            _apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";
        }
        public async Task<StockData?> FetchStockInfoAsync(string ticker) // defining the function that retrieves stock information from an inputted ticker.
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{_baseApi}/quote?symbol={ticker}&token={_apiKey}"; // creating the HTTP request.
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var stockData = JsonConvert.DeserializeObject<StockData>(jsonResponse); // Storing the recieved data as a stock data object
                    if (stockData != null)
                    {
                        stockData.Ticker = ticker; // adding the stock ticker to the object as it is not included in the response
                    }
                    return stockData;
                }
                else
                {
                    Console.WriteLine("Could not get data. (Finhub API)"); // If the API call does not generate a valid response an error is logged into the console for debugging.
                    return null;
                }
            }
        }
        public async Task<decimal> FetchCurrentPriceAsync(string ticker) // this function just gets the current price of a stock and no other information.
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{_baseApi}/quote?symbol={ticker}&token={_apiKey}"; // creating the HTTP request
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<dynamic>(jsonResponse); // Storing the response as a dynamic data type as there is no custom object for this response.

                    if (json != null && json["c"] != null) //checking to see if a current price was returned as it is binded to the Json object "c"
                    {
                        return (decimal)json["c"];
                    }
                    else { return 0; }
                }
                else 
                {
                    Console.WriteLine("Could not get data. (Finhub API)"); // If an error occured in the API call an error is logged into the console for debugging
                    return 0; 
                }
            }
        }

    }
    public class AlphaVantage // class for API calls to AlphaVantage API
    {
        private string apiKey;
        private string baseUrl;

        public AlphaVantage(string apiKey) //here the API key is entered by the user as there is a limitation on the request numbers.
        {
            this.apiKey = apiKey;
            this.baseUrl = "https://www.alphavantage.co/query";
        }

        public async Task<List<StockData2>> FetchStockData(string symbol)
        {
            List<StockData2> allStockData = new List<StockData2>();
            string function = "TIME_SERIES_WEEKLY";
            string url = $"{baseUrl}?function={function}&symbol={symbol}&apikey={apiKey}&outputsize=compact"; // creating the HTTP request

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync(url);
                    JObject data = JObject.Parse(response); // Converts response into a Json Object

                    if (data.ContainsKey("Weekly Time Series")) //checks the response if of the correct format
                    {
                        var timeSeries = data["Weekly Time Series"] as JObject;
                        if (timeSeries != null)
                        {
                            int count = 0;

                            foreach (var entry in timeSeries.Properties()) // loop through all the data
                            {
                                if (count >= 100) break; //stops the loop after 100 datapoints have been read.

                                string date = entry.Name;
                                DateTime dateTime = DateTime.Parse(date);

                                var values = entry.Value as JObject; // tries cast entry.value into a json object and due to "as" it handles any error nicely instead casting it as null if it cant.
                                if (values != null)
                                {                                                           // assigns all the properties to temporary fields converting them into the appropriate data types.
                                    decimal open = Convert.ToDecimal(values["1. open"]); 
                                    decimal high = Convert.ToDecimal(values["2. high"]);
                                    decimal low = Convert.ToDecimal(values["3. low"]);
                                    decimal close = Convert.ToDecimal(values["4. close"]);
                                    long volume = Convert.ToInt64(values["5. volume"]);
                                                                                            
                                    allStockData.Add(new StockData2
                                    {                                   // populating the objects with the temprary fields 
                                        Date = dateTime,
                                        Open = open,
                                        High = high,
                                        Low = low,
                                        Close = close,
                                        Volume = volume
                                    });

                                    count++;    // incrementing the count of how many data points have been read.
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("The JSON response does not contain 'Weekly Time Series' data."); // error logs in the console where a response has been sent but not correct
                        Console.WriteLine(response);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching stock data for {symbol}: {ex.Message}"); // error logs in the console for any other exception that hasn't already been handled
                }
            }

            return allStockData;
        }
    }
    public class StockNewsService // API class for the Stock news API calls which also come from finhub API but has been seperated as it uses a different plan.
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public StockNewsService(HttpClient httpClient) //constructor initialising the API key as again this key can withstand the volume with no limit.
        {
            _httpClient = httpClient;
            _apiKey = "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0";
        }

        public async Task<List<NewsArticle>> GetStockNewsAsync() // function for getting general stock news
        {
            string url = $"https://finnhub.io/api/v1/news?category=general&token={_apiKey}"; //creating the HTTP request

            var response = await _httpClient.GetFromJsonAsync<List<NewsArticle>>(url);
            return response?.Take(5).ToList() ?? new List<NewsArticle>();  //only takes the first 5 responses and formats them into a list of news articles.
        }
        public async Task<NewsArticle?> GetStockNewsByTickerAsync(string ticker) // this function gets news from the last 7 days about specific stocks.
        {
            string fromDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd"); // calculates the date 7 days ago.
            string toDate = DateTime.UtcNow.ToString("yyyy-MM-dd"); // gets the current date

            string url = $"https://finnhub.io/api/v1/company-news?symbol={ticker}&from={fromDate}&to={toDate}&token={_apiKey}"; // creates the HTTP request.

            var response = await _httpClient.GetFromJsonAsync<List<NewsArticle>>(url);
            return response?.FirstOrDefault(); //resturns rather the first item in the response which is the first article or its default type which in this case is null.
        }
    }
}

﻿namespace Stock_API_Calls
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using static BlazorApp1.Components.Pages.Weather;


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

        public decimal PriceChange => CurrentPrice - OpenPrice;
    }

    class APICalls
    {
        private readonly string _baseApi;
        private readonly string _apiKey;

        public APICalls(string baseApi, string apiKey)
        {
            _baseApi = baseApi;
            _apiKey = apiKey;
        }

        public async Task<dynamic> FetchStockInfoAsync(string ticker)
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
                        stockData.Ticker = ticker;
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
    }

}

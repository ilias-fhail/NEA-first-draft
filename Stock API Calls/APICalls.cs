namespace Stock_API_Calls
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

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
                        var stockInfo = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        return stockInfo;
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

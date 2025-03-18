using APIcalls;
using StockProSim.Data;
using System.ComponentModel.DataAnnotations.Schema;
using static APIcalls.AlphaVantage;
namespace RiskCalculations
{

    // Here is all the code that deals with the calculations for risk and statistical analysis of stock data. 
    public class RiskCalculator
    {
        private readonly string _apiKey = "BGJXL2TUCOT3RB2K";  // example of the user's unique API key for AlphaVantage

        public RiskCalculator() { }

        public async Task<List<StockEntry>> PrepareStockData(string ticker) // this function prepares the stock data into a collection of custom objects which can then be passed on to the rest of the code.
        {
            AlphaVantage alphaVantage = new AlphaVantage(_apiKey);
            var stockData = await alphaVantage.FetchStockData(ticker); //asynchronous call to the API

            DateTime today = DateTime.Today;

            var formattedData = stockData.Select(data => new StockEntry    //formating each datapoint as a new object
            {
                Ticker = ticker,
                Date = today,
                ClosePrice = data.Close
            }).ToList();

            return formattedData; //returning a list of the formatted data
        }

        public async Task<List<List<decimal>>> FetchClosingPrices(List<TradeHistory> symbols) // this function actively retrieves the data from the API calls or the databse.
        {
            MyServerDb databse = new MyServerDb(); //initialising an instance of the databse class
            var allClosingPrices = new List<List<decimal>>();

            foreach (var symbol in symbols) //looping through all the stock tickers for which data has been requested for
            {
                if (await databse.DataExists(symbol.StockTicker) == true)  //checking first if the symbol exists in the database
                {
                    Console.WriteLine("symbol exists in Database");
                    if (await databse.IsDataWeekOld(symbol.StockTicker) == true) // if it does check if the data is a week old
                    {
                        Console.WriteLine("data is a week old");
                        databse.DeleteData(symbol.StockTicker); //remove the old data to save memory
                        var stockData = await PrepareStockData(symbol.StockTicker); // call the previous function to get the data from the API and format it into objects
                        databse.AddData(stockData); // add the new data to the database
                    }
                }
                else // if it is not present calling an API call to get the data
                {
                    Console.WriteLine("symbol does not exist adding it");
                    var stockData = await PrepareStockData(symbol.StockTicker);
                    databse.AddData(stockData); //add it to database
                }
                var ClosingPrices = await databse.GetData(symbol.StockTicker); //get it from the database
                allClosingPrices.Add(ClosingPrices);
            }

            return allClosingPrices; //return a list of lists of the prices
        }

        public decimal StockVariance(List<decimal> closingPrices)  // a purely mathematical function with no external calls to calculate statistical variance for a list of decimal values
        {
            if (closingPrices.Count < 2) return 0; // checking the list meets the minimum length requirement

            // Calculate returns
            List<decimal> returns = new List<decimal>();
            for (int i = 1; i < closingPrices.Count; i++)
            {
                returns.Add((closingPrices[i] - closingPrices[i - 1]) / closingPrices[i - 1]);  // iterating through and adding to a list the returns values which is the change in price each day.
            }

            int count = returns.Count; // total number of items in the list
            if (count < 2) return 0;
                                                // variance mathematical algorithm below
            decimal average = returns.Average();
            decimal variance = returns.Sum(r => (r - average) * (r - average));
            return variance / (count - 1);
        }

        public decimal StockCovariance(List<decimal> prices1, List<decimal> prices2) // another purely mathematical function with no external calls to calculate statistical covariance for 2 lists of decimal values
        {
            int count = Math.Min(prices1.Count, prices2.Count);  // finds the length of the smallest list and checks if it meets the minimum length
            if (count < 2) return 0;

            List<decimal> returns1 = new List<decimal>();
            List<decimal> returns2 = new List<decimal>();
            for (int i = 1; i < count; i++)
            {                                                                   // calculates the returns for the 2 lists
                returns1.Add((prices1[i] - prices1[i - 1]) / prices1[i - 1]);
                returns2.Add((prices2[i] - prices2[i - 1]) / prices2[i - 1]);
            }

            int returnCount = Math.Min(returns1.Count, returns2.Count);  //calculates the total and validates the returns list
            if (returnCount < 2) return 0;
                                                            // algorithm for covariance following the formula
            decimal average1 = returns1.Average();
            decimal average2 = returns2.Average();

            decimal covariance = 0;
            for (int i = 0; i < returnCount; i++)
            {
                covariance += (returns1[i] - average1) * (returns2[i] - average2);
            }

            return covariance / (returnCount - 1);
        }

        public async Task<decimal> PortfolioVariance(List<TradeHistory> trades, List<List<decimal>> closingPrices) //calculating the varinace of a collection of stock trades
        {
            decimal total = 0;
            decimal tempVar1 = 0, tempVar2 = 0;

            APICalls apiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0"); //intialising finhub API calls

            for (int i = 0; i < trades.Count; i++)
            {                                                                                          // updating the values of some of the trade parameters to ensure that they are up to date as of the calculation
                trades[i].CurrentPrice = await apiCalls.FetchCurrentPriceAsync(trades[i].StockTicker);
                trades[i].PriceChange = trades[i].CurrentPrice - trades[i].PriceBought;
                trades[i].TradeValue = trades[i].CurrentPrice * trades[i].Quantity;
                total += trades[i].TradeValue; //calculating the total portfolio worth
            }

            foreach (var trade in trades)  // looping through the trades and calculating their weight in refrence to the total portfolio.
            {
                trade.TradeWeight = trade.TradeValue / total;
            }
                                                                                                                // algorithm for portfolio variance following the formula where variance of each stock and covariance of every pair of stocks is calculated
            for (int i = 0; i < trades.Count; i++)
            {
                tempVar1 += trades[i].TradeWeight * trades[i].TradeWeight * StockVariance(closingPrices[i]);
            }

            for (int i = 0; i < trades.Count; i++)
            {
                for (int j = i + 1; j < trades.Count; j++)
                {
                    tempVar2 += trades[i].TradeWeight * trades[j].TradeWeight * StockCovariance(closingPrices[i], closingPrices[j]);
                }
            }

            return tempVar1 + 2 * tempVar2;
        }

        public async Task<decimal> GetVAR(List<TradeHistory> trades)  // the final algorithm which is the only one called from the front end which calculates Value at Risk and only needs an input of trades.
        {
            decimal total = trades.Sum(trade => trade.TradeValue);
            List<List<decimal>> closingPrices = await FetchClosingPrices(trades);
            decimal portfolioVariance = await PortfolioVariance(trades, closingPrices); // calls the portfolio variance
            return Convert.ToDecimal(Math.Round(2.33 * Convert.ToDouble(total) * Math.Sqrt(Convert.ToDouble(portfolioVariance)), 2)); // uses the formula to convert variance into value at risk  Note: 2.33 is the confidence index and in this case represents a confidence of 99% 
        }
    }
}

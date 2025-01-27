using APIcalls;
using StockProSim.Data;
using System.ComponentModel.DataAnnotations.Schema;
using static APIcalls.AlphaVantage;
namespace RiskCalculations
{
    public class RiskCalculator
    {
        private readonly string _apiKey = "BGJXL2TUCOT3RB2K";

        public RiskCalculator() { }

        public async Task<List<List<decimal>>> FetchClosingPrices(List<TradeHistory> symbols)
        {
            MyServerDb databse = new MyServerDb();
            var allClosingPrices = new List<List<decimal>>();

            foreach (var symbol in symbols)
            {
                if (await databse.DataExists(symbol.StockTicker) == true) 
                {
                    Console.WriteLine("symbol exists in Database");
                    if (await databse.IsDataWeekOld(symbol.StockTicker) == true)
                    {
                        Console.WriteLine("data is a week old");
                        databse.DeleteData(symbol.StockTicker);
                        var stockData = await PrepareStockData(symbol.StockTicker);
                        databse.AddData(stockData);
                    }
                }
                else
                {
                    Console.WriteLine("symbol does not exist adding it");
                    var stockData = await PrepareStockData(symbol.StockTicker);
                    databse.AddData(stockData);
                }
                var ClosingPrices = await databse.GetData(symbol.StockTicker);
                allClosingPrices.Add(ClosingPrices);
            }

            return allClosingPrices;
        }

        public decimal StockVariance(List<decimal> closingPrices)
        {
            if (closingPrices.Count < 2) return 0;

            // Calculate returns
            List<decimal> returns = new List<decimal>();
            for (int i = 1; i < closingPrices.Count; i++)
            {
                returns.Add((closingPrices[i] - closingPrices[i - 1]) / closingPrices[i - 1]);
            }

            int count = returns.Count;
            if (count < 2) return 0;
            decimal average = returns.Average();
            decimal variance = returns.Sum(r => (r - average) * (r - average));
            return variance / (count - 1);
        }

        public decimal StockCovariance(List<decimal> prices1, List<decimal> prices2)
        {
            int count = Math.Min(prices1.Count, prices2.Count);
            if (count < 2) return 0;

            List<decimal> returns1 = new List<decimal>();
            List<decimal> returns2 = new List<decimal>();
            for (int i = 1; i < count; i++)
            {
                returns1.Add((prices1[i] - prices1[i - 1]) / prices1[i - 1]);
                returns2.Add((prices2[i] - prices2[i - 1]) / prices2[i - 1]);
            }

            int returnCount = Math.Min(returns1.Count, returns2.Count);
            if (returnCount < 2) return 0;

            decimal average1 = returns1.Average();
            decimal average2 = returns2.Average();

            decimal covariance = 0;
            for (int i = 0; i < returnCount; i++)
            {
                covariance += (returns1[i] - average1) * (returns2[i] - average2);
            }

            return covariance / (returnCount - 1);
        }

        public async Task<decimal> PortfolioVariance(List<TradeHistory> trades, List<List<decimal>> closingPrices)
        {
            decimal total = 0;
            decimal tempVar1 = 0, tempVar2 = 0;

            APICalls apiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");

            for (int i = 0; i < trades.Count; i++)
            {
                trades[i].CurrentPrice = await apiCalls.FetchCurrentPriceAsync(trades[i].StockTicker);
                trades[i].PriceChange = trades[i].CurrentPrice - trades[i].PriceBought;
                trades[i].TradeValue = trades[i].CurrentPrice * trades[i].Quantity;
                total += trades[i].TradeValue;
            }

            foreach (var trade in trades)
            {
                trade.TradeWeight = trade.TradeValue / total;
            }

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

        public async Task<decimal> GetVAR(List<TradeHistory> trades)
        {
            decimal total = trades.Sum(trade => trade.TradeValue);
            List<List<decimal>> closingPrices = await FetchClosingPrices(trades);
            decimal portfolioVariance = await PortfolioVariance(trades, closingPrices);
            return Convert.ToDecimal(Math.Round(2.33 * Convert.ToDouble(total) * Math.Sqrt(Convert.ToDouble(portfolioVariance)), 2)); // 2.33 is the confidence index for 99%
        }
        public async Task<List<StockEntry>> PrepareStockData(string ticker)
        {
            AlphaVantage alphaVantage = new AlphaVantage(_apiKey);
            var stockData = await alphaVantage.FetchStockData(ticker);

            DateTime today = DateTime.Today;

            var formattedData = stockData.Select(data => new StockEntry
            {
                Ticker = ticker,
                Date = today,
                ClosePrice = data.Close
            }).ToList();

            return formattedData;
        }
    }
}

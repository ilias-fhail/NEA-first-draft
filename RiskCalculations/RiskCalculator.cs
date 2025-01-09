using APIcalls;
using StockProSim.Data;
using static APIcalls.AlphaVantage;
namespace RiskCalculations
{
    public class RiskCalculator
    {
        private readonly string _apiKey = "BGJXL2TUCOT3RB2K";

        public RiskCalculator() { }

        public async Task<List<List<decimal>>> FetchClosingPrices(List<TradeHistory> symbols)
        {
            AlphaVantage alphaVantage = new AlphaVantage(_apiKey);
            var allClosingPrices = new List<List<decimal>>();

            foreach (var symbol in symbols)
            {
                var stockData = await alphaVantage.FetchStockData(symbol.StockTicker);
                var closingPrices = stockData.Select(sd => sd.Close).ToList();
                allClosingPrices.Add(closingPrices);
            }

            return allClosingPrices;
        }

        public decimal StockVariance(List<decimal> closingPrices)
        {
            int count = closingPrices.Count;
            if (count < 2) return 0;

            decimal total = closingPrices.Sum();
            decimal average = total / count;

            decimal variance = closingPrices.Sum(price => (price - average) * (price - average));
            return variance / (count - 1);
        }

        public decimal StockCovariance(List<decimal> prices1, List<decimal> prices2)
        {
            int count = Math.Min(prices1.Count, prices2.Count);
            if (count < 2) return 0;

            decimal average1 = prices1.Take(count).Average();
            decimal average2 = prices2.Take(count).Average();

            decimal covariance = 0;
            for (int i = 0; i < count; i++)
            {
                covariance += (prices1[i] - average1) * (prices2[i] - average2);
            }

            return covariance / (count - 1);
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
                for (int j = 0; j < trades.Count; j++)
                {
                    if (i != j)
                    {
                        tempVar2 += trades[i].TradeWeight * trades[j].TradeWeight * StockCovariance(closingPrices[i], closingPrices[j]);
                    }
                }
            }

            return tempVar1 + 2 * tempVar2;
        }

        public async Task<decimal> GetVAR(List<TradeHistory> trades)
        {
            decimal total = trades.Sum(trade => trade.TradeValue);
            List<List<decimal>> closingPrices = await FetchClosingPrices(trades);
            decimal portfolioVariance = await PortfolioVariance(trades, closingPrices);
            return 2 * total * portfolioVariance; // 2 is the confidence index
        }
    }
}

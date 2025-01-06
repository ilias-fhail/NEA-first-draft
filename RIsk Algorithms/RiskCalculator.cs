﻿using APIcalls;
using StockProSim.Data;
using static APIcalls.AlphaVantage;
namespace RIsk_Algorithms
{
    public class RiskCalculator
    {
        public RiskCalculator() { }
        public async Task<double> StockVariance(string symbol)
        {
            string apiKey = "MUY2SK806D7KE5WM";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            List<StockData2> stockDataList = await alphaVantage.FetchStockData(symbol);

            double total = 0;
            double average = 0;
            int count = 0;
            double variance = 0;
            double tempVar = 0;

            foreach (var stockData in stockDataList)
            {
                total = total + stockData.Close;
                count++;
            }
            average = total / count;
            foreach (var stockData in stockDataList)
            {
                tempVar =tempVar +(( stockData.Close - average)* (stockData.Close - average)) ;
            }
            variance = 1/(count-1) * tempVar;
            return variance;
        }
        public async Task<double> StockCovariance(string symbol1,string symbol2)
        {
            string apiKey = "MUY2SK806D7KE5WM";

            AlphaVantage alphaVantage = new AlphaVantage(apiKey);
            List<StockData2> stockDataList1 = await alphaVantage.FetchStockData(symbol1);
            List<StockData2> stockDataList2 = await alphaVantage.FetchStockData(symbol2);

            double total1 = 0;
            double total2 = 0;
            double average1 = 0;
            double average2;
            int count = 0;
            double Covariance = 0;
            double tempVar1 = 0;
            double tempVar2 = 0;

            foreach (var stockData in stockDataList1)
            {
                total1 = total1 + stockData.Close;
                count++;
            }
            average1 = total1 / count;
            foreach (var stockData in stockDataList1)
            {
                tempVar1 = tempVar1 + ((stockData.Close - average1) * (stockData.Close - average1));
            }
            foreach (var stockData in stockDataList2)
            {
                total2 = total2 + stockData.Close;
            }
            average2 = total2 / count;
            foreach (var stockData in stockDataList2)
            {
                tempVar2 = tempVar2 + ((stockData.Close - average2) * (stockData.Close - average2));
            }
            Covariance = 1 / (count - 1) * tempVar1*tempVar2;
            return Covariance;
        }
        public async Task<decimal> Portfolio_Variance(List<TradeHistory> trade)
        {
            decimal total = 0;
            APICalls ApiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
            foreach (var trades in trade)
            {
                trades.CurrentPrice = await ApiCalls.FetchCurrentPriceAsync(trades.StockTicker);
                trades.PriceChange = trades.CurrentPrice - trades.PriceBought;
                trades.TradeValue = trades.CurrentPrice * trades.Quantity;
                total = trades.TradeValue + total;
            }
        }


    }
}


using RiskCalculations;
namespace UnitTests
{
    [TestClass]
    public class Calculation_Tests
    {
        [TestMethod]
        public void StockVariance_WithSingleValue()
        {
            var riskCalculator = new RiskCalculator();
            var closingPrices = new List<decimal> { 100m };

            var variance = riskCalculator.StockVariance(closingPrices);

            Assert.AreEqual(0, variance);
        }

        [TestClass]
        public class RiskCalculatorTests
        {
            [TestMethod]
            public void StockVariance_WithMultipleValues()
            {
                var riskCalculator = new RiskCalculator();
                var closingPrices = new List<decimal> { 100m, 200m, 300m };

                var variance = riskCalculator.StockVariance(closingPrices);

                var returns = new List<decimal>{(200m - 100m) / 100m, (300m - 200m) / 200m};

                var averageReturn = returns.Average();
                var expectedVariance = (decimal)((Math.Pow((double)(returns[0] - averageReturn), 2) +
                                                  Math.Pow((double)(returns[1] - averageReturn), 2)) / (returns.Count - 1));

                Assert.AreEqual(expectedVariance, variance);
            }

            [TestMethod]
            public void StockCovariance_WithIdenticalValues()
            {
                var riskCalculator = new RiskCalculator();
                var prices1 = new List<decimal> { 100m, 200m, 300m };
                var prices2 = new List<decimal> { 100m, 200m, 300m };

                var covariance = riskCalculator.StockCovariance(prices1, prices2);

                var returns1 = new List<decimal>{(200m - 100m) / 100m, (300m - 200m) / 200m };

                var returns2 = new List<decimal>{(200m - 100m) / 100m,(300m - 200m) / 200m};

                var average1 = returns1.Average();
                var average2 = returns2.Average();
                var expectedCovariance = (decimal)(((returns1[0] - average1) * (returns2[0] - average2) +
                                                    (returns1[1] - average1) * (returns2[1] - average2)) / (returns1.Count - 1));

                Assert.AreEqual(expectedCovariance, covariance);
            }

            [TestMethod]
            public void StockCovariance_WithDifferentValues()
            {
                var riskCalculator = new RiskCalculator();
                var prices1 = new List<decimal> { 100m, 200m, 300m };
                var prices2 = new List<decimal> { 300m, 200m, 100m };

                var covariance = riskCalculator.StockCovariance(prices1, prices2);

                var returns1 = new List<decimal>{(200m - 100m) / 100m,(300m - 200m) / 200m};

                var returns2 = new List<decimal>{(200m - 300m) / 300m,(100m - 200m) / 200m};
                var average1 = returns1.Average();
                var average2 = returns2.Average();
                var expectedCovariance = ((returns1[0] - average1) * (returns2[0] - average2) +(returns1[1] - average1) * (returns2[1] - average2)) / (returns1.Count - 1);

                Assert.AreEqual(expectedCovariance, covariance);
            }

            [TestMethod]
            public void StockCovariance_WithMismatchedLengths()
            {
                var riskCalculator = new RiskCalculator();
                var prices1 = new List<decimal> { 100m, 200m };
                var prices2 = new List<decimal> { 300m, 200m, 100m };

                var covariance = riskCalculator.StockCovariance(prices1, prices2);

                var returns1 = new List<decimal>{(200m - 100m) / 100m};

                var returns2 = new List<decimal>{(200m - 300m) / 300m };

                var average1 = returns1.Average();
                var average2 = returns2.Average();
                var expectedCovariance = (returns1[0] - average1) * (returns2[0] - average2) / (returns1.Count);

                Assert.AreEqual(expectedCovariance, covariance);
            }
        }

        [TestMethod]
        public void StockVariance_WithLargeDataSet()
        {
            var riskCalculator = new RiskCalculator();
            string filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_data.csv";
            List<decimal> closeValues = new List<decimal>();
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    var values = line.Split(',');
                    if (values.Length > 1)
                    {
                        if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue))
                        {
                            closeValues.Add(closeValue);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse value");
                        }
                    }
                }
            }
            var variance = riskCalculator.StockVariance(closeValues);

            var expectedVariance = (decimal)0.00035;
            Assert.AreEqual(expectedVariance, Math.Round(variance, 5));
        }
        [TestMethod]
        public void StockCovariance_WithLargeDataSet()
        {
            var riskCalculator = new RiskCalculator();
            string filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_datacopy.csv";
            List<decimal> closeValues = new List<decimal>();
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    var values = line.Split(',');
                    if (values.Length > 1)
                    {
                        if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue))
                        {
                            closeValues.Add(closeValue);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse value");
                        }
                    }
                }
            }
            filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_data2.csv";
            List<decimal> closeValues2 = new List<decimal>();
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    var values = line.Split(',');
                    if (values.Length > 1)
                    {
                        if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue))
                        {
                            closeValues2.Add(closeValue);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse value");
                        }
                    }
                }
            }
            var Covariance = riskCalculator.StockCovariance(closeValues,closeValues2);

            var expectedVariance = (decimal)0.00027;
            Assert.AreEqual(expectedVariance, Math.Round(Covariance, 5));
        }
    }
}
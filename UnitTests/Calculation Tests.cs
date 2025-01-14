
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

        [TestMethod]
        public void StockVariance_WithMultipleValues()
        {
            var riskCalculator = new RiskCalculator();
            var closingPrices = new List<decimal> { 100m, 200m, 300m };

            var variance = riskCalculator.StockVariance(closingPrices);

            var expectedVariance = (decimal)((Math.Pow(100 - 200, 2) + Math.Pow(200 - 200, 2) + Math.Pow(300 - 200, 2)) / 2); 
            Assert.AreEqual(expectedVariance, variance);
        }

        [TestMethod]
        public void StockCovariance_WithIdenticalValues()
        {
            var riskCalculator = new RiskCalculator();
            var prices1 = new List<decimal> { 100m, 200m, 300m };
            var prices2 = new List<decimal> { 100m, 200m, 300m };

            var covariance = riskCalculator.StockCovariance(prices1, prices2);

            var expectedCovariance = (decimal)((Math.Pow(100 - 200, 2) + Math.Pow(200 - 200, 2) + Math.Pow(300 - 200, 2)) / 2);
            Assert.AreEqual(expectedCovariance, covariance);
        }

        [TestMethod]
        public void StockCovariance_WithDifferentValues()
        {
            var riskCalculator = new RiskCalculator();
            var prices1 = new List<decimal> { 100m, 200m, 300m };
            var prices2 = new List<decimal> { 300m, 200m, 100m };

            var covariance = riskCalculator.StockCovariance(prices1, prices2);

            var expectedCovariance = (decimal)((((100 - 200) * (300 - 200)) + ((200 - 200) * (200 - 200)) + ((300 - 200) * (100 - 200))) / 2);
            Assert.AreEqual(expectedCovariance, covariance);
        }

        [TestMethod]
        public void StockCovariance_WithMismatchedLengths()
        {
            var riskCalculator = new RiskCalculator();
            var prices1 = new List<decimal> { 100m, 200m };
            var prices2 = new List<decimal> { 300m, 200m, 100m };

            var covariance = riskCalculator.StockCovariance(prices1, prices2);

            var expectedCovariance = (decimal)((((100 - 150) * (300 - 200)) + ((200 - 150) * (200 - 200))) / 1);
            Assert.AreEqual(expectedCovariance, covariance);
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

            var expectedVariance = (decimal)2062.37940;
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

            var expectedVariance = (decimal)-120.02043;
            Assert.AreEqual(expectedVariance, Math.Round(Covariance, 5));
        }
    }
}
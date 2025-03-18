
using RiskCalculations;
namespace UnitTests
{

    // my collection of unit tests designed to test the function of the statistical analysis algorithms in the risk calculations class.

    [TestClass]
    public class Calculation_Tests
    {
        [TestMethod]
        public void StockVariance_WithSingleValue() //tests that if the list is too short it returns 0
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
            public void StockVariance_WithMultipleValues()  // checks for a standard variance calculation matching the algorithm
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
            public void StockCovariance_WithIdenticalValues() // checking if covariance works with identical data sets incase of any divide by 0 errors
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
            public void StockCovariance_WithDifferentValues() // a standard covariance calculation check
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
            public void StockCovariance_WithMismatchedLengths() // checks if it works with lists of different lengths
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

        // Here are my tests that use large sets of data to check it works with large quantities of data        

        [TestMethod]
        public void StockVariance_WithLargeDataSet()
        {
            var riskCalculator = new RiskCalculator();
            string filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_data.csv"; //excel spreadsheet file which contains large lists of stock data
            List<decimal> closeValues = new List<decimal>();
            using (var reader = new StreamReader(filePath)) //opening a file reader that closes after use.
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null)     // makes sure the file is read until it reaches an empty line
                {
                    if (isHeader)           // removes the first line which is a header
                    {
                        isHeader = false;
                        continue;
                    }

                    var values = line.Split(','); //splits the data by commas
                    if (values.Length > 1)
                    {
                        if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue)) // trys removing dollar signs from the data and then converting them to decimals
                        {
                            closeValues.Add(closeValue);
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse value");  // catches any error with the try parse.
                        }
                    }
                }
            }
            var variance = riskCalculator.StockVariance(closeValues);

            var expectedVariance = (decimal)0.00035;
            Assert.AreEqual(expectedVariance, Math.Round(variance, 5));  // compares the algorithm value with the value given by excel
        }
        [TestMethod]
        public void StockCovariance_WithLargeDataSet()
        {
            var riskCalculator = new RiskCalculator();
            string filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_datacopy.csv"; //excel file with one set of stock data
            List<decimal> closeValues = new List<decimal>();
            using (var reader = new StreamReader(filePath))
            {
                string? line;
                bool isHeader = true;

                while ((line = reader.ReadLine()) != null) //reads the file until the end
                {
                    if (isHeader)
                    {                       // removing the header
                        isHeader = false;
                        continue;
                    }

                    var values = line.Split(',');   // split the data by commas
                    if (values.Length > 1)
                    {
                        if (decimal.TryParse(values[1].Trim('$'), out decimal closeValue))  // try trim the dollar signs from the data
                        {
                            closeValues.Add(closeValue);
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse value"); //catch any errors with the try parse converting data to decimals
                        }
                    }
                }
            }
            filePath = "C:\\Users\\ilias\\source\\repos\\NEA first draft\\UnitTests\\test_data2.csv"; // excel file with another set of stock data
            List<decimal> closeValues2 = new List<decimal>();
            using (var reader = new StreamReader(filePath)) //repeat of the same code but on a new file
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
                            Console.WriteLine("Failed to parse value");
                        }
                    }
                }
            }
            var Covariance = riskCalculator.StockCovariance(closeValues,closeValues2);

            var expectedVariance = (decimal)0.00027;
            Assert.AreEqual(expectedVariance, Math.Round(Covariance, 5));  //compare the computed value with the value from the spreadsheets.
        }
    }
}
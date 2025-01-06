using Microsoft.Data.SqlClient;
using System.Data;
using APIcalls;


namespace StockProSim.Data
{
    public class MyServerDb
    {
        private string _connectionString;

        public MyServerDb()
        {
            _connectionString = "Data Source=ILIAS_LAPTOP;Initial Catalog=Stock Simulator;Integrated Security=True;TrustServerCertificate=True";
        }

        public List<string> GetUserNames()
        {
            var names = new List<string>();
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "Select UserFirstName from dbo.Client";
                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var name = dataReader.GetString(0);
                    names.Add(name);
                }
            }
            return names;
        }

        public void AddUserName(string userName)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "insert into dbo.Client (UserFirstName) VALUES (@Name)";
                var nameParameter = command.Parameters.Add("@Name", SqlDbType.Text);
                nameParameter.Value = userName;
                command.ExecuteNonQuery();
            }
        }
        public async Task AddToWatchlist(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("Ticker cannot be null or empty.", nameof(ticker));

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO dbo.Watch_List (StockTicker) VALUES (@Ticker)";
                    command.Parameters.Add("@Ticker", SqlDbType.NVarChar).Value = ticker;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveFromWatchlist(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                throw new ArgumentException("Ticker cannot be null or empty.", nameof(ticker));

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM dbo.Watch_List WHERE StockTicker = @Ticker";
                    command.Parameters.Add("@Ticker", SqlDbType.NVarChar).Value = ticker;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<string>> GetWatchlistAsync()
        {
            List<string> watchlist = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT StockTicker FROM dbo.Watch_List";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            watchlist.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return watchlist;
        }
        public async Task BuyStockAsync(string ticker, decimal priceBought, decimal currentPrice, decimal priceChange, int quantity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker", connection);
                checkCommand.Parameters.AddWithValue("@Ticker", ticker);

                var result = await checkCommand.ExecuteScalarAsync();

                if (result != null)
                {
                    var updateCommand = new SqlCommand(
                        "UPDATE dbo.TradeHistory SET Quantity = Quantity + @Quantity, CurrentPrice = @PriceBought WHERE StockTicker = @Ticker",
                        connection);
                    updateCommand.Parameters.AddWithValue("@Ticker", ticker);
                    updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                    updateCommand.Parameters.AddWithValue("@PriceBought", currentPrice);

                    await updateCommand.ExecuteNonQueryAsync();
                }
                else
                {
                    var insertCommand = new SqlCommand(
                        "INSERT INTO dbo.TradeHistory (StockTicker, PriceBought, CurrentPrice, PriceChange, Quantity) VALUES (@Ticker, @PriceBought, @CurrentPrice, @PriceChange, @Quantity)",
                        connection);
                    insertCommand.Parameters.AddWithValue("@Ticker", ticker);
                    insertCommand.Parameters.AddWithValue("@PriceBought", priceBought);
                    insertCommand.Parameters.AddWithValue("@CurrentPrice", currentPrice);
                    insertCommand.Parameters.AddWithValue("@PriceChange", priceChange);
                    insertCommand.Parameters.AddWithValue("@Quantity", quantity);

                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SellStockAsync(string ticker, int quantity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker", connection);
                checkCommand.Parameters.AddWithValue("@Ticker", ticker);

                var result = await checkCommand.ExecuteScalarAsync();

                if (result != null)
                {
                    int currentQuantity = (int)result;
                    if (quantity > currentQuantity)
                    {
                        throw new InvalidOperationException("Cannot sell more than available quantity.");
                    }

                    if (quantity == currentQuantity)
                    {
                        var deleteCommand = new SqlCommand("DELETE FROM dbo.TradeHistory WHERE StockTicker = @Ticker", connection);
                        deleteCommand.Parameters.AddWithValue("@Ticker", ticker);

                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var updateCommand = new SqlCommand(
                            "UPDATE dbo.TradeHistory SET Quantity = Quantity - @Quantity WHERE StockTicker = @Ticker",
                            connection);
                        updateCommand.Parameters.AddWithValue("@Ticker", ticker);
                        updateCommand.Parameters.AddWithValue("@Quantity", quantity);

                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot sell a stock that does not exist.");
                }
            }
        }
        public async Task<List<TradeHistory>> GetTradeHistoryAsync()
        {
            List<TradeHistory> trades = new List<TradeHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT * FROM dbo.TradeHistory", connection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trades.Add(new TradeHistory
                    {
                        Id = reader.GetInt32(0),
                        StockTicker = reader.GetString(1),
                        PriceBought = reader.GetDecimal(2),
                        CurrentPrice = reader.GetDecimal(2) + reader.GetDecimal(4),
                        PriceChange = reader.GetDecimal(4),
                        Quantity = reader.GetInt32(5),
                        TradeValue = (reader.GetDecimal(2) + reader.GetDecimal(4)) * reader.GetInt32(5)
                    });
                }
            }

            return trades;
        }

        public async Task<decimal> GetProfits()
        {
            decimal profits = 0;
            List<TradeHistory> trade = await GetTradeHistoryAsync();
            APICalls ApiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
            foreach (var trades in trade)
            {
                trades.CurrentPrice = await ApiCalls.FetchCurrentPriceAsync(trades.StockTicker);
                trades.PriceChange = trades.CurrentPrice - trades.PriceBought;
                trades.TradeValue = trades.CurrentPrice * trades.Quantity;
                profits = profits + trades.PriceChange * trades.Quantity;
            }
            return profits;
        }

    }
    public class TradeHistory
    {
        public int Id { get; set; }
        public string StockTicker { get; set; }
        public decimal PriceBought { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PriceChange { get; set; }
        public int Quantity { get; set; }
        public decimal TradeValue { get; set; }
        public decimal TradeWeight { get; set; }
    }

}

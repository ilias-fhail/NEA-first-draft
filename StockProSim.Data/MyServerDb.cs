using Microsoft.Data.SqlClient;
using System.Data;
using APIcalls;
using System.Text;
using System.Security.Cryptography;


namespace StockProSim.Data
{

    // here is all my database code which manages all the data that is inserted, read and deleted from my database.
    public class MyServerDb
    {
        private string _connectionString;

        public MyServerDb()
        {
            _connectionString = "Data Source=ILIAS_LAPTOP;Initial Catalog=Stock Simulator;Integrated Security=True;TrustServerCertificate=True";  // here is the connection string that connects my device to my databse remotely
        }

        public List<string> GetUserNames()  // this function returns a list of usernames who have registerd
        {
            var names = new List<string>();
            using (SqlConnection connection = new SqlConnection())  
            {
                connection.ConnectionString = _connectionString;
                connection.Open();                                         // opens a temparary connection with the database
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "Select Username from dbo.User";      // select SQL query for returning all the usernames in the user table
                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var name = dataReader.GetString(0);  // gets the first string which is the name
                    names.Add(name); 
                }
            }
            return names; // returning a list of names
        }
        public async Task AddToWatchlist(string ticker, int userID) // Inserts a stock ticker into the watch list with the user ID it corresponds to
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO dbo.Watch_List (StockTicker, UserID) VALUES (@Ticker, @UserID)"; //SQL query for insering into the collumns StockTicker and userID into the Watch_List table
                    command.Parameters.Add("@Ticker", SqlDbType.NVarChar).Value = ticker; // fills the paramters of the SQL wuery with the correct variables.
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    await command.ExecuteNonQueryAsync(); // await for the query to be completed first.
                }
            }
        }


        public async Task RemoveFromWatchlist(string ticker, int userID) //removes a stock ticker from the watch list table which corresponds to the correct user ID
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM dbo.Watch_List WHERE StockTicker = @Ticker AND UserID = @UserID"; // SQL query to delete the field from the watch list table where the stock ticker and user ID match
                    command.Parameters.Add("@Ticker", SqlDbType.NVarChar).Value = ticker;
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<List<string>> GetWatchlistAsync(int userID) // fetches a list of tickers that are part of the watch list of a specific user ID
        {
            List<string> watchlist = new List<string>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT StockTicker FROM dbo.Watch_List WHERE UserID = @UserID"; //SQL query to select all the Stock Ticker collumn values for the fields where the UserID match the userID specified
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            watchlist.Add(reader.GetString(0)); // adds each string to the list before it is returned
                        }
                    }
                }
            }

            return watchlist;
        }

        public async Task BuyStockAsync(string ticker, decimal priceBought, decimal currentPrice, decimal priceChange, int quantity, int userID) // this subroutine inserts a field that represents the purchase of a stock linked to a specific user ID
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);  // SQL query to select the quanitity column of a field where the stock ticker and user ID match
                checkCommand.Parameters.AddWithValue("@Ticker", ticker);
                checkCommand.Parameters.AddWithValue("@UserID", userID);

                var result = await checkCommand.ExecuteScalarAsync();

                try
                {
                    if (result != null) // this checks if the stock already exists in the database as if it does a quanitity will be returned from the previous call. below is the code if it does already exist
                    {
                        var getOldValuesCommand = new SqlCommand(
                            "SELECT PriceBought FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", // gets the old price the stock was bought to calculate a new average at which the stock has now been bought at
                            connection);
                        getOldValuesCommand.Parameters.AddWithValue("@Ticker", ticker);
                        getOldValuesCommand.Parameters.AddWithValue("@UserID", userID);
                        var oldpricebought = await getOldValuesCommand.ExecuteScalarAsync();

                        int oldQuantity = (int)result;
                        decimal oldPriceBought = (decimal)oldpricebought;
                        int maxQuantity = int.MaxValue;
                        int totalQuantity = oldQuantity + quantity;
                        if (totalQuantity > maxQuantity)
                        {
                            throw new InvalidOperationException($"Quantity cannot exceed {maxQuantity}."); // check if too much of a stock has been bought that it can no longer be stored as an integer
                        }
                        else
                        {
                            decimal newAveragePrice = ((oldQuantity * oldPriceBought) + (quantity * currentPrice)) / totalQuantity;

                            var updateCommand = new SqlCommand(
                                "UPDATE dbo.TradeHistory SET Quantity = @TotalQuantity, PriceBought = @NewAveragePrice WHERE StockTicker = @Ticker AND UserID = @UserID", // update the trade with a new quantity and the updated average price
                                connection);
                            updateCommand.Parameters.AddWithValue("@Ticker", ticker);
                            updateCommand.Parameters.AddWithValue("@TotalQuantity", totalQuantity);
                            updateCommand.Parameters.AddWithValue("@NewAveragePrice", newAveragePrice);
                            updateCommand.Parameters.AddWithValue("@UserID", userID);

                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        var insertCommand = new SqlCommand(
                            "INSERT INTO dbo.TradeHistory (StockTicker, PriceBought, CurrentPrice, PriceChange, Quantity, UserID) VALUES (@Ticker, @PriceBought, @CurrentPrice, @PriceChange, @Quantity, @UserID)", // if it doesnt already exist in the trade table insert it from new.
                            connection);
                        insertCommand.Parameters.AddWithValue("@Ticker", ticker);
                        insertCommand.Parameters.AddWithValue("@PriceBought", priceBought);
                        insertCommand.Parameters.AddWithValue("@CurrentPrice", currentPrice);
                        insertCommand.Parameters.AddWithValue("@PriceChange", priceChange);
                        insertCommand.Parameters.AddWithValue("@Quantity", quantity);
                        insertCommand.Parameters.AddWithValue("@UserID", userID);

                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547) // Error number for constraint violations. Catching any other errors that deal with numbers being too big that they can no longer be formatted in their current data structures.
                    {
                        throw new InvalidOperationException("The quantity value you are trying to enter is too large or violates a constraint.");
                    }
                    else
                    {
                        throw new Exception("An error occurred while processing your request.", ex);
                    }
                }
            }
        }



        public async Task SellStockAsync(string ticker, int quantity, int userID)
        {
            APICalls ApiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
            decimal currentP = await ApiCalls.FetchCurrentPriceAsync(ticker);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);
                checkCommand.Parameters.AddWithValue("@Ticker", ticker);
                checkCommand.Parameters.AddWithValue("@UserID", userID);

                var result = await checkCommand.ExecuteScalarAsync();

                if (result != null)
                {
                    int currentQuantity = (int)result;
                    if (quantity > currentQuantity)
                    {
                        throw new InvalidOperationException("Cannot sell more than available quantity.");
                    }
                    var getPriceCommand = new SqlCommand("SELECT PriceBought FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);
                    getPriceCommand.Parameters.AddWithValue("@Ticker", ticker);
                    getPriceCommand.Parameters.AddWithValue("@UserID", userID);

                    var price = await getPriceCommand.ExecuteScalarAsync();
                    decimal priceChange = currentP - (decimal)price;
                    Console.WriteLine(priceChange);
                    decimal totalProfit = priceChange * quantity;
                    Console.WriteLine(quantity + " and " + totalProfit);

                    await AddBaseProfitAsync(totalProfit, userID);

                    if (quantity == currentQuantity)
                    {
                        var deleteCommand = new SqlCommand("DELETE FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);
                        deleteCommand.Parameters.AddWithValue("@Ticker", ticker);
                        deleteCommand.Parameters.AddWithValue("@UserID", userID);

                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var updateCommand = new SqlCommand(
                            "UPDATE dbo.TradeHistory SET Quantity = Quantity - @Quantity WHERE StockTicker = @Ticker AND UserID = @UserID",
                            connection);
                        updateCommand.Parameters.AddWithValue("@Ticker", ticker);
                        updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                        updateCommand.Parameters.AddWithValue("@UserID", userID);

                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot sell a stock that does not exist.");
                }
            }
        }

        public async Task<List<TradeHistory>> GetTradeHistoryAsync(int userID)
        {
            List<TradeHistory> trades = new List<TradeHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT * FROM dbo.TradeHistory WHERE UserID = @UserID", connection);
                command.Parameters.AddWithValue("@UserID", userID);

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


        public bool AddUser(string username, string password)
        {
            var hashedPassword = HashPassword(password);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (IsUsernameExists(username) == false)
                {
                    var query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                        try
                        {
                            command.ExecuteNonQuery();
                            return true;
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            return false;
                        }
                    }
                }
                else { return false; }
            }
        }

        public bool IsUsernameExists(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool ValidateUser(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    var storedPasswordHash = command.ExecuteScalar() as string;
                    if (storedPasswordHash == null)
                    {
                        return false;
                    }

                    return VerifyPassword(password, storedPasswordHash);
                }
            }
        }
        public int GetUserID(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT userID FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);


                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public string GetUsername(int userId)
        {
            string username = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Username FROM Users WHERE UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        var result = command.ExecuteScalar();

                        if (result != null)
                        {
                            username = result.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return username ?? "User not found";
        }


        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        public async Task<decimal> GetProfits(int userID)
        {
            decimal profits = await GetBaseProfitAsync(userID);
            List<TradeHistory> trade = await GetTradeHistoryAsync(userID);
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
        public async Task AddData(List<StockEntry> stockEntries)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "INSERT INTO StockData (Ticker, Date, Price) VALUES (@Ticker, @Date, @Price)";

                foreach (var stockEntry in stockEntries)
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ticker", stockEntry.Ticker);
                        command.Parameters.AddWithValue("@Date", stockEntry.Date);
                        command.Parameters.AddWithValue("@Price", stockEntry.ClosePrice);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }


        public async Task DeleteData(string ticker)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM StockData WHERE Ticker = @Ticker";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> DataExists(string ticker)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(1) FROM StockData WHERE Ticker = @Ticker";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task<bool> IsDataWeekOld(string ticker)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT MAX(Date) FROM StockData WHERE Ticker = @Ticker";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    var result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value && result is DateTime lastDate)
                    {
                        return (DateTime.UtcNow - lastDate).TotalDays > 7;
                    }
                    return false;
                }
            }
        }

        public async Task<List<decimal>> GetData(string ticker)
        {
            var priceList = new List<decimal>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Price FROM StockData WHERE Ticker = @Ticker ORDER BY Date";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            priceList.Add(reader.GetDecimal(0));
                        }
                    }
                }
            }
            return priceList;
        }

        public void AddProfit(DateTime date, decimal number, int userID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM Profits WHERE RecordDate = @RecordDate AND UserID = @UserID";
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@RecordDate", date);
                    checkCommand.Parameters.AddWithValue("@UserID", userID);

                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        string insertQuery = "INSERT INTO Profits (RecordDate, Value, UserID) VALUES (@RecordDate, @Value, @UserID)";
                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@RecordDate", date);
                            insertCommand.Parameters.AddWithValue("@Value", number);
                            insertCommand.Parameters.AddWithValue("@UserID", userID);

                            insertCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"A record with the date {date:yyyy-MM-dd} already exists for UserID {userID}.");
                    }
                }
            }
        }

        public List<ProfitPoint> GetAllProfits(int userID)
        {
            var profits = new List<ProfitPoint>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Profits WHERE UserID = @UserID", connection);
                command.Parameters.AddWithValue("@UserID", userID);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        profits.Add(new ProfitPoint
                        {
                            Date = reader.GetDateTime(reader.GetOrdinal("RecordDate")),
                            Value = reader.GetDecimal(reader.GetOrdinal("Value"))
                        });
                    }
                }
            }

            return profits;
        }

        public async Task AddBaseProfitAsync(decimal baseProfit, int userID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM BaseProfits WHERE UserID = @UserID";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserID", userID);
                    int count = (int)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        string updateQuery = "UPDATE BaseProfits SET BaseProfit = @BaseProfit WHERE UserID = @UserID";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@BaseProfit", baseProfit);
                            updateCommand.Parameters.AddWithValue("@UserID", userID);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO BaseProfits (BaseProfit, UserID) VALUES (@BaseProfit, @UserID)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@BaseProfit", baseProfit);
                            insertCommand.Parameters.AddWithValue("@UserID", userID);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
        public async Task<decimal> GetBaseProfitAsync(int userID)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT BaseProfit FROM BaseProfits WHERE UserID = @UserID";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@UserID", userID);
                    object result = await selectCommand.ExecuteScalarAsync();

                    if (result != null)
                    {
                        return (decimal)result;
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO BaseProfits (BaseProfit, UserID) VALUES (0, @UserID)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@UserID", userID);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                        return 0;
                    }
                }
            }
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
    public class StockEntry
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public decimal ClosePrice { get; set; }
    }
    public class ProfitPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

}

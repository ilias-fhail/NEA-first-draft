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
                    command.CommandText = "DELETE FROM dbo.Watch_List WHERE StockTicker = @Ticker AND UserID = @UserID"; // SQL query to delete the record from the watch list table where the stock ticker and user ID match
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
                string query = "SELECT StockTicker FROM dbo.Watch_List WHERE UserID = @UserID"; //SQL query to select all the Stock Ticker collumn values for the records where the UserID match the userID specified
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

        public async Task BuyStockAsync(string ticker, decimal priceBought, decimal currentPrice, decimal priceChange, int quantity, int userID) // this subroutine inserts a record that represents the purchase of a stock linked to a specific user ID
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);  // SQL query to select the quanitity column of a record where the stock ticker and user ID match
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



        public async Task SellStockAsync(string ticker, int quantity, int userID) // sell a stock from the databse rather deleting it or adjusting the quantity
        {
            APICalls ApiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
            decimal currentP = await ApiCalls.FetchCurrentPriceAsync(ticker);
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkCommand = new SqlCommand("SELECT Quantity FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection); // SQL query for selecting the quantity associated with the stock ticker
                checkCommand.Parameters.AddWithValue("@Ticker", ticker);
                checkCommand.Parameters.AddWithValue("@UserID", userID);

                var result = await checkCommand.ExecuteScalarAsync();

                if (result != null) //checks if the stock ticker exists as it would have in an invalid result if it didnt
                {
                    int currentQuantity = (int)result;
                    if (quantity > currentQuantity) //checking if the request is to sell more stock than what is already bought.
                    {
                        throw new InvalidOperationException("Cannot sell more than available quantity.");
                    }
                    var getPriceCommand = new SqlCommand("SELECT PriceBought FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);
                    getPriceCommand.Parameters.AddWithValue("@Ticker", ticker);
                    getPriceCommand.Parameters.AddWithValue("@UserID", userID);

                    var price = await getPriceCommand.ExecuteScalarAsync();
                    decimal priceChange = currentP - (decimal)price; //calculating the profit made from selling the stocks now that they are liquid assets
                    Console.WriteLine(priceChange);
                    decimal totalProfit = priceChange * quantity;
                    Console.WriteLine(quantity + " and " + totalProfit);

                    await AddBaseProfitAsync(totalProfit, userID);

                    if (quantity == currentQuantity) //checking id after selling the stock the quantity is now at zero indicating no stock is held
                    {
                        var deleteCommand = new SqlCommand("DELETE FROM dbo.TradeHistory WHERE StockTicker = @Ticker AND UserID = @UserID", connection);
                        deleteCommand.Parameters.AddWithValue("@Ticker", ticker);
                        deleteCommand.Parameters.AddWithValue("@UserID", userID);

                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                    else
                    {                                                   //otherwise just adjust the new quantity of stock
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
                    throw new InvalidOperationException("Cannot sell a stock that does not exist.");     //exception to catch invalid requests
                }
            }
        }

        public async Task<List<TradeHistory>> GetTradeHistoryAsync(int userID) // gets the list of trades from the databse associated with the inputted user ID
        {
            List<TradeHistory> trades = new List<TradeHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT * FROM dbo.TradeHistory WHERE UserID = @UserID", connection);  // SQL query that selects all the records where the user ID matches
                command.Parameters.AddWithValue("@UserID", userID);

                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trades.Add(new TradeHistory             // formatting the returned data into custom object classes to be processed
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


        public bool AddUser(string username, string password)  // register a user to the database.
        {
            var hashedPassword = HashPassword(password); // hash the password immediately for privacy

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                if (IsUsernameExists(username) == false)
                {
                    var query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)"; // SQL query that creates a new row in the Users Table with the entered username and hashed passowrd
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
                            Console.WriteLine($"Error: {ex.Message}"); // catches and logs any error that might happen in the console.
                            return false;
                        }
                    }
                }
                else { return false; }
            }
        }

        public bool IsUsernameExists(string username) // checks if the same username exists
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM Users WHERE Username = @Username"; //SQL query that counts how many times in the entire table where the username parameter matches the one specified.
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    var count = (int)command.ExecuteScalar();
                    return count > 0; //returns a boolean value depending if it has appeared at all or not
                }
            }
        }

        public bool ValidateUser(string username, string password) // log in function that checks the username and password match
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT PasswordHash FROM Users WHERE Username = @Username"; // SQL query that gets the stored hash password for the matching username 
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    var storedPasswordHash = command.ExecuteScalar() as string;
                    if (storedPasswordHash == null) //checks that a value is returned
                    {
                        return false;
                    }

                    return VerifyPassword(password, storedPasswordHash); //compares the stored value and the hashed value of the entered password by calling a subroutine.
                }
            }
        }
        public int GetUserID(string username) // gets the ID associated with the logged in user
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT userID FROM Users WHERE Username = @Username"; // SQL query that gets the userID for the row containing the matching username
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);


                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public string GetUsername(int userId) //gets the username associated with the logged in userID (variation of the previous function)
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


        private string HashPassword(string password) // hashing function for encrypting the password.
        {
            using (var sha256 = SHA256.Create()) // implementing system.security.cryptography to hash it
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // converting the password into an array of bytes which is how SHA256 encrypts before being encrypted
                return Convert.ToBase64String(bytes); //converted back to a string to be put into the database
            }
        }

        private bool VerifyPassword(string password, string storedHash) //checks if the two inputted values match after the non hashed one is hashed
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        public async Task<decimal> GetProfits(int userID)  // gets the total trade profits associated with the inputted user ID
        {
            decimal profits = await GetBaseProfitAsync(userID);
            List<TradeHistory> trade = await GetTradeHistoryAsync(userID);
            APICalls ApiCalls = new APICalls("https://finnhub.io/api/v1", "cpnv24hr01qru1ca7qdgcpnv24hr01qru1ca7qe0");
            foreach (var trades in trade)                                                           // using an API to update the values of the trades to ensure that they are up to date before computing profits.
            {
                trades.CurrentPrice = await ApiCalls.FetchCurrentPriceAsync(trades.StockTicker);
                trades.PriceChange = trades.CurrentPrice - trades.PriceBought;
                trades.TradeValue = trades.CurrentPrice * trades.Quantity;
                profits = profits + trades.PriceChange * trades.Quantity;
            }
            return profits;
        }
        public async Task AddData(List<StockEntry> stockEntries) // inserts  a list of historical data into the database
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "INSERT INTO StockData (Ticker, Date, Price) VALUES (@Ticker, @Date, @Price)"; // initialising the SQL query to insert a new historical data row with a timestamp

                foreach (var stockEntry in stockEntries)
                {
                    using (SqlCommand command = new SqlCommand(query, connection)) //inserting each value in the list into the database
                    {
                        command.Parameters.AddWithValue("@Ticker", stockEntry.Ticker);
                        command.Parameters.AddWithValue("@Date", stockEntry.Date);
                        command.Parameters.AddWithValue("@Price", stockEntry.ClosePrice);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }


        public async Task DeleteData(string ticker) // deleting any old data for a certain stock ticker
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM StockData WHERE Ticker = @Ticker"; // deletes any row where the tickers match
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> DataExists(string ticker) // checks if the stock ticker exists in the historical databse already
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(*) FROM StockData WHERE Ticker = @Ticker"; // SQL wuery that counts how many times it can find the ticker
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;  // returning a boolean value if it is present or not.
                }
            }
        }

        public async Task<bool> IsDataWeekOld(string ticker)  //checks if the timestamp of the data is a week old or not
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT MAX(Date) FROM StockData WHERE Ticker = @Ticker"; //finds the latest date with that stock ticker
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    var result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value && result is DateTime lastDate) // if a value was found and checks if it is the right data type and then assigns it the variable last date.
                    {
                        return (DateTime.UtcNow - lastDate).TotalDays > 7; // compares the gap in days to today to 7
                    }
                    return false;
                }
            }
        }

        public async Task<List<decimal>> GetData(string ticker) // gets a list of historical stock prices for a particular ticker
        {
            var priceList = new List<decimal>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Price FROM StockData WHERE Ticker = @Ticker ORDER BY Date"; // SQL query that selects the price and orders it by date to ensure the returns are calculated right
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ticker", ticker);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            priceList.Add(reader.GetDecimal(0));    // add to a list before returning
                        }
                    }
                }
            }
            return priceList;
        }

        public void AddProfit(DateTime date, decimal number, int userID) // adds a profit stamp to the table tracking profit progress for each user
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string checkQuery = "SELECT COUNT(*) FROM Profits WHERE RecordDate = @RecordDate AND UserID = @UserID"; // SQL query that counts if a profit stamp has already been entered for that day.
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@RecordDate", date);
                    checkCommand.Parameters.AddWithValue("@UserID", userID);

                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0) //if it hasnt:
                    {
                        string insertQuery = "INSERT INTO Profits (RecordDate, Value, UserID) VALUES (@RecordDate, @Value, @UserID)"; //insert the new stamp into the table
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
                        Console.WriteLine($"A record with the date {date:yyyy-MM-dd} already exists for UserID {userID}.");     // for any other reason log an error message.
                    }
                }
            }
        }

        public List<ProfitPoint> GetAllProfits(int userID) // returns a list of all profits linked to a userID
        {
            var profits = new List<ProfitPoint>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Profits WHERE UserID = @UserID", connection); // SQL query taht selects the entire record 
                command.Parameters.AddWithValue("@UserID", userID);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        profits.Add(new ProfitPoint                                     // stores each record as a custom object class which is then added to a list.
                        {
                            Date = reader.GetDateTime(reader.GetOrdinal("RecordDate")), 
                            Value = reader.GetDecimal(reader.GetOrdinal("Value"))
                        });
                    }
                }
            }

            return profits;
        }

        public async Task AddBaseProfitAsync(decimal baseProfit, int userID) // this manages the "base profit" which is profit made from previous sales of stock and not due to a current holding appreciating
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM BaseProfits WHERE UserID = @UserID"; // SQL query counting how many times a base profit exists with the matching user ID
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserID", userID);
                    int count = (int)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        string updateQuery = "UPDATE BaseProfits SET BaseProfit = @BaseProfit WHERE UserID = @UserID"; // if it does exist update the base profit with the new value 
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@BaseProfit", baseProfit);
                            updateCommand.Parameters.AddWithValue("@UserID", userID);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO BaseProfits (BaseProfit, UserID) VALUES (@BaseProfit, @UserID)"; // if it doesnt insert a new base profit linked to the user id into the table
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
        public async Task<decimal> GetBaseProfitAsync(int userID)// Gets the vase profit value linked to the inputted userID from the database
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string selectQuery = "SELECT BaseProfit FROM BaseProfits WHERE UserID = @UserID"; // selects the baseprofit where the userID matches
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@UserID", userID);
                    object result = await selectCommand.ExecuteScalarAsync();

                    if (result != null) // checks that a valid input was returned
                    {
                        return (decimal)result;
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO BaseProfits (BaseProfit, UserID) VALUES (0, @UserID)"; // if a base profit doesnt exist fill an empty one with 0 as the base profit
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
    public class TradeHistory // custom object class for trades
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
    public class StockEntry // custom object class for historical data points
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public decimal ClosePrice { get; set; }
    }
    public class ProfitPoint  //custom object class for profit stamps.
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

}

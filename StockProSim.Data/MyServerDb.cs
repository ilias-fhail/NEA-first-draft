using Microsoft.Data.SqlClient;
using System.Data;


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
    }
}

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
        public void AddToWatchlist(string ticker)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "insert into dbo.Watch_List (StockTicker) VALUES (@Name)";
                var nameParameter = command.Parameters.Add("@Name", SqlDbType.Text);
                nameParameter.Value = ticker;
                command.ExecuteNonQuery();
            }
        }
        public void RemoveFromWatchlist(string ticker)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM dbo.Watch_List WHERE StockTicker = @Name";
                var nameParameter = command.Parameters.Add("@Name", SqlDbType.Text);
                nameParameter.Value = ticker;
                command.ExecuteNonQuery();
            }
        }
    }
}

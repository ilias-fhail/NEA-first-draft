using Microsoft.Data.SqlClient;
using System.Data;


namespace StockProSim.Data
{
    public class MyServerDb
    {
        private string _connectionString;

        public MyServerDb(string connectionString)
        {
            _connectionString = connectionString;
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
                command.CommandText = "insert into dbo."
            }
        }

    }
}

using StockProSim.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace NEA_first_draft
{
    internal class Program
    {
        static void Main()
        {
            const string ConnectionString = "Data Source=ILIAS_LAPTOP;Initial Catalog=Stock Portfolio;Integrated Security=True;TrustServerCertificate=True";

            Console.WriteLine(ConnectionString);
            MyServerDb db = new(ConnectionString);

            var names = db.GetUserNames();
            Console.WriteLine("User Names:");
            foreach (var name in names)
            {
                Console.WriteLine(name);
            }
            Console.WriteLine();

            Console.WriteLine("Please enter a name to add:");
            string newName = Console.ReadLine() ?? string.Empty;
            db.AddUserName(newName);
        }
    }
}

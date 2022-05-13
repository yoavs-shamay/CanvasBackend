using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace ResetCanvas
{
    class Program
    {
        public const bool Reset = false;
        static async Task Main(string[] args)
        {
            if (Reset)
            {
                var connectionString = "server=remotemysql.com;userid=xwcWQw3nCm;password=D6mQvJnLV5;database=xwcWQw3nCm;CharSet=utf8;";
                int width = 100, height = 100;
                var connection = new MySqlConnection(connectionString);
                connection.Open();
                var removeCanvasQuery = "TRUNCATE TABLE pixels;";
                var removeCanvasCmd = new MySqlCommand(removeCanvasQuery, connection);
                removeCanvasCmd.ExecuteNonQuery();
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        var query = $"INSERT INTO pixels(Red, Green, Blue, LastModifier, X, Y) VALUES(255, 255, 255, \"\", {i}, {j});";
                        var cmd = new MySqlCommand(query, connection);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}

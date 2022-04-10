using MySql.Data.MySqlClient;
using System;

namespace ResetCanvas
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "server=canvas-db.mysql.database.azure.com;userid=yodi555;password=1234qwer!@#$;database=Canvas;CharSet=utf8;";
            int width = 100, height = 100;
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            var removeCanvasQuery = "TRUNCATE TABLE Pixels;";
            var removeCanvasCmd = new MySqlCommand(removeCanvasQuery, connection);
            removeCanvasCmd.ExecuteNonQuery();
            for (int i=0; i < width; i++)
            {
                for (int j=0; j < height; j++)
                {
                    var query = $"INSERT INTO Pixels(Red, Green, Blue, LastModifier, X, Y) VALUES(255, 255, 255, \"\", {i}, {j});";
                    var cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

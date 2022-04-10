using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Canvas
{
    public class DatabaseOperations
    {
        private MySqlConnection _connection;
        private string _pixelsTableName = "Pixels";
        private string _usersTableName = "Users";
        public DatabaseOperations(IOptions<DatabaseOptions> options)
        {
            var dbOptions = options.Value;
            string connectionString = $"server={dbOptions.Server};userid={dbOptions.UserId};password={dbOptions.Password};database={dbOptions.Database};charSet={dbOptions.CharSet}";
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        public Pixel[,] GetAllPixels(int height, int width)
        {
            string query = "SELECT * FROM Pixels";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            MySqlDataReader reader = cmd.ExecuteReader();
            Pixel[,] result = new Pixel[width, height];
            while (reader.Read())
            {
                var red = reader.GetInt32(1);
                var green = reader.GetInt32(2);
                var blue = reader.GetInt32(3);
                var lastModifier = reader.GetString(4);
                var x = reader.GetInt32(5);
                var y = reader.GetInt32(6);
                var pixel = new Pixel(red, green, blue, lastModifier, x, y);
                result[x, y] = pixel;
            }
            reader.Close();
            return result;
        }

        public void ReplacePixel(int x, int y,  int newRed, int newGreen, int newBlue, string sessionId)
        {
            var getUserQuery = $"SELECT * FROM {_usersTableName} WHERE sessionId = \"{sessionId}\";";
            var getUserCmd = new MySqlCommand(getUserQuery, _connection);
            var reader = getUserCmd.ExecuteReader();
            string name;
            if (reader.Read())
            {
                name = reader.GetString(1);
                reader.Close();
            }
            else
            {
                reader.Close();
                return;
            }
            var replaceQuery = $"UPDATE {_pixelsTableName} SET LastModifier = \"{name}\", Red = {newRed}, Green = {newGreen}, Blue = {newBlue} WHERE X = {x} AND y = {y};";
            var cmd = new MySqlCommand(replaceQuery, _connection);
            cmd.ExecuteNonQuery();
            var time = DateTime.Now.ToString();
            var updateQuery = $"UPDATE {_usersTableName} SET LastUpdate = \"{time}\" WHERE SessionId = \"{sessionId}\";";
            var updateCmd = new MySqlCommand(updateQuery, _connection);
            updateCmd.ExecuteNonQuery();
        }

        public string Login(string userId)
        {
            var sessionId = Guid.NewGuid().ToString();
            var updateQuery = $"UPDATE {_usersTableName} SET sessionId = \"{sessionId}\" WHERE userId = \"{userId}\";";
            var cmd = new MySqlCommand(updateQuery, _connection);
            cmd.ExecuteNonQuery();
            return sessionId;
        }

        public bool DoesUserExist(string userId)
        {
            var query = $"SELECT EXISTS(SELECT 1 FROM {_usersTableName} WHERE userId = \"{userId}\" LIMIT 1);";
            var cmd = new MySqlCommand(query, _connection);
            var result = ((Int64)cmd.ExecuteScalar()) == 1;
            return result;
        }

        public string Register (string userId, string userName)
        {
            var sessionId = Guid.NewGuid().ToString();
            var lastUpdate = DateTime.MinValue.ToString();
            var q = $"INSERT INTO {_usersTableName}(UserId, Username, SessionId, LastUpdate) VALUES(\"{userId}\", \"{userName}\", \"{sessionId}\", \"{lastUpdate}\");";
            var cmd = new MySqlCommand(q, _connection);
            cmd.ExecuteNonQuery();
            return sessionId;
        }

        public void Logout(string sessionId)
        {
            var q = $"UPDATE {_usersTableName} SET sessionId = \"\" WHERE sessionId = \"{sessionId}\";";
            var cmd = new MySqlCommand(q, _connection);
            cmd.ExecuteNonQuery();
        }

        public double GetRemainingTime(string sessionId)
        {
            var findUserQuery = $"SELECT * FROM {_usersTableName} WHERE sessionId = \"{sessionId}\"";
            var cmd = new MySqlCommand(findUserQuery, _connection);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var lastUpdate = reader.GetString(3);
                reader.Close();
                var minutesFromLastUpdate = DateTime.Now.Subtract(DateTime.Parse(lastUpdate)).TotalMinutes;
                return 5 - minutesFromLastUpdate;
            }
            reader.Close();
            return -1;
        }

        public bool IsValidSession(string sessionId)
        {
            var query = $"SELECT EXISTS(SELECT 1 FROM {_usersTableName} WHERE sessionId = \"{sessionId}\" LIMIT 1);";
            var cmd = new MySqlCommand(query, _connection);
            var result = ((Int64)cmd.ExecuteScalar()) == 1;
            return result;
        }
    }
}

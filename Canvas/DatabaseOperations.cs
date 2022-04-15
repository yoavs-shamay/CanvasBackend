using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<Pixel[,]> GetAllPixels(int height, int width)
        {
            string query = "SELECT * FROM Pixels";
            MySqlCommand cmd = new MySqlCommand(query, _connection);
            var reader = (await cmd.ExecuteReaderAsync());
            Pixel[,] result = new Pixel[width, height];
            while (await reader.ReadAsync())
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
            await reader.CloseAsync();
            return result;
        }

        public async Task ReplacePixel(int x, int y,  int newRed, int newGreen, int newBlue, string sessionId)
        {
            var getUserQuery = $"SELECT * FROM {_usersTableName} WHERE sessionId = \"{sessionId}\";";
            var getUserCmd = new MySqlCommand(getUserQuery, _connection);
            var reader = await getUserCmd.ExecuteReaderAsync();
            string name;
            if (await reader.ReadAsync())
            {
                name = reader.GetString(1); //TODO async too?
                await reader.CloseAsync();
            }
            else
            {
                await reader.CloseAsync();
                return;
            }
            var replaceQuery = $"UPDATE {_pixelsTableName} SET LastModifier = \"{name}\", Red = {newRed}, Green = {newGreen}, Blue = {newBlue} WHERE X = {x} AND y = {y};";
            var cmd = new MySqlCommand(replaceQuery, _connection);
            await cmd.ExecuteNonQueryAsync();
            var time = DateTime.Now.ToString();
            var updateQuery = $"UPDATE {_usersTableName} SET LastUpdate = \"{time}\" WHERE SessionId = \"{sessionId}\";";
            var updateCmd = new MySqlCommand(updateQuery, _connection);
            await updateCmd.ExecuteNonQueryAsync();
        }

        public async Task<string> Login(string userId)
        {
            var sessionId = Guid.NewGuid().ToString();
            var updateQuery = $"UPDATE {_usersTableName} SET sessionId = \"{sessionId}\" WHERE userId = \"{userId}\";";
            var cmd = new MySqlCommand(updateQuery, _connection);
            await cmd.ExecuteNonQueryAsync();
            return sessionId;
        }

        public async Task<bool> DoesUserExist(string userId)
        {
            var query = $"SELECT EXISTS(SELECT 1 FROM {_usersTableName} WHERE userId = \"{userId}\" LIMIT 1);";
            var cmd = new MySqlCommand(query, _connection);
            var result = ((Int64)(await cmd.ExecuteScalarAsync())) == 1;
            return result;
        }

        public async Task<string> Register (string userId, string userName)
        {
            var sessionId = Guid.NewGuid().ToString();
            var lastUpdate = DateTime.MinValue.ToString();
            var q = $"INSERT INTO {_usersTableName}(UserId, Username, SessionId, LastUpdate) VALUES(\"{userId}\", \"{userName}\", \"{sessionId}\", \"{lastUpdate}\");";
            var cmd = new MySqlCommand(q, _connection);
            await cmd.ExecuteNonQueryAsync();
            return sessionId;
        }

        public async Task Logout(string sessionId)
        {
            var q = $"UPDATE {_usersTableName} SET sessionId = \"\" WHERE sessionId = \"{sessionId}\";";
            var cmd = new MySqlCommand(q, _connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<double> GetRemainingTime(string sessionId)
        {
            var findUserQuery = $"SELECT * FROM {_usersTableName} WHERE sessionId = \"{sessionId}\"";
            var cmd = new MySqlCommand(findUserQuery, _connection);
            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var lastUpdate = reader.GetString(3);
                await reader.CloseAsync();
                var minutesFromLastUpdate = DateTime.Now.Subtract(DateTime.Parse(lastUpdate)).TotalMinutes;
                return 5 - minutesFromLastUpdate;
            }
            await reader.CloseAsync();
            return -1;
        }

        public async Task<bool> IsValidSession(string sessionId)
        {
            var query = $"SELECT EXISTS(SELECT 1 FROM {_usersTableName} WHERE sessionId = \"{sessionId}\" LIMIT 1);";
            var cmd = new MySqlCommand(query, _connection);
            var result = ((Int64)(await cmd.ExecuteScalarAsync())) == 1;
            return result;
        }
    }
}

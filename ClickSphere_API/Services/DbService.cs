using System.Dynamic;
using Octonica.ClickHouseClient;

namespace ClickSphere_API.Services
{
    /**
     * This class is used to create a connection to the ClickHouse database
     * and to execute queries on the database
     */
    public class DbService : IDbService
    {
        private readonly ClickHouseConnectionStringBuilder? _connString;
        private string Host { get; set; }
        private ushort Port { get; set; }

        /**
         * This is the constructor for the DbService class
         */
        public DbService(IConfiguration configuration)
        {
            // check required config values
            if (configuration["ClickHouse:Host"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:Host");
            else
                Host = configuration["ClickHouse:Host"]!;

            if (configuration["ClickHouse:Port"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:Port");
            else
                Port = ushort.TryParse(configuration["ClickHouse:Port"], out var port) ? port : 
                throw new Exception("ClickHouse:Port is not a valid port number");
            
            if (configuration["ClickHouse:User"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:User");


            _connString = new ClickHouseConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                User = configuration["ClickHouse:User"]!,
                Password = configuration["ClickHouse:Password"],
                Database = configuration["ClickHouse:Database"]
            } ?? throw new Exception("ClickHouse connection string is not valid");

            InitializeDatabase().Wait();
        }

        /**
         * This method is used to create a connection to the ClickHouse database
         * @return A ClickHouseConnection object that represents the connection to the database
         */
        private ClickHouseConnection CreateConnection()
        {
            if(_connString == null)
                throw new Exception("ClickHouse connection string is not valid");
            
            return new ClickHouseConnection(_connString.ConnectionString);
        }

        /**
        * This method is used to check if the login credentials are valid
        * @param user The username of the user
        * @param password The password of the user
        */
        public async Task<bool> CheckLogin(string user, string password)
        {
            try
            {
                var connectionString = $"Host={Host};Port={Port};User={user};Password={password};";
                
                using var connection = new ClickHouseConnection(connectionString);
                await connection.OpenAsync();  // Try to open a connection
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";  // Try to execute a simple query
                await command.ExecuteScalarAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /**
         * This method is used to execute a query on the ClickHouse database
         * @param query The query to be executed
         * @return A list of strings that represent the result of the query
         */
        public async Task<List<string>> ExecuteQuery(string query)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var reader = await command.ExecuteReaderAsync();
            var result = new List<string>();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }
            return result;
        }

        /**
         * This method is used to execute a query on the ClickHouse database
         * @param query The query to be executed
         * @return A List of dictionary of <string, object> that represents the result of the query
         */
        public async Task<List<Dictionary<string, object>>> ExecuteQueryDictionary(string query)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var reader = await command.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            return result;
        }

        /**
         * This method is used to execute a query on the ClickHouse database which returns an object
         * @param query The query to be executed
         * @return T The result of the query
        */
        public async Task<T> ExecuteQueryObject<T>(string query) where T : class, new()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var reader = await command.ExecuteReaderAsync();
            var result = new T();
            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    property?.SetValue(result, reader.GetValue(i));
                }
            }
            return result;
        }

        /**
        * This method is used to execute a query on the ClickHouse database
        * @param query The query to be executed
        * @return A List of T that represents the result of the query
        */
        public async Task<List<T>> ExecuteQueryList<T>(string query) where T : class, new()
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var reader = await command.ExecuteReaderAsync();
            var result = new List<T>();
            while (await reader.ReadAsync())
            {
                var row = new T();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    property?.SetValue(row, reader.GetValue(i));
                }
                result.Add(row);
            }
            return result;
        }

        /**
         * This method is used to execute a non-query on the ClickHouse database
         * @param query The query to be executed
         */
        public async Task<int> ExecuteNonQuery(string query)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            return await command.ExecuteNonQueryAsync();
        }

        /**
         * This method is used to execute a scalar query on the ClickHouse database
         * @param query The query to be executed
         * @return The result of the query
         */
        public async Task<object?> ExecuteScalar(string query)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var result = await command.ExecuteScalarAsync();
            return result ?? DBNull.Value;
        }

        /**
        * This method is used to initialize the database
        */
        public async Task InitializeDatabase()
        {
            // create the database if it does not exist
            string query = "CREATE DATABASE IF NOT EXISTS ClickSphere";
            await ExecuteNonQuery(query);
            
            // create the users table if it does not exist
            query = "CREATE TABLE IF NOT EXISTS ClickSphere.Users (Id UUID, UserName String, LDAP_User String, FirstName String, LastName String, Phone String, Department String) ENGINE = MergeTree() ORDER BY Id";
            await ExecuteNonQuery(query);

            // create the views table if it does not exist
            query = "CREATE TABLE IF NOT EXISTS ClickSphere.Views (Id String, Name String, Description String, Type String) ENGINE = MergeTree() ORDER BY Id";
            await ExecuteNonQuery(query);
        }
    }
}
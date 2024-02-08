using Octonica.ClickHouseClient;

namespace ClickViews_API.Services
{
    /**
     * This class is used to create a connection to the ClickHouse database
     * and to execute queries on the database
     */
    public class DbService()
    {
        private readonly ClickHouseConnectionStringBuilder? _connString;

        /**
         * This is the constructor for the DbService class
         */
        public DbService(IConfiguration configuration) : this()
        {
            // check required config values
            if (configuration["ClickHouse:Host"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:Host");
            if (configuration["ClickHouse:Port"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:Port");
            if (configuration["ClickHouse:User"] == null)
                throw new ArgumentNullException(nameof(configuration), "ClickHouse:User");

            _connString = new ClickHouseConnectionStringBuilder
            {
                Host = configuration["ClickHouse:Host"],
                Port = Convert.ToUInt16(configuration["ClickHouse:Port"]),
                User = configuration["ClickHouse:User"]!,
                Password = configuration["ClickHouse:Password"],
                Database = configuration["ClickHouse:Database"]
            } ?? throw new Exception("ClickHouse connection string is not valid");
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
    }
}
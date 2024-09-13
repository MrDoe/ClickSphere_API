using Octonica.ClickHouseClient;
namespace ClickSphere_API.Services;

/// <summary>
/// This class is used to create a connection to the ClickHouse database
/// and to execute queries on the database
/// </summary>
public class DbService : IDbService
{
    private readonly ClickHouseConnectionStringBuilder? _connString;
    private string Host { get; set; }
    private ushort Port { get; set; }

    /// <summary>
    /// Constructor for the DbService class
    /// </summary>
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
            Database = configuration["ClickHouse:Database"],
        } ?? throw new Exception("ClickHouse connection string is not valid");

        try
        {
            InitializeDatabase().Wait();
        }
        catch(Exception)
        {
            throw new Exception("Database server is not accessible! Please check, if it is running.\n");
        }
    }

    /// <summary>
    /// Create a connection to the ClickHouse database
    /// </summary>
    /// <returns>A ClickHouseConnection object that represents the connection to the database</returns>
    private ClickHouseConnection CreateConnection()
    {
        if (_connString == null)
            throw new Exception("ClickHouse connection string is not valid");

        return new ClickHouseConnection(_connString.ConnectionString);
    }

    /// <summary>
    /// Check if the login credentials are valid
    /// </summary>
    /// <param name="user">The username of the user</param>
    /// <param name="password">The password of the user</param>
    /// <returns>True if the login credentials are valid, false otherwise</returns>
    public async Task<bool> CheckLogin(string user, string password)
    {
        try
        {
            var connectionString = $"Host={Host};Port={Port};User={user};Password={password};";

            using var connection = new ClickHouseConnection(connectionString);
            await connection.OpenAsync();  // Try to open a connection

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";  // Try to Execute a simple query
            await command.ExecuteScalarAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Execute a query on the ClickHouse database
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <returns>A list of strings that represent the result of the query</returns>
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

    /// <summary>
    /// Execute a query on the ClickHouse database
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <returns>A List of dictionary that represents the result of the query</returns>
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

    /// <summary>
    /// Execute a query on the ClickHouse database which returns an object
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>The result of the query</returns>
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

    /// <summary>
    /// Execute a query on the ClickHouse database
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>A List of T that represents the result of the query</returns>
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

    /// <summary>
    /// Execute a non-query on the ClickHouse database
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <returns>The number of rows affected</returns>
    public async Task<int> ExecuteNonQuery(string query)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        var command = connection.CreateCommand(query);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute a scalar query on the ClickHouse database
    /// </summary>
    /// <param name="query">The query to be executed</param>
    /// <returns>The result of the query</returns>
    public async Task<object?> ExecuteScalar(string query)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        var command = connection.CreateCommand(query);
        var result = await command.ExecuteScalarAsync();
        return result ?? DBNull.Value;
    }

    /// <summary>
    /// Initialize the ClickSphere database and create the required tables
    /// </summary>
    public async Task InitializeDatabase()
    {
        //await DeleteDatabase();

        string query = "CREATE DATABASE IF NOT EXISTS ClickSphere";
        await ExecuteNonQuery(query);

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Users (Id UUID, UserName String, LDAP_User String, FirstName String, LastName String, Email String, Phone String, Department String, Role String) ENGINE = MergeTree() PRIMARY KEY(Id)";
        await ExecuteNonQuery(query);

        // check if default user in ClickSphere.Users exists
        query = "SELECT UserName FROM ClickSphere.Users WHERE UserName = 'default'";
        object? result = await ExecuteScalar(query);

        if (result is DBNull)
        {
            // create default user
            query = "SELECT id FROM system.users WHERE name = 'default'";
            var guid = await ExecuteScalar(query);
            string? uid = guid!.ToString();

            if (!string.IsNullOrEmpty(uid))
            {
                query = $"INSERT INTO ClickSphere.Users (Id, UserName, LDAP_User, FirstName, LastName, Email, Phone, Department, Role) VALUES ('{uid}', 'default', '', 'Default', 'User', '', '', 'ClickHouse', 'default')";
                await ExecuteNonQuery(query);
            }
        }

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Views (Id String, Name String, Description String, Type String) ENGINE = MergeTree() PRIMARY KEY(Id)";
        await ExecuteNonQuery(query);

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.ViewColumns (Id UUID, Database String, ViewId String, ColumnName String, DataType String, ControlType String, Placeholder String, Sorter UInt32, Description String) ENGINE = MergeTree() PRIMARY KEY(Database, ViewId, ColumnName)";
        await ExecuteNonQuery(query);
    }

    /// <summary>
    /// Delete the ClickSphere database
    /// </summary>
    public async Task DeleteDatabase()
    {
        string query = "DROP DATABASE IF EXISTS ClickSphere";
        await ExecuteNonQuery(query);
    }
}
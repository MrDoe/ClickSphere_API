using System.Configuration;
using System.Reflection;
using ClickSphere_API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Octonica.ClickHouseClient;
using Octonica.ClickHouseClient.Exceptions;
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
    private static bool _initialized = false;

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
            // do not execute if executed before
            if (!_initialized)
                InitializeDatabase().Wait();
        }
        catch (Exception)
        {
            throw new Exception("Database server is not accessible! Please check, if it is running.\n");
        }
        finally
        {
            _initialized = true;
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
                // convert NaN to null
                if (reader.GetValue(i).ToString() == "NaN")
                    row.Add(reader.GetName(i), "NaN");
                else
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
    /// Execute bulk insert on the ClickHouse database
    /// </summary>
    /// <param name="database">The name of the database</param>
    /// <param name="tableName">The name of the table</param>
    /// <param name="columnNames">The names of the columns to insert</param>
    /// <param name="data">The data to insert</param>
    public async Task ExecuteBulkInsert(string database, string tableName, string[] columnNames,
                                        IReadOnlyList<object[]?> data)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        try
        {
            await using var writer = await connection.CreateColumnWriterAsync(
                $"INSERT INTO {database}.{tableName} ({string.Join(", ", columnNames)}) VALUES", default);

            // split data into columns
            var columnData = new List<object[]>();
            for (int i = 0; i < columnNames.Length; ++i)
            {
                var column = new object[data.Count];
                for (int j = 0; j < data.Count; ++j)
                {
                    column[j] = data[j]![i];
                }
                columnData.Add(column);
            }

            await writer.WriteTableAsync(columnData, data.Count, default);
        }
        catch (ClickHouseException e)
        {
            throw new Exception(e.Message);
        }
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

        // get version from csproj file
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        string query = "CREATE DATABASE IF NOT EXISTS ClickSphere";
        await ExecuteNonQuery(query);

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Config (Key String, Value String, Section String) ENGINE = MergeTree() PRIMARY KEY(Key, Section)";
        await ExecuteNonQuery(query);

        query = "SELECT Value FROM ClickSphere.Config WHERE Key = 'Version' and Section = 'ClickSphere'";
        object? result = await ExecuteScalar(query);

        // create role 'Admin'
        query = "CREATE ROLE IF NOT EXISTS Admin";
        await ExecuteNonQuery(query);

        if (result is DBNull)
        {
            query = $"INSERT INTO ClickSphere.Config (Key, Value, Section) VALUES ('Version', '{version}', 'ClickSphere')";
            await ExecuteNonQuery(query);
        }
        else
        {
            if (result != null && result.ToString() != version)
            {
                query = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{version}' WHERE Key = 'Version' AND Section = 'ClickSphere'";
                await ExecuteNonQuery(query);
            }
        }

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Users (Id UUID, UserName String, LDAP_User String, FirstName String, LastName String, Email String, Phone String, Department String, Role String) ENGINE = MergeTree() PRIMARY KEY(Id)";
        await ExecuteNonQuery(query);

        // check if default user in ClickSphere.Users exists
        query = "SELECT UserName FROM ClickSphere.Users WHERE UserName = 'default'";
        result = await ExecuteScalar(query);

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

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Views (Id String, Name String, Description String, Type String, Questions String) ENGINE = MergeTree() PRIMARY KEY(Id)";
        await ExecuteNonQuery(query);

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.ViewColumns (Id UUID, Database String, ViewId String, ColumnName String, DataType String, ControlType String, Placeholder String, Sorter UInt32, Description String) ENGINE = MergeTree() PRIMARY KEY(Database, ViewId, ColumnName)";
        await ExecuteNonQuery(query);

        query = "CREATE TABLE IF NOT EXISTS ClickSphere.Embeddings (Id UUID, Question String, Database String, Table String, SQL_Query String, Embedding_Question Array(Float32)) ENGINE = MergeTree() PRIMARY KEY(Id)";
        await ExecuteNonQuery(query);

        // check if AI Configuration exists
        query = "SELECT Key FROM ClickSphere.Config WHERE Section = 'RAGConfig'";
        result = await ExecuteScalar(query);

        if (result is DBNull)
        {
            await InsertAiConfig("RAGConfig");
        }

        query = "SELECT Key FROM ClickSphere.Config WHERE Section = 'Text2SQLConfig'";
        result = await ExecuteScalar(query);

        if (result is DBNull)
        {
            await InsertAiConfig("Text2SQLConfig");
        }
    }

    /// <summary>
    /// Insert the AI Configuration into the ClickSphere database
    /// </summary>
    /// <param name="type">The type of AI Configuration</param>
    private async Task InsertAiConfig(string type)
    {
        // insert AI Configuration
        string query = $"INSERT INTO ClickSphere.Config (Key, Value, Section) VALUES ('OllamaUrl', 'http://localhost:11434', '{type}')";
        await ExecuteNonQuery(query);

        string systemPrompt = "";
        string modelName = "";
        if (type == "Text2SQLConfig")
        {
            modelName = "gemma2:9b-instruct-q5_K_M";
            systemPrompt =
"""
# IDENTITY and PURPOSE

Translate natural text in English or German to ClickHouse SQL queries (Text2SQL).
Be an expert in ClickHouse SQL databases.
Be an expert in clinical and medical data and terminology in German and English.
Use ClickHouse SQL references, tutorials, and documentation to generate valid ClickHouse SQL queries.

# STEPS

- Analyze the table schema and identify the columns needed. Use ColumnDescription only for understanding the containing data.
- Use ColumnName **only** as provided in the table schema - don't modify it!
- If possible, calculate columns needed for the query (e.g. Age from Birthday).
- Analyze the question and identify the specific ClickHouse SQL instructions and functions needed.
- Use only ClickHouse SQL functions and ClickHouse SQL data types.
- Ensure the correct number of arguments and data types are used in functions.
- Generate a ClickHouse SQL query that accurately reflects the question and provides the desired output.
- Write ClickHouse function names in camelCase format (e.g., toDate, toDateTime). Start function names with a lowercase letter.
- Do not write any comments with dashes (--) in the query.
- Translate diagnoses provided as text into the respective ICD-10 codes, if needed.
- Split up diagnoses from the question into their respective words (e.g., 'lung cancer' -> '%lung%', '%cancer%').
- Append 'If' (like countIf, sumIf, avgIf, etc.) to the function name if needed.

# OUTPUT INSTRUCTIONS

- Ask the user for clarification if the question is unclear or ambiguous.
- Deny questions that require columns not present or not derivable from the table schema.
- If an ClickHouse SQL query can be generated, output the SQL query only, without comments.

# INPUT

- Table name: `[_TABLE_NAME_]`
- Table schema: `[_TABLE_SCHEMA_]`

# QUESTION

""";
        }
        else if (type == "RAGConfig")
        {
            modelName = "bge-m3";
            systemPrompt =
"""
# IDENTITY and PURPOSE

You are a pathologist and expert for medical data, diagnoses, abbreviations and pathological findings in German and English.
Your task is to find the best matching data sets for keywords or questions.

# STEPS

The following rules apply:
- Take special care not to misinterpret the data and select the data that matches the question.
- Search for medical synonyms and abbreviations of the keywords entered.
- If the user searches for tumor tissue, the user should not be shown data with normal tissue.
- If the user searches for *cancer* or *tumor*, *no* data of benign tumors or normal tissue should be displayed to the user.
- Benign tumor tissue is also referred to as non-malignant or benign tumor tissue (no indication of malignancy).
- Malignant tumor tissue (cancer) is also referred to as malignant or neoplastic tumor tissue.
- Normal tissue is also referred to as tumor-free tissue.
- If the user searches for a specific organ, no primary findings of another organ should be displayed to the user.

# QUESTION

Find the best matching records for the following question or keywords:
"[KEYWORDS]""
""";
        }
        systemPrompt = systemPrompt.Replace("\n", "\\n").Replace("'", "''");

        query = $"INSERT INTO ClickSphere.Config (Key, Value, Section) VALUES ('OllamaModel', '{modelName}', '{type}')";
        await ExecuteNonQuery(query);

        query = $"INSERT INTO ClickSphere.Config (Key, Value, Section) VALUES ('SystemPrompt', '{systemPrompt}', '{type}')";
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

    /// <summary>
    /// Get the system configuration
    /// </summary>
    /// <param name="type">The type of the configuration</param>
    /// <returns>The system configuration from the database</returns>
    public AiConfig GetAiConfig(string type)
    {
        string sql = $"SELECT Key, Value FROM ClickSphere.Config WHERE Section = '{type}' order by Key";
        var result = ExecuteQueryDictionary(sql).Result;

        AiConfig config = new();

        if (result.Count == 0)
            return config;

        foreach (var row in result)
        {
            if (row.ContainsKey("Key") && row.ContainsKey("Value"))
            {
                string? key = row["Key"]?.ToString();
                string? value = row["Value"]?.ToString();

                if (key == "OllamaUrl")
                    config.OllamaUrl = value;
                else if (key == "OllamaModel")
                    config.OllamaModel = value;
                else if (key == "SystemPrompt")
                    config.SystemPrompt = value;
            }
        }
        return config;
    }

    /// <summary>
    /// Set the system configuration
    /// </summary>
    /// <param name="config">The system configuration to set</param>
    /// <param name="type">The type of the configuration (RAG, Text2SQL)</param>
    /// <returns>True if the configuration was set successfully</returns>
    public async Task SetAiConfig(string type, AiConfig config)
    {
        // escape single quotes in input string
        config.SystemPrompt = config.SystemPrompt?.Replace("'", "''");

        try
        {
            // update ClickSphere.Config table (KEY, VALUE, SECTION)
            string sql = $"ALTER TABLE ClickSphere.Config " +
                         $"UPDATE Value = '{config.OllamaUrl}' " +
                         $"WHERE Key = 'OllamaUrl' AND Section = '{type}'";
            await ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config " +
                  $"UPDATE Value = '{config.OllamaModel}' " +
                  $"WHERE Key = 'OllamaModel' AND Section = '{type}'";
            await ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config " +
                  $"UPDATE Value = '{config.SystemPrompt}' " +
                  $"WHERE Key = 'SystemPrompt' AND Section = '{type}'";
            await ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating AiConfig: {e.Message}");
        }
    }
}
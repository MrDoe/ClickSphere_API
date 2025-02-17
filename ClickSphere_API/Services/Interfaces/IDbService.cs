using ClickSphere_API.Models;
using Octonica.ClickHouseClient;
using Octonica.ClickHouseClient.Exceptions;
namespace ClickSphere_API.Services;

/// <summary>
/// Represents a database service.
/// </summary>
public interface IDbService
{
    /// <summary>
    /// Executes a query and returns a list of strings.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of strings.</returns>
    Task<List<string>> ExecuteQuery(string query);

    /// <summary>
    /// Executes a query and returns a list of dictionaries.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of dictionaries.</returns>
    Task<List<Dictionary<string, object>>> ExecuteQueryDictionary(string query);

    /// <summary>
    /// Executes a non-query SQL statement and returns the number of affected rows.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>The number of affected rows.</returns>
    Task<int> ExecuteNonQuery(string query);

    /// <summary>
    /// Executes a query and returns a scalar value.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>The scalar value.</returns>
    Task<object?> ExecuteScalar(string query);

    /// <summary>
    /// Checks if the login credentials are valid.
    /// </summary>
    /// <param name="user">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>True if the login is valid, false otherwise.</returns>
    Task<bool> CheckLogin(string user, string password);

    /// <summary>
    /// Executes a query and returns a single object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <returns>The object of type T.</returns>
    Task<T> ExecuteQueryObject<T>(string query) where T : class, new();

    /// <summary>
    /// Executes a query and returns a list of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of the objects.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of objects of type T.</returns>
    Task<List<T>> ExecuteQueryList<T>(string query) where T : class, new();

    /// <summary>
    /// Executes a bulk insert operation.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnNames">The column names.</param>
    /// <param name="data">The data to insert.</param>
    Task ExecuteBulkInsert(string database, string tableName, string[] columnNames, IReadOnlyList<object[]?> data);

    /// <summary>
    /// Get the system configuration
    /// </summary>
    /// <param name="type">The type of the configuration</param>
    /// <returns>The system configuration from the database</returns>
    AiConfig GetAiConfig(string type);
    
    /// <summary>
    /// Set the system configuration
    /// </summary>
    /// <param name="config">The system configuration to set</param>
    /// <param name="type">The type of the configuration (RAG, Text2SQL)</param>
    /// <returns>True if the configuration was set successfully</returns>
    Task SetAiConfig(string type, AiConfig config);


    /// <summary>
    /// Executes a query and returns a list of dictionaries.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>A list of dictionaries.</returns>
    IAsyncEnumerable<Dictionary<string, object>> ExecuteQueryDictionaryAsync(string query);

    ClickHouseConnection CreateConnection();
}
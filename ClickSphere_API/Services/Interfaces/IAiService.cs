namespace ClickSphere_API.Services;
/// <summary>
/// Represents an interface for an AI service.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Asks a question to the AI service and returns the response.
    /// </summary>
    /// <param name="question">The question to ask.</param>
    /// <returns>The response from the AI service.</returns>
    Task<string> Ask(string question);

    /// <summary>
    /// Generates a query based on the question, database, and table provided.
    /// </summary>
    /// <param name="question">The question to generate the query for.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="table">The name of the table.</param>
    /// <returns>The generated query.</returns>
    Task<string> GenerateQuery(string question, string database, string table);

    /// <summary>
    /// Generates and executes a query based on the question, database, and table provided.
    /// </summary>
    /// <param name="question">The question to generate and execute the query for.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="table">The name of the table.</param>
    /// <returns>The result of executing the query.</returns>
    Task<string> GenerateAndExecuteQuery(string question, string database, string table);
}

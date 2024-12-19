using ClickSphere_API.Models;

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
    /// <param name="useEmbeddings">Whether to use embeddings for the query generation.</param>
    /// <returns>The generated query.</returns>
    Task<string> GenerateQuery(string question, string database, string table, bool useEmbeddings);

    /// <summary>
    ///  Gets the possible questions that can be asked to the AI service based on the database and table provided.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    Task<IList<string>> GetPossibleQuestions(string database, string table);

    /// <summary>
    /// Gets the column descriptions for the specified table.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="table">The name of the table.</param>
    /// <returns>The column descriptions for the specified table.</returns>
    Task<IDictionary<string, string>> GetColumnDescriptions(string database, string table);

    /// <summary>
    /// Sets the AI configuration.
    /// </summary>
    /// <param name="config">The AI configuration to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAiConfig(AiConfig config);

    /// <summary>
    /// Gets the AI configuration.
    /// </summary>
    /// <returns>The AI configuration.</returns>
    AiConfig GetAiConfig();
}

using ClickSphere_API.Models;
namespace ClickSphere_API.Services;

/// <summary>
/// Represents the interface for interacting with API views.
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Creates a new RAG table.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    Task<IResult> CreateRAGTable();

        /// <summary>
    /// Generate embedding from input text.
    /// </summary>
    /// <param name="input">The input as plain text</param>
    /// <returns>Embedding vector</returns>
    Task<List<List<float>>?> GenerateEmbedding(string input);

    /// <summary>
    /// Store embedding of SQL query in ClickHouse database.
    /// </summary>
    /// <params name="question">The user question</params>
    /// <params name="query">The SQL query</params>
    /// <params name="embedding">The embedding of the question and query</params>
    /// <returns>True if the embedding was stored successfully</returns>
    Task<bool> StoreSqlEmbedding(string question, string database, string table, string query, List<List<float>> embedding);

    /// <summary>
    /// Gets the similar queries based on the question provided.
    /// </summary>
    /// <param name="question">The question to get similar queries for.</param>
    /// <param name="database">The name of the database.</param>
    /// <param name="table">The name of the table.</param>
    /// <returns>The list of similar queries.</returns>
    Task<IList<string>> GetSimilarQueries(string question, string database, string table);

    /// <summary>
    /// Store embedding of document in RAG table
    /// </summary>
    /// <params name="filename">The file name of the source file</params>
    /// <params name="document">The document as plain text</params>
    /// <params name="embedding">The embedding of the document</params>
    /// <returns>True if the embedding was stored successfully</returns>
    Task<bool> StoreRagEmbedding(string filename, string document, List<float> embedding);

    /// <summary>
    /// Get the document contents from the RAG table by a keyword by doing RAG search.
    /// </summary>
    /// <param name="keyword">The keyword to search for in the documents</param>
    /// <returns>List of embeddings of the documents</returns>
    Task<IList<string>> GetRagDocuments(string keyword);
}
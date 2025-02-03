using ClickSphere_API.Models.Requests;
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
    /// <param name="taskType">The type of task (search_document, search_query, classification, clustering)</param>
    /// <returns>Embedding vector</returns>
    Task<List<List<float>>?> GenerateEmbedding(string input, string taskType);

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
    /// <params name="id">The id of the data set</params>
    /// <params name="filename">The file name of the source file</params>
    /// <params name="document">The document as plain text</params>
    /// <params name="database">The database where the document is stored</params>
    /// <params name="tableName">The table name where the document is stored</params>
    /// <params name="columnName">The column where the document is stored</params>
    /// <params name="embedding">The embedding vector of the document</params>
    Task StoreRagEmbedding(long id, string filename, string document, string database,
                           string tableName, string columnName, List<float> embedding);

    /// <summary>
    /// Get the document contents from the RAG table by a keyword by doing RAG search.
    /// </summary>
    /// <param name="keyword">The keyword to search for in the documents</param>
    /// <param name="distance">The distance threshold for the search (from -100 to 100)</param>
    /// <param name="database">The name of the database</param>
    /// <param name="viewName">The name of the view</param>
    /// <param name="columnName">The column to search for the keyword</param>
    /// <returns>RAGresult object containing Session Id and ResultCount</returns>
    Task<RAGresult?> GetRagDocuments(string keyword, int distance, string database, 
                                     string viewName, string columnName);

    /// <summary>
    /// Delete the embeddings from the RAG table.
    /// </summary>
    /// <param name="database">The name of the database</param>
    /// <param name="tableName">The name of the table</param>
    /// <param name="columnName">The name of the column</param>
    Task DeleteRagEmbeddings(string database, string tableName, string columnName);
}
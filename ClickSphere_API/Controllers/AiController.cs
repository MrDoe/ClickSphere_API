using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClickSphere_API.Services;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Models;
using System.Text;
using System.Linq;
namespace ClickSphere_API.Controllers;

/// <summary>
/// Interface to the Ollama AI service.
/// </summary>
[ApiController]
public class AiController(IAiService AiService, IRagService RagService, IDbService DbService) : ControllerBase
{
    /// <summary>
    /// Ask the AI a question. Call the Ollama API
    /// </summary>
    /// <param name="question" example="How can I create a new table?">The question to ask.</param>
    /// <returns>The answer to the question.</returns>
    [Authorize]
    [Route("/ask")]
    [HttpPost]
    public async Task<string> Ask(string question)
    {
        // Call the Ollama API
        string response = await AiService.Ask(question);
        return response;
    }

    /// <summary>
    /// Generate a SQL query based on a question and a table.
    /// </summary>
    /// <param name="request">The request to generate a query.</param>
    /// <returns>The answer to the question.</returns>
    [Authorize]
    [Route("/generateQuery")]
    [HttpPost]
    public async Task<string> GenerateQuery(GenerateQueryRequest request)
    {
        if (request.Question == null || request.Database == null || request.Table == null)
        {
            return "Invalid request";
        }

        if (request.UseEmbeddings)
        {
            // Get similar queries from embeddings
            var queries = await RagService.GetSimilarQueries(request.Question, request.Database, request.Table);
            if (queries.Count > 0)
                return queries[0];
        }

        // Call the Ollama API to get response
        return await AiService.GenerateQuery(request.Question, request.Database, request.Table, request.UseEmbeddings);
    }

    /// <summary>
    /// Get the possible questions that can be asked to the AI service based on the database and table provided.
    /// </summary>
    /// <param name="database" example="default">The name of the database.</param>
    /// <param name="table" example="trips">The name of the table.</param>
    /// <returns>The list of possible questions.</returns>
    [Authorize]
    [Route("/getPossibleQuestions")]
    [HttpGet]
    public async Task<IList<string>> GetPossibleQuestions(string database, string table)
    {
        // Call the Ollama API
        IList<string> response = await AiService.GetPossibleQuestions(database, table);
        return response;
    }

    /// <summary>
    /// Get column descriptions for the specified table.
    /// </summary>
    /// <param name="database" example="default">The name of the database.</param>
    /// <param name="table" example="trips">The name of the table.</param>
    /// <returns>The dictionary of column descriptions.</returns>
    [Route("/getColumnDescriptions")]
    [HttpGet]
    public async Task<IDictionary<string, string>> GetColumnDescriptions(string database, string table)
    {
        // Call the Ollama API
        return await AiService.GetColumnDescriptions(database, table);
    }

    /// <summary>
    /// Get the system configuration.
    /// </summary>
    /// <returns>The system configuration.</returns>
    [Authorize]
    [Route("/getAiConfig")]
    [HttpGet]
    public AiConfig GetAiConfig()
    {
        // Call the Ollama API
        AiConfig response = AiService.GetAiConfig();
        return response;
    }

    /// <summary>
    /// Set the system configuration.
    /// </summary>
    /// <param name="config">The system configuration.</param>
    /// <returns>The system configuration.</returns>
    [Authorize]
    [Route("/setAiConfig")]
    [HttpPost]
    public async Task SetAiConfig(AiConfig config)
    {
        // Call the Ollama API
        await AiService.SetAiConfig(config);
    }

    /// <summary>
    /// Store the embedding of a document in the RAG table.
    /// </summary>
    /// <param name="doc">The document to store the embedding.</param>
    /// <returns>True if the embedding was stored successfully.</returns>
    [Authorize]
    [Route("/storeRagEmbedding")]
    [HttpPost]
    public async Task<bool> StoreRagEmbedding(Document doc)
    {
        if (doc == null || doc.Filename == null || doc.Content == null)
        {
            return false;
        }
        // Generate embedding for the document
        string? content = Encoding.UTF8.GetString(Convert.FromBase64String(doc.Content));

        var embedding = await RagService.GenerateEmbedding(content, "File");
        if (embedding == null)
        {
            return false;
        }
        // decode content from base64
        if (string.IsNullOrEmpty(content))
            return false;
        else
            await RagService.StoreRagEmbedding(doc.Filename, content, embedding[0]);

        return true;
    }

    /// <summary>
    /// Store the embedding of view column data in the RAG table.
    /// </summary>
    /// <param name="database" example="ClickSphere">The name of the database.</param>
    /// <param name="viewName" example="V_HCC_Nexus">The name of the view.</param>
    /// <param name="dataColumn" example="osnBefundText">The column to store as embedding.</param>
    /// <param name="keyColumn" example="osnMateriealarten">The column to use as key for the embedding.</param>
    /// <returns>True if the embedding was stored successfully.</returns>
    [Route("/storeViewColumnEmbedding")]
    [HttpPost]
    public async Task<bool> StoreViewColumnEmbedding(string database, string viewName, string dataColumn, string keyColumn)
    {
        if(string.IsNullOrEmpty(viewName) || string.IsNullOrEmpty(dataColumn) || string.IsNullOrEmpty(database))
            return false;

        // Iterate over data of the view
        var dataset = await DbService.ExecuteQueryDictionary($"SELECT * FROM {database}.{viewName}");
      
        if (dataset == null || dataset.Count == 0)
            return false;
        
        // Generate embedding for each dataset of the column data
        foreach (var row in dataset)
        {
            if (!row.ContainsKey(dataColumn))
                return false;
            
            string? columnData = row[dataColumn].ToString();
            
            if(string.IsNullOrEmpty(columnData))
                return false;
            
            var embedding = await RagService.GenerateEmbedding(columnData, "search_query");
            if (embedding == null)
            {
                return false;
            }
            await RagService.StoreRagEmbedding(row[keyColumn].ToString() ?? "", columnData, embedding[0]);
        }
        return true;
    }

    /// <summary>
    /// Get the document contents from the RAG table by a keyword by doing RAG search.
    /// </summary>
    /// <param name="b64Keywords">The keyword to search for in the documents.</param>
    /// <param name="distance">The distance threshold for the search (from -100 to 100).</param>
    /// <returns>The list documents.</returns>
    //[Authorize]
    [Route("/getRagDocuments")]
    [HttpGet]
    public async Task<IList<string>> GetRagDocuments(string b64Keywords, int distance)
    {
        // decode base64 keywords
        string keywords = Encoding.UTF8.GetString(Convert.FromBase64String(b64Keywords));

        // add prefix to the keywords
        keywords = "Get all relevant documents which may be important for answering this question: " + keywords;

        IList<string> documents = await RagService.GetRagDocuments(keywords, distance);

        if (documents.Count == 0)
            return [];
        else
            return documents;
    }
}

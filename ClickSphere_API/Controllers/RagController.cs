using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClickSphere_API.Services;
using ClickSphere_API.Models.Requests;
using System.Text;
namespace ClickSphere_API.Controllers;

/// <summary>
/// Interface to the Ollama AI service for embedding generation and RAG search.
/// </summary>
[ApiController]
public class RagController(IRagService RagService, IDbService DbService) : ControllerBase
{
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

        var embedding = await RagService.GenerateEmbedding(content, "");
        if (embedding == null)
        {
            return false;
        }
        // decode content from base64
        if (string.IsNullOrEmpty(content))
            return false;
        else
            await RagService.StoreRagEmbedding(doc.Id, doc.Filename, content, "", "", "", embedding[0]);

        return true;
    }

    /// <summary>
    /// Store the embedding of view column data in the RAG table.
    /// </summary>
    /// <param name="request">The request containing the view column data.</param>
    /// <returns>True if the embedding was stored successfully.</returns>
    [Route("/storeViewColumnEmbedding")]
    [HttpPost]
    public async Task<IActionResult> StoreViewColumnEmbedding(StoreViewColumnRequest request)
    {
        if(string.IsNullOrEmpty(request.tableName) || string.IsNullOrEmpty(request.dataColumn) || 
           string.IsNullOrEmpty(request.database))
            return BadRequest("Table name, data column, and database cannot be null or empty.");

        // delete existing embeddings
        await RagService.DeleteRagEmbeddings(request.database, request.tableName, request.dataColumn);

        // Iterate over data of the view
        var dataset = await DbService.ExecuteQueryDictionary(
            $"SELECT * FROM {request.database}.{request.tableName}");
      
        if (dataset == null || dataset.Count == 0)
            return BadRequest("No data found in the dataset.");
        
        // Generate embedding for each dataset of the column data
        for(int i=0; i<dataset.Count; ++i)
        {
            var row = dataset[i];

            if (!row.ContainsKey(request.dataColumn))
                return BadRequest($"Column {request.dataColumn} not found in dataset.");

            if (!row.ContainsKey(request.keyColumn))
                return BadRequest($"Key column {request.keyColumn} not found in dataset.");
            
            string? columnData = row[request.dataColumn].ToString();
            
            if(string.IsNullOrEmpty(columnData))
                continue;
            
            var embedding = await RagService.GenerateEmbedding(columnData, "");
            if (embedding == null)
                return BadRequest($"Failed to generate embedding for row {i}.");

            // get id from key column
            long id;
            try 
            {
               id = Convert.ToInt64(row[request.keyColumn]);
            }
            catch
            {
                return BadRequest($"Failed to convert key column {request.keyColumn} to long for row {i}.");
            }
            await RagService.StoreRagEmbedding(id, "", columnData, request.database, request.tableName, 
                                               request.dataColumn, embedding[0]);
        }
        return Ok();
    }

    /// <summary>
    /// Get the document contents from the RAG table by a keyword by doing RAG search.
    /// </summary>
    /// <param name="b64Keywords">The keyword to search for in the documents.</param>
    /// <param name="distance">The distance threshold for the search (from -100 to 100).</param>
    /// <param name="database" example="ClickSphere">The name of the database.</param>
    /// <param name="viewName" example="V_HCC_Nexus">The name of the view.</param>
    /// <param name="columnName" example="osnBefundText">The column to search for the keyword.</param>
    /// <returns>The list documents.</returns>
    //[Authorize]
    [Route("/getRagDocuments")]
    [HttpGet]
    public async Task<RAGresult?> GetRagDocuments(string b64Keywords, int distance, string database,
                                                  string viewName, string columnName)
    {
        // decode base64 keywords
        string keywords = Encoding.UTF8.GetString(Convert.FromBase64String(b64Keywords));
        return await RagService.GetRagDocuments(keywords, distance, database, viewName, columnName);
    }
}
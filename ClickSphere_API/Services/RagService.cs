using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClickSphere_API.Models;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Services;

namespace ClickSphere_API.Services;

/// <summary>
/// This class is used as RAG (Retrieval Augmented Generation) service.
/// </summary>
public class RagService : IRagService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RagService"/> class.
    /// </summary>
    /// <param name="dbService">The database service.</param>
    public RagService(IDbService dbService)
    {
        DbService = dbService;
        AiConfig = GetAiConfig();
        CreateRAGTable().Wait();
    }

    private readonly IDbService? DbService;
    private readonly AiConfig? AiConfig;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Get the system configuration
    /// </summary>
    /// <returns>The system configuration from the database</returns>
    private AiConfig GetAiConfig()
    {
        string sql = "SELECT Key, Value FROM ClickSphere.Config WHERE Section = 'AiConfig' order by Key";
        var result = DbService!.ExecuteQueryDictionary(sql).Result;

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
    /// Create the ClickSphere.RAG table with embeddings.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public async Task<IResult> CreateRAGTable()
    {
        try
        {
            var query = @"
                CREATE TABLE IF NOT EXISTS ClickSphere.RAG
                (
                    Id UInt64,
                    FilePath String,
                    FileContent String,
                    Embedding Array(Float32)
                )
                ENGINE = MergeTree()
                ORDER BY Id";

            await DbService!.ExecuteNonQuery(query);

            // create index on the embedding column
            query = "CREATE INDEX IF NOT EXISTS rag_embedding_index ON ClickSphere.RAG(Embedding) TYPE minmax GRANULARITY 1";
            await DbService.ExecuteNonQuery(query);

            // Create table for RAG search results
            query = @"
                CREATE TABLE IF NOT EXISTS ClickSphere.RAGresult (
                Id Int64,
                FilePath String, 
                Distance Float, 
                SessionId UUID NOT NULL
                ) ENGINE Memory";
            await DbService.ExecuteNonQuery(query);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Generate embedding from input text.
    /// </summary>
    /// <param name="input">The input text</param>
    /// <param name="taskType">The task type (search_query, search_document, clustering, classification)</param>
    /// <returns>Embedding vector</returns>
    public async Task<List<List<float>>?> GenerateEmbedding(string input, string? taskType)
    {
        // create a new HttpClient and HttpClientHandler
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig!.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(15)
        };

        // Add an Accept header for JSON format
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            //temperature = 0.1 // keep default value
        };

        // add task type to the input (required for some models)
        if(!string.IsNullOrEmpty(taskType))
            input = taskType + ": " + input;

        var request = new OllamaEmbed
        {
            model = "snowflake-arctic-embed2",
            input = input,
            truncate = true,
            options = requestOptions,
            keep_alive = "5m"
        };

        string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);

        var jsonContent = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            mediaType);

        // Send POST request to the Ollama API
        HttpResponseMessage response;
        try 
        {
            response = await client.PostAsync("api/embed", jsonContent);
        }
        catch (Exception e)
        {
            throw new Exception($"Error calling Ollama API: {e.Message}");
        }

        if (response.IsSuccessStatusCode)
        {
            // Read the response object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse == null)
                return null;

            var jsonObject = JsonSerializer.Deserialize<OllamaEmbedResponse>(jsonResponse, jsonOptions);

            // Return the answer from the json object
            if (jsonObject!.embeddings != null)
                return jsonObject.embeddings;
            else
                return null;
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Store embedding of SQL query in ClickHouse database
    /// </summary>
    /// <params name="question">The user question</params>
    /// <params name="query">The SQL query</params>
    /// <params name="embedding">The embedding of the question and query</params>
    /// <returns>True if the embedding was stored successfully</returns>
    public async Task<bool> StoreSqlEmbedding(string question, string database, string table, string query, List<List<float>> embedding)
    {
        // convert embedding to string
        string embeddingString = string.Join(",", embedding.SelectMany(x => x).Select(x => x.ToString()));

        // escape single quotes in the strings
        question = question.Replace("'", "''");
        query = query.Replace("'", "''");

        try
        {
            // insert embedding into ClickSphere.Embeddings table
            string sql = "INSERT INTO ClickSphere.Embeddings " +
                         "(Question, Database, Table, SQL_Query, Embedding_Question) " +
                        $"VALUES ('{question}', '{database}', '{table}', '{query}', '[{embeddingString}]')";
            await DbService!.ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            throw new Exception($"Error storing embedding: {e.Message}");
        }

        return true;
    }

    /// <summary>
    /// Query the database for the most similar embeddings for a given question.
    /// Generate an embedding for the question and compare it to the embeddings in the database.
    /// </summary>
    /// <param name="question">The user question</param>
    /// <param name="database">The database to query</param>
    /// <param name="table">The table to query</param>
    /// <returns>List of SQL queries with the most similar embeddings</returns>
    public async Task<IList<string>> GetSimilarQueries(string question, string database, string table)
    {
        // generate embedding for the question
        var embedding = await GenerateEmbedding(question, "Represent this sentence for searching relevant passages");

        if (embedding == null)
            return [];

        // convert embedding to string
        string embeddingString = string.Join(",", embedding.SelectMany(x => x).Select(x => x.ToString()));

        // get the most similar embeddings from the database
        string sql = $"SELECT SQL_Query, cosineDistance(Embedding_Question, [{embeddingString}]) as Distance " +
                      "FROM ClickSphere.Embeddings " +
                     $"WHERE cosineDistance(Embedding_Question, [{embeddingString}]) < 0.2 " +
                     $"AND Database = '{database}' AND Table = '{table}' AND isNotNull(Embedding_Question) " +
                     $"ORDER BY 2 DESC";
        var result = await DbService!.ExecuteQueryDictionary(sql);

        return [.. result.Select(x => x["SQL_Query"]?.ToString() ?? "")];
    }

    /// <summary>
    /// Store embedding of document in RAG table
    /// </summary>
    /// <params name="id">The id of the data set</params>
    /// <params name="filename">The file name of the source file</params>
    /// <params name="document">The document as plain text</params>
    /// <params name="embedding">The embedding of the document</params>
    /// <returns>True if the embedding was stored successfully</returns>
    public async Task<bool> StoreRagEmbedding(long id, string filename, string document, List<float> embedding)
    {
        // convert embedding to string
        string embeddingString = string.Join(",", embedding.Select(x => x.ToString()));

        // escape single quotes in the strings
        filename = filename.Replace("'", "''");
        document = document.Replace("'", "''");

        // escape %, _, and \ in the strings
        filename = filename.Replace("%", "\\%").Replace("_", "\\_").Replace("\\", "\\\\");
        document = document.Replace("%", "\\%").Replace("_", "\\_").Replace("\\", "\\\\");

        try
        {
            // insert embedding into ClickSphere.RAG table
            string sql = "INSERT INTO ClickSphere.RAG " +
                         "(Id, FilePath, FileContent, Embedding) " +
                        $"VALUES ({id}, '{filename}', '{document}', '[{embeddingString}]')";
            await DbService!.ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            throw new Exception($"Error storing embedding: {e.Message}");
        }

        return true;
    }

    /// <summary>
    /// Get the document contents from the RAG table by a keyword by doing RAG search.
    /// </summary>
    /// <param name="keyword">The keyword to search for in the documents</param>
    /// <param name="distance">The distance threshold for the search</param>
    /// <returns>Session id for search results</returns>
    public async Task<Guid?> GetRagDocuments(string keyword, int distance)
    {
        string sDistance = ((float)distance / 100.0f).ToString("0.00");

        // generate embedding for the keyword
        // taskType "Represent this sentence for searching relevant passages"
        var embedding = await GenerateEmbedding(keyword, "");
        if (embedding == null)
            return null;

        // convert embedding to string
        string embeddingString = string.Join(",", embedding.SelectMany(x => x).Select(x => x.ToString()));

        // get the most similar embeddings from the database
        string sql = $"WITH EmbeddingCTE AS (" +
                    $"SELECT [{embeddingString}] AS EmbeddingString" +
                    $") " +
                    $"SELECT Id, FilePath, cosineDistance(Embedding, EmbeddingString) as Distance " +
                    $"FROM ClickSphere.RAG, EmbeddingCTE " +
                    $"WHERE cosineDistance(Embedding, EmbeddingString) < {sDistance} " +
                    $"ORDER BY cosineDistance(Embedding, EmbeddingString) ASC LIMIT 1000";

        var result = await DbService!.ExecuteQueryDictionary(sql);

        if(result.Count == 0)
            return null;
        
        // create session id
        var sessionId = Guid.NewGuid();

        // store output to temporary table
        string sqlQuery = "";
        foreach (var row in result)
        {
            sqlQuery += @$"
            INSERT INTO ClickSphere.RAGresult
            ( Id,
              FilePath, 
              Distance, 
              SessionId ) 
            VALUES (
             {row["Id"]},
            '{row["FilePath"]}',
             {row["Distance"]},
            '{sessionId}');";
        }
        await DbService.ExecuteNonQuery(sqlQuery);

        return sessionId;
    }
}
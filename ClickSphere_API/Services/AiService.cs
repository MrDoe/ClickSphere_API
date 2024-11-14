using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClickSphere_API.Models.Requests;
using System.Text.Json.Serialization;
using ClickSphere_API.Models;

namespace ClickSphere_API.Services;

/// <summary>
/// Ai service class to connect to Ollama
/// </summary>
public partial class AiService : IAiService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiService"/> class.
    /// </summary>
    public AiService(IDbService dbService, IApiViewService viewService)
    {
        DbService = dbService;
        ViewService = viewService;
        AiConfig = GetAiConfig();
    }

    /// <summary>
    /// Sets the AI configuration.
    /// </summary>
    public void SetAiConfig()
    {
        AiConfig = GetAiConfig();
        if (string.IsNullOrEmpty(AiConfig.OllamaUrl) || string.IsNullOrEmpty(AiConfig.OllamaModel))
        {
            throw new Exception("Error: Ollama URL or model not set in the configuration!");
        }
    }

    private IDbService? DbService { get; set; }
    private IApiViewService? ViewService { get; set; }
    private readonly string OllamaApiPath = "api/generate";
    private AiConfig AiConfig { get; set; }
    private readonly string promptAddition = " Keep it short and as simple as possible.";
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Ask a question regarding to ClickHouse databases
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public async Task<string> Ask(string question)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        string systemPrompt = "You are an expert for ClickHouse database systems. Answer questions related to ClickHouse databases only.";

        OllamaRequest request = new()
        {
            model = AiConfig.OllamaModel!,
            prompt = question,
            stream = false,
            system = systemPrompt
        };

        // Create the JSON request content.
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request, jsonOptions),
            Encoding.UTF8,
            mediaType);

        // Send a POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(OllamaApiPath, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response as json object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);

            // Return the answer from the json object
            return jsonObject?.response ?? "";
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Generate a SQL query based on a question and a table
    /// </summary>
    /// <param name="question">The question to convert into a SQL query</param>
    /// <param name="database">The database the query will run on.</param>
    /// <param name="table">The table the query will run on.</param>
    /// <returns></returns>
    public async Task<string> GenerateQuery(string question, string database, string table)
    {
        // get source table definition from database
        string? tableDefinition = await ViewService!.GetViewDefinition(database, table);

        if (tableDefinition == null)
            return "ERROR: Invalid table name!";

        // add table definition to the system prompt
        string systemPrompt = AiConfig.SystemPrompt!;
        systemPrompt = systemPrompt.Replace("[_TABLE_NAME_]", $"{database}.{table}");
        systemPrompt = systemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };

        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(120)
        };

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            temperature = 0.0
        };

        var request = new OllamaRequest
        {
            model = AiConfig.OllamaModel!,
            system = systemPrompt,
            prompt = question + promptAddition,
            stream = false
        };

        string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);

        var jsonContent = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            mediaType);

        // Send POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(OllamaApiPath, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response as json object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse, jsonOptions);

            if (jsonObject == null)
                return "ERROR: No response from Ollama API!";

            // Return the answer from the json object
            if (jsonObject.response != null)
            {
                string responseText = jsonObject.response.Trim();

                // remove ```sql from the beginning of the response and ``` from the end
                int sqlIndex = responseText.IndexOf("```sql");
                int endSqlIndex = responseText.LastIndexOf("```");
                if (sqlIndex != -1 && endSqlIndex != -1)
                {
                    responseText = responseText[(sqlIndex + 5)..endSqlIndex];
                }

                // get the query after the first SELECT until the first ';' character
                int selectIndex = responseText.IndexOf("SELECT");
                int semicolonIndex = responseText.IndexOf(';');
                if (semicolonIndex == -1)
                {
                    // add semicolon to the end of the response
                    responseText += ";";
                    semicolonIndex = responseText.IndexOf(';');
                }

                if (selectIndex != -1)
                {
                    responseText = responseText[selectIndex..(semicolonIndex + 1)];
                }

                // generate embedding for question and query
                var embedding = await GenerateEmbedding(question, responseText);
                if (embedding != null)
                {
                    // store embedding in ClickHouse database
                    await StoreEmbedding(question, database, table, responseText, embedding);
                }

                return responseText;
            }
            return jsonObject.response ?? "Unsuccessful query generation!";
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Get first 10 rows of the table and ask AI to generate a list of possible questions
    /// </summary>
    /// <param name="database">The database to get the table from</param>
    /// <param name="table">The table to get the rows from</param>
    /// <returns>List of possible questions</returns>
    public async Task<IList<string>> GetPossibleQuestions(string database, string table)
    {
        // get 20 random rows from the table
        var rows = await DbService!.ExecuteQueryDictionary($"SELECT * FROM {database}.{table} ORDER BY rand() LIMIT 10");

        // get source table definition from database
        string? tableDefinition = await ViewService!.GetViewDefinition(database, table);

        if (tableDefinition == null)
            return [];

        // add table definition to the system prompt
        string systemPrompt = """
# IDENTITY and PURPOSE

You are an expert for data analysis of medical and biosample data.
Your task is to create a list of related questions for the EXAMPLE DATASET given by the user.
Take TABLE SCHEMA as a reference for the columns of the EXAMPLE DATASET.
Precisely follow the instructions given to you.

# TABLE SCHEMA

[_TABLE_SCHEMA_]

""";

        systemPrompt = systemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        // create a prompt with the table rows
        StringBuilder prompt = new();

        prompt.AppendLine("# EXAMPLE DATASET");
        prompt.AppendLine();

        // generate column header
        var columnHeaders = rows.First().Keys;
        foreach (var column in columnHeaders)
        {
            prompt.Append($"\"{column}\"");
            if (column != columnHeaders.Last())
                prompt.Append(',');
        }

        prompt.AppendLine();

        foreach (var row in rows)
        {
            if (row == rows.First())
                continue;

            foreach (var column in row)
            {
                prompt.Append($"\"{column.Value}\",");
            }
            prompt.AppendLine();
        }


        prompt.AppendLine("# YOUR TASK:");
        prompt.AppendLine();
        prompt.AppendLine("Generate a list of 10 related questions for analyzing the EXAMPLE DATASET.\n" +
                          "Only ask questions about existing columns of the EXAMPLE DATASET.\n" +
                          "Don't ask questions for which to answer columns are needed that are not present in the EXAMPLE DATASET.\n" +
                          "Output the questions only, separated by newline characters. No explanations. No numberings. No bullet lists.");

        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            temperature = 0.1
        };

        var request = new OllamaRequest
        {
            model = "gemma2:9b-instruct-q5_K_M",
            system = systemPrompt,
            prompt = prompt.ToString(),
            stream = false
        };

        string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);

        var jsonContent = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            mediaType);

        // Send POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(OllamaApiPath, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse == null)
                return ["ERROR: No response from Ollama API!"];

            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse, jsonOptions);

            // Return the answer from the json object
            if (jsonObject!.response != null)
            {
                string responseText = jsonObject.response.Trim();

                // get view object
                var view = await ViewService!.GetViewConfig(database, table);
                view.Questions = responseText;

                // write questions to config table
                await ViewService!.UpdateView(view);

                // split the response into lines
                return responseText.Split('\n').Select(x => x.Trim()).ToList();
            }
            return ["Unsuccessful generation of questions!"];
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Analyze the table and generate a list of column descriptions column by column
    /// </summary>
    /// <param name="table">The table to get the rows from</param>
    /// <param name="column">The column to get the description for</param>
    /// <param name="datatype">The datatype of the column</param>
    /// <returns>Dictionary of column descriptions</returns>
    private async Task<string> GetColumnDescription(string table, string column, string datatype)
    {
        // get first 100 rows of the table
        //var rows = await DbService!.ExecuteQueryDictionary($"SELECT distinct(`{column}`) FROM {database}.{table} LIMIT 10");

        // add table definition to the system prompt
        string systemPrompt =
"""
# Instructions:

- You are an expert for biosample and medical data.
- Be precise and concise and follow the instructions given to you.
- Output the column description only, no bullet points or numbered lists.
""";

        string prompt = """
# Instructions:

Generate a description for a database column based on the following information:
- Table Name: [TableName]
- Column Name (DataType): [ColumnName] ([DataType])
Output a short description only, no explanations. 

# Output:

""";

        // insert table and column name into the prompt
        prompt = prompt.Replace("[TableName]", table);
        prompt = prompt.Replace("[ColumnName]", column);
        prompt = prompt.Replace("[DataType]", datatype);

        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };

        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(120)
        };

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            temperature = 0.1
        };

        var request = new OllamaRequest
        {
            model = "gemma2:9b-instruct-q5_K_M",
            system = systemPrompt,
            prompt = prompt,
            stream = false
        };

        string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);

        var jsonContent = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            mediaType);

        // Send POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(OllamaApiPath, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if (jsonResponse == null)
                return "";

            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse, jsonOptions);

            // Return the answer from the json object
            if (jsonObject!.response != null)
            {
                return jsonObject.response.Trim();
            }
            return "";
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

/// <summary>
/// Generate column descriptions for the specified table.
/// </summary>
/// <param name="database" example="default">The name of the database.</param>
/// <param name="table" example="trips">The name of the table.</param>
/// <returns>The dictionary of column descriptions.</returns>
public async Task<IDictionary<string, string>> GetColumnDescriptions(string database, string table)
{
    // get column names first
    var columns = await ViewService!.GetViewColumns(database, table, false);

    // iterate over columns and get descriptions
    var columnDescriptions = new Dictionary<string, string>();
    foreach (var column in columns)
    {
        var description = await GetColumnDescription(table, column.ColumnName!, column.DataType!);
        columnDescriptions.Add(column.ColumnName!, description);
    }

    return columnDescriptions;
}

    /// <summary>
    /// Get the system configuration
    /// </summary>
    /// <returns>The system configuration from the database</returns>
    public AiConfig GetAiConfig()
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
    /// Set the system configuration
    /// </summary>
    /// <param name="config">The system configuration to set</param>
    /// <returns>True if the configuration was set successfully</returns>
    public async Task SetAiConfig(AiConfig config)
    {
        // escape single quotes in the strings
        config.SystemPrompt = config.SystemPrompt?.Replace("'", "''");

        try
        {
            // update ClickSphere.Config table (KEY, VALUE, SECTION)
            string sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.OllamaUrl}' WHERE Key = 'OllamaUrl' AND Section = 'AiConfig'";
            await DbService!.ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.OllamaModel}' WHERE Key = 'OllamaModel' AND Section = 'AiConfig'";
            await DbService!.ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.SystemPrompt}' WHERE Key = 'SystemPrompt' AND Section = 'AiConfig'";
            await DbService!.ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating AiConfig: {e.Message}");
        }
    }

    /// <summary>
    /// Generate embedding from user question and SQL query
    /// </summary>
    /// <param name="question">The user question</param>
    /// <param name="query">The SQL query</param>
    /// <returns>Embedding of the question and query</returns>
    public async Task<List<List<float>>?> GenerateEmbedding(string question, string query)
    {
        // create a new HttpClient and HttpClientHandler
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(AiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            temperature = 0.0
        };

        var request = new OllamaEmbed
        {
            model = "nomic-embed-text",
            input = $"QUESTION: {question}\nSQL_QUERY: {query}",
            truncate = true,
            options = requestOptions,
            keep_alive = "30m"
        };

        string jsonRequest = JsonSerializer.Serialize(request, jsonOptions);

        var jsonContent = new StringContent(
            jsonRequest,
            Encoding.UTF8,
            mediaType);

        // Send POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync("api/embed", jsonContent);

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
    /// Store embedding in ClickHouse database
    /// </summary>
    /// <params name="question">The user question</params>
    /// <params name="query">The SQL query</params>
    /// <params name="embedding">The embedding of the question and query</params>
    /// <returns>True if the embedding was stored successfully</returns>
    public async Task<bool> StoreEmbedding(string question, string database, string table, string query, List<List<float>> embedding)
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
        var embedding = await GenerateEmbedding(question, "");

        if (embedding == null)
            return [];

        // convert embedding to string
        string embeddingString = string.Join(",", embedding.SelectMany(x => x).Select(x => x.ToString()));

        // get the most similar embeddings from the database
        string sql = $"SELECT SQL_Query, cosineDistance(Embedding_Question, [{embeddingString}]) as Distance " +
                      "FROM ClickSphere.Embeddings " +
                     $"WHERE cosineDistance(Embedding_Question, [{embeddingString}]) > 0.5 " +
                     $"AND Database = '{database}' AND Table = '{table}' AND isNotNull(Embedding_Question) " +
                     $"ORDER BY 2 DESC";
        var result = await DbService!.ExecuteQueryDictionary(sql);

        return result.Select(x => x["SQL_Query"]?.ToString() ?? "").ToList();
    }
}
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClickSphere_API.Models.Requests;
using System.Text.Json.Serialization;
namespace ClickSphere_API.Services;

/// <summary>
/// Ai service class to connect to Ollama
/// </summary>
public partial class AiService(IDbService dbService) : IAiService
{
    private readonly IDbService DbService = dbService;
    private readonly string OllamaUrl = "http://localhost:11434";
    private readonly string OllamaApiPath = "/api/generate";
    private readonly string SystemPrompt = """
# IDENTITY and PURPOSE

You are an expert for ClickHouse SQL databases and translate user questions into valid ClickHouse SQL queries.
You are a master of ClickHouse SQL analyzing each prompt to identify the specific instructions and functions needed.
You utilize this knowledge to generate an output that precisely matches the user's question.
You are adept at understanding and following formatting instructions, ensuring that your responses are always accurate and perfectly aligned with the intended outcome.
Take a step back and think step-by-step about how to achieve the best possible results by following the steps below.

# STEPS

- Analyze the given table schema below and identify the columns needed for the query. 
- Analyze the question below and identify the specific ClickHouse SQL instructions and functions needed to generate a valid ClickHouse SQL query.
- Use the ClickHouse SQL Reference (https://clickhouse.com/docs/en/sql-reference) as reference for validating ClickHouse SQL functions and data types.
- Generate a ClickHouse SQL query that accurately reflects the question and provides the desired output.
- Ensure that all functions and data types used in the query are valid ClickHouse SQL functions and data types.
- Ensure that you are using the correct number of arguments and data types when using functions in the query.
- Write ClickHouse function names in camelCase format.

# OUTPUT INSTRUCTIONS

- Double-check the query to ensure that it is valid ClickHouse SQL and provides the desired output.
- Don't explain. Output the SQL query text only.

# INPUT
- The SQL table schema is provided below:
```
[_TABLE_SCHEMA_]
```

# QUESTION
""";

    private readonly string PromptAddition = " Don't explain. Output the SQL query only.";
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
        using HttpClient client = new()
        {
            BaseAddress = new Uri(OllamaUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        string systemPrompt = "You are an expert for ClickHouse database systems. Answer questions related to ClickHouse databases only.";
        
        OllamaRequest request = new()
        {
            model = "codegemma",
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

    private string SanitizeTableDefinition(string tableDefinition)
    {
        string sanitizedTableDefinition;
        // remove unnecessary characters
        sanitizedTableDefinition = tableDefinition.Replace("`", "");
        sanitizedTableDefinition = sanitizedTableDefinition.Replace("\n", " ");
        sanitizedTableDefinition = sanitizedTableDefinition.Replace("\t", "");
        sanitizedTableDefinition = sanitizedTableDefinition.Replace("  ", " ");
        sanitizedTableDefinition = sanitizedTableDefinition.Replace("  ", " ");
        sanitizedTableDefinition = sanitizedTableDefinition.Replace("  ", " ");

        // cut off everything after SETTINGS
        int settingsIndex = sanitizedTableDefinition.IndexOf("SETTINGS");
        if (settingsIndex != -1)
            sanitizedTableDefinition = sanitizedTableDefinition[..settingsIndex];
        
        return sanitizedTableDefinition;
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
        string? tableDefinition = (await DbService.ExecuteScalar($"SHOW CREATE TABLE `{database}`.`{table}`"))?.ToString();

        if (tableDefinition == null)
            return "ERROR: Invalid table name!";

        // remove newlines and unnecessary whitespaces from the table definition
        tableDefinition = SanitizeTableDefinition(tableDefinition.ToString());

        // add table definition to the system prompt
        string systemPrompt = SystemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        using HttpClient client = new()
        {
            BaseAddress = new Uri(OllamaUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the JSON string for the request
        var requestOptions = new OllamaRequestOptions
        {
            temperature = 0.1,
            //num_thread = 14
        };

        var request = new OllamaRequest
        {
            model = "codegemma",
            system = systemPrompt,
            prompt = question,
            stream = false,
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
                // get the query after the first SELECT until the first ';' character
                int selectIndex = jsonObject.response.IndexOf("SELECT");
                int semicolonIndex = jsonObject.response.IndexOf(';');
                if (selectIndex != -1 && semicolonIndex != -1)
                {
                    return jsonObject.response[selectIndex..(semicolonIndex + 1)];
                }
            }
            return jsonObject.response ?? "Unsuccessful query generation!";
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Generate a SQL query and execute it on the database
    /// </summary>
    /// <param name="question">The question to convert into a SQL query</param>
    /// <param name="database">The database the query will run on.</param>
    /// <param name="table">The table the query will run on.</param>
    /// <returns>Result of the query.</returns>
    public async Task<string> GenerateAndExecuteQuery(string question, string database, string table)
    {
        // generate the query
        string query = await GenerateQuery(question, database, table);

        try
        {
            // execute the query
            var result = await DbService.ExecuteQueryDictionary(query);

            // return the result as json string
            return JsonSerializer.Serialize(result);
        }
        catch (Exception e)
        {
            return e.Message + "\n\nQuery: " + query;
        }
    }
}
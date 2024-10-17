using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClickSphere_API.Models.Requests;
using System.Text.Json.Serialization;
using ClickSphere_API.Models;
//using Microsoft.AspNetCore.Mvc.ViewEngines;
//using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ClickSphere_API.Services;

/// <summary>
/// Ai service class to connect to Ollama
/// </summary>
public partial class AiService(IDbService dbService, IApiViewService viewService) : IAiService
{
    private readonly IDbService DbService = dbService;
    private readonly IApiViewService? ViewService = viewService;
    private readonly string OllamaUrl = "http://localhost:11434";
    private readonly string OllamaApiPath = "api/generate";
    private readonly string SystemPrompt = """
# IDENTITY and PURPOSE

Translate natural text in English or German to ClickHouse SQL queries (Text2SQL).
Be an expert in ClickHouse SQL databases.
Be an expert in clinical and medical data and terminology in German and English.
Use ClickHouse SQL references, tutorials, and documentation to generate valid ClickHouse SQL queries.

# STEPS

- Analyze the given table schema and identify the necessary columns. Use column descriptions to understand the expected data.
- Use only the columns provided in the table schema to generate the query.
- Analyze the question and identify the specific ClickHouse SQL instructions and functions needed.
- Use the ClickHouse SQL Reference and tutorials to validate all possible ClickHouse SQL functions and data types.
- Generate a ClickHouse SQL query that accurately reflects the question and provides the desired output.
- Ensure all functions and data types used in the query are valid ClickHouse SQL functions and data types.
- Ensure the correct number of arguments and data types are used in functions.
- Ensure all column names in the query match exactly as written in the table schema.
- Write ClickHouse function names in camelCase format (e.g., toDate, toDateTime). Start function names with a lowercase letter.
- Do not write any comments with dashes (--) in the query.
- Translate diagnoses provided as text into the respective ICD-10 codes, if needed.
- Split up diagnoses from the question into their respective words (e.g., 'lung cancer' -> '%lung%', '%cancer%').
- Append 'If' (like countIf, sumIf, avgIf, etc.) to the function name if needed.

# OUTPUT INSTRUCTIONS

- Ask the user for clarification if the question is unclear or ambiguous.
- Deny questions that require columns not present or not calculatable from the table schema.
- If an ClickHouse SQL query can be generated, output the SQL query only without comments.

# INPUT
- Table name: `[_TABLE_NAME_]`
- Table schema: `[_TABLE_SCHEMA_]`

# QUESTION
""";
    private readonly string promptAddition = " Keep it short and simple.";

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
            model = "sqlcoder:15b",
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
        string systemPrompt = SystemPrompt.Replace("[_TABLE_NAME_]", $"{database}.{table}");
        systemPrompt = systemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        using HttpClient client = new()
        {
            BaseAddress = new Uri(OllamaUrl),
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
            model = "sqlcoder:15b",
            system = systemPrompt,
            prompt = question + promptAddition,
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
                if(semicolonIndex == -1)
                {
                    semicolonIndex = responseText.Length - 1;
                }

                if (selectIndex != -1)
                {
                    responseText = responseText[selectIndex..(semicolonIndex + 1)];
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

    /// <summary>
    /// Get first 10 rows of the table and ask AI to generate a list of possible questions
    /// </summary>
    /// <param name="database">The database to get the table from</param>
    /// <param name="table">The table to get the rows from</param>
    /// <returns>List of possible questions</returns>
    public async Task<IList<string>> GetPossibleQuestions(string database, string table)
    {
        // get first 100 rows of the table
        var rows = await DbService.ExecuteQueryDictionary($"SELECT * FROM {database}.{table} LIMIT 10");

        // get source table definition from database
        string? tableDefinition = await ViewService!.GetViewDefinition(database, table);

        if (tableDefinition == null)
            return [];

        // add table definition to the system prompt
        string systemPrompt = """
# IDENTITY and PURPOSE
You are an expert for data analysis and medicial data.
You are able to generate a list of related questions for analyzing a given dataset.
Take a look at the EXAMPLE DATASET given below.
Precisely follow the instructions given to you.

# TABLE SCHEMA
[_TABLE_SCHEMA_]

# EXAMPLE DATASET
""";
        
        systemPrompt = systemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        // create a prompt with the table rows
        StringBuilder prompt = new();

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
            if(row == rows.First())
                continue;
            
            foreach (var column in row)
            {
                prompt.Append($"\"{column.Value}\",");
            }
            prompt.AppendLine();
        }
        prompt.AppendLine("# TASKS");
        prompt.AppendLine("Generate a list of 5 to 10 related questions for analyzing the EXAMPLE DATASET." + 
                          "Only ask questions about columns of the EXAMPLE DATASET. No explanations. No numberings. No bullet lists. " + 
                          "Do not ask questions where columns are needed which are not present in the EXAMPLE DATASET. " +
                          "Output the questions only, separated by newline characters.");

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
            temperature = 0.0
        };

        var request = new OllamaRequest
        {
            model = "sqlcoder:15b",
            system = systemPrompt,
            prompt = prompt.ToString(),
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
            // Read the response object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if(jsonResponse == null)
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
    /// Analyze the table and generate a list of column descriptions
    /// </summary>
    /// <param name="database">The database to get the table from</param>
    /// <param name="table">The table to get the rows from</param>
    /// <returns>Dictionary of column descriptions</returns>
    public async Task<IDictionary<string, string>> GetColumnDescriptions(string database, string table)
    {
        // get first 100 rows of the table
        var rows = await DbService.ExecuteQueryDictionary($"SELECT * FROM {database}.{table} LIMIT 10");

        // add table definition to the system prompt
        string systemPrompt = """
# IDENTITY and PURPOSE

You are an expert for data analysis and medicial data.
You are able to generate a list of descriptions for the columns of a given dataset.
Take a look at the EXAMPLE DATASET given below.
Be concise and precise.

# EXAMPLE DATASET

""";
        // create a prompt with the table rows
        StringBuilder prompt = new();

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
            if(row == rows.First())
                continue;
            
            foreach (var column in row)
            {
                prompt.Append($"\"{column.Value}\",");
            }
            prompt.AppendLine();
        }
        prompt.AppendLine("\n# TASKS\n\n" + 
                          "For every column in the EXAMPLE DATASET find an appropriate description.\n" + 
                          "Output in the format: '[COLUMN1_NAME],[DESCRIPTION_1],\n[COLUMN2_NAME],[DESCRIPTION_2],\n...");

        using HttpClient client = new()
        {
            BaseAddress = new Uri(OllamaUrl),
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
            model = "sqlcoder:15b",
            system = systemPrompt,
            prompt = prompt.ToString(),
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
            // Read the response object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if(jsonResponse == null)
                return new Dictionary<string, string>() { { "ERROR", "No response from Ollama API!" } };
            
            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse, jsonOptions);

            // Return the answer from the json object
            if (jsonObject!.response != null)
            {
                string responseText = jsonObject.response.Trim();

                // build dictionary from response ("column1,description1\n...")
                var columnDescriptions = new Dictionary<string, string>();
                var lines = responseText.Split('\n').Select(x => x.Trim()).ToList();
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        columnDescriptions.Add(parts[0].Trim(), parts[1].Trim());
                    }
                }

                return columnDescriptions;
            }
            return new Dictionary<string, string>() { { "ERROR", "Unsuccessful generation of column descriptions!" } };
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Get the system configuration
    /// </summary>
    /// <returns>The system configuration from the database</returns>
    public AiConfig GetAiConfig()
    {
        string sql = "SELECT Key, Value FROM ClickSphere.Config WHERE Section = 'AiConfig' order by Key";
        var result = DbService.ExecuteQueryDictionary(sql).Result;

        AiConfig config = new();

        if (result.Count == 0)
            return config;
        
        foreach(var row in result)
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
            string sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.OllamaUrl}' WHERE Key = 'OllamaUrl' AND Section = 'AiService'";
            await DbService.ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.OllamaModel}' WHERE Key = 'OllamaModel' AND Section = 'AiService'";
            await DbService.ExecuteNonQuery(sql);

            sql = $"ALTER TABLE ClickSphere.Config UPDATE Value = '{config.SystemPrompt}' WHERE Key = 'SystemPrompt' AND Section = 'AiService'";
            await DbService.ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating AiConfig: {e.Message}");
        }
    }
}
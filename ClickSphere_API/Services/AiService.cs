using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ClickSphere_API.Models;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Tools;
namespace ClickSphere_API.Services;

/// <summary>
/// Ai service class to connect to Ollama
/// </summary>
public partial class AiService : IAiService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AiService"/> class.
    /// </summary>
    public AiService(IDbService dbService, IApiViewService viewService, IRagService ragService)
    {
        RagService = ragService;
        DbService = dbService;
        ViewService = viewService;
        Text2SQLConfig = DbService.GetAiConfig("Text2SQLConfig");
    }

    private IDbService DbService { get; set; } = default!;
    private IApiViewService ViewService { get; set; } = default!;
    private AiConfig Text2SQLConfig { get; set; } = default!;
    private IRagService RagService { get; set; } = default!;
    private readonly string OllamaApiPath = "api/generate";
    private readonly string promptAddition = " Keep it short and as simple as possible.";
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(120)
        };

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Clear();
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        string systemPrompt =
"""
Follow the instructions precisely and concisely.
Do not output additional information, just the answer to the question as plain text.
""";

        OllamaRequestOptions options = new()
        {
            temperature = 0.1,
            num_ctx = 512
        };

        OllamaRequest request = new()
        {
            model = Text2SQLConfig.OllamaModel!,
            prompt = question,
            stream = false,
            system = systemPrompt,
            options = options,
            keep_alive = "-1m"
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
    /// Translate a text into English.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <returns>The translated text.</returns>
    public async Task<string> Translate(string text)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
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
            system = @"You are an expert for translations of medical terms and diagnoses from German to English.
                       Be sure to use the right terminology and be precise.
                       Translate the user's text into English.
                       Output the translated text only, no explanations.",
            prompt = text,
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
    /// <param name="useEmbeddings">Whether to use embeddings for the question and query</param>
    /// <returns></returns>
    public async Task<string> GenerateQuery(string question, string database, string table, bool useEmbeddings)
    {
        // get source table definition from database
        string? tableDefinition = await ViewService!.GetViewDefinition(database, table);

        if (tableDefinition == null)
            return "ERROR: Invalid table name!";

        // add table definition to the system prompt
        string systemPrompt = Text2SQLConfig.SystemPrompt!;
        systemPrompt = systemPrompt.Replace("[_TABLE_NAME_]", $"{database}.{table}");
        systemPrompt = systemPrompt.Replace("[_TABLE_SCHEMA_]", tableDefinition);

        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };

        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
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
            model = Text2SQLConfig.OllamaModel!,
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
                    responseText = responseText[(sqlIndex + 6)..endSqlIndex];
                }
                
                responseText = responseText.Replace("```", "");
                responseText = responseText.Replace("\\n", " ");

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

                // remove semicolon
                responseText = responseText.TrimEnd(';');

                // replace all newlines with spaces
                responseText = responseText.Replace("\n", " ");
                responseText = responseText.Replace("\r", "");
                responseText = responseText.Replace("\t", " ");
                
                // replace multiple spaces with a single space
                responseText = Regex.Replace(responseText, @"\s+", " ");

                if (useEmbeddings)
                {
                    // generate embedding for question and query
                    var embedding = await RagService.GenerateEmbedding($"{question}\n\nSQL_QUERY: {responseText}", "search_document");
                    if (embedding != null)
                    {
                        // store embedding in ClickHouse database
                        await RagService.StoreSqlEmbedding(question, database, table, responseText, embedding);
                    }
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
    /// Pull a model from the Ollama API.
    /// </summary>
    /// <param name="model">The model to pull.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<Result> PullModelAsync(string model)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(15)
        };

        HttpResponseMessage? response;
        try
        {
            var requestBody = new
            {
                name = model,
                insecure = false,
                stream = true
            };

            response = await client.PostAsync(
                "api/pull",
                new StringContent(JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"),
                CancellationToken.None).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var streamContent = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(streamContent);

                string? line;
                DateTime lastNotificationTime = DateTime.MinValue;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var statusUpdate = JsonSerializer.Deserialize<Dictionary<string, object>?>(line);
                        if (statusUpdate == null)
                            continue;

                        string statusMessage = statusUpdate["status"]?.ToString() ?? "Model pull in progress";

                        if (statusUpdate.TryGetValue("completed", out object? completed) &&
                           statusUpdate.TryGetValue("total", out object? total))
                        {
                            statusMessage += $" ({completed}/{total})";
                        }

                        // Check if 5 seconds have passed since the last notification
                        if ((DateTime.Now - lastNotificationTime).TotalSeconds >= 5)
                        {
                            Console.WriteLine(statusMessage);
                            lastNotificationTime = DateTime.Now;
                        }

                        if (statusUpdate!.ContainsKey("completed") == true)
                        {
                            if (statusUpdate["completed"]?.ToString() == statusUpdate["total"]?.ToString())
                            {
                                break;
                            }
                        }

                        // handle errors
                        if (statusUpdate!.ContainsKey("error"))
                        {
                            return new Result(false, "Model pull failed. Error: " + statusUpdate["error"]);
                        }
                    }
                }
                return new Result(true, "Model pull completed.");
            }
            else
            {
                return new Result(false, "Model pull failed with status: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            return new Result(false, "Model pull failed. Error: " + ex.Message);
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
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
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
- Keep the column description as short as possible.
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
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
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
        try
        {
            var response = await client.PostAsync(OllamaApiPath, jsonContent);
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return e.Message;
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
    /// Get models from the Ollama API.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The models.</returns>
    public IList<string> GetModels(CancellationToken token = default)
    {
        return GetModelsAsync(token).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get models from the Ollama API.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The models.</returns>
    public async Task<IList<string>> GetModelsAsync(CancellationToken token = default)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(15)
        };

        List<string> models = [];
        HttpResponseMessage? response = null;
        try
        {
            response = await client.GetAsync(
                "api/tags",
                token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        if (response != null && response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync(token);
            using var reader = new StreamReader(stream);
            string? output = await reader.ReadToEndAsync(token);

            if (output != null)
            {
                // only collect model names
                var deserializedModels = JsonSerializer.Deserialize<OllamaModelRequest>(output)?.models;

                if (deserializedModels != null)
                {
                    foreach (var model in deserializedModels)
                    {
                        models.Add(model!.name!);
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"Request failed with status: {response?.StatusCode}.");
        }

        return models;
    }

    /// <summary>
    /// Pulls a new model from the Ollama API.
    /// </summary>
    /// <param name="modelName">The name of the model to pull.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The models.</returns>
    public async Task<Result> PullModelAsync(string modelName, CancellationToken token = default)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(15)
        };

        HttpResponseMessage? response;
        try
        {
            var requestBody = new
            {
                name = modelName,
                insecure = false,
                stream = true
            };

            response = await client.PostAsync(
                "api/pull",
                new StringContent(JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"),
                token).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                using var streamContent = await response.Content.ReadAsStreamAsync(token);
                using var reader = new StreamReader(streamContent);

                string? line;
                DateTime lastNotificationTime = DateTime.MinValue;
                while ((line = await reader.ReadLineAsync(token)) != null && !token.IsCancellationRequested)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var statusUpdate = JsonSerializer.Deserialize<Dictionary<string, object>?>(line);
                        if (statusUpdate == null)
                            continue;

                        string statusMessage = statusUpdate["status"]?.ToString() ?? "Model pull in progress";

                        if (statusUpdate.TryGetValue("completed", out object? completed) &&
                           statusUpdate.TryGetValue("total", out object? total))
                        {
                            statusMessage += $" ({completed}/{total})";
                        }

                        // Check if 5 seconds have passed since the last notification
                        if ((DateTime.Now - lastNotificationTime).TotalSeconds >= 5)
                        {
                            Console.WriteLine(statusMessage);
                            lastNotificationTime = DateTime.Now;
                        }

                        if (statusUpdate!.ContainsKey("completed") == true)
                        {
                            if (statusUpdate["completed"]?.ToString() == statusUpdate["total"]?.ToString())
                            {
                                break;
                            }
                        }

                        // handle errors
                        if (statusUpdate!.ContainsKey("error"))
                        {
                            return new Result(false, "Model pull failed. Error: " + statusUpdate["error"]);
                        }
                    }
                }
                return new Result(true, "Model pull completed.");
            }
            else
            {
                return new Result(false, "Model pull failed with status: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            return new Result(false, "Model pull failed. Error: " + ex.Message);
        }
    }

    /// <summary>
    /// Deletes a model from Ollama.
    /// </summary>
    /// <param name="modelName">The name of the model to delete.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<Result> DeleteModelAsync(string modelName)
    {
        // create a new HttpClient and HttpClientHandler
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };
        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(Text2SQLConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(15)
        };

        // Send a POST request to the Ollama API
        HttpResponseMessage? response;
        try
        {
            var requestBody = new
            {
                name = modelName
            };
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            response = await client.SendAsync(request, CancellationToken.None).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return new Result(true, "Model deleted successfully.");
            }
            else
            {
                return new Result(false, "Model deletion failed with status: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            return new Result(false, "Model deletion failed. Error: " + ex.Message);
        }
    }
}
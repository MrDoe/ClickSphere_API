using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClickSphere_API.Models.Requests;
namespace ClickSphere_API.Services;

/// <summary>
/// Ai service class to connect to Ollama
/// </summary>
public partial class AiService(IDbService dbService) : IAiService
{
    private readonly IDbService DbService = dbService;
    private readonly string Model = "duckdb-nsql";
    private string SystemPrompt = "Here is the schema of the ClickHouse database table the SQL query will run on: ";
    private readonly string OllamaUrl = "http://localhost:11434";
    private readonly string OllamaApiPath = "/api/generate";


    /// <summary>
    /// Ask a question regarding to ClickHouse databases
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public async Task<string> Ask(string question)
    {
        using HttpClient client = new();
        client.BaseAddress = new Uri(OllamaUrl);
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // prompt for the AI
        SystemPrompt = "Only give short advices on questions related to ClickHouse database systems. Here is my question: ";

        // Create the JSON request content.
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new OllamaRequest{ model = "codegemma", prompt = question, stream = false, system = SystemPrompt }),
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
    /// <param name="table">The table the query will run on.</param>
    /// <returns></returns>
    public async Task<string> GenerateQuery(string question, string table)
    {
        // get source table definition from database
        string? tableDefinition = (await DbService.ExecuteScalar("SHOW CREATE TABLE " + table))?.ToString();

        if(tableDefinition == null)
            return "ERROR: Invalid table name!";
        
        // remove newlines and unnecessary whitespaces from the table definition
        tableDefinition = tableDefinition.ToString().Replace("\n", " ").Replace("\t", " ")
                                                    .Replace("    ", " ").Replace("   ", " ")
                                                    .Replace("  ", " ").Replace(" = ", "=")
                                                    .Replace(", ", ",").Replace(" ( ", "(");

        using HttpClient client = new();
        client.BaseAddress = new Uri(OllamaUrl);
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");

        // Add an Accept header for JSON format
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // prompt for the AI
        string sourceTable = SystemPrompt + tableDefinition + "; ";

        // Create the JSON request content
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new OllamaRequest{ model = Model, prompt = question, stream = false, system = sourceTable }),
            Encoding.UTF8,
            mediaType);

        // Send a POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(OllamaApiPath, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response as json object
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);

            if(jsonObject == null)
                return "ERROR: No response from Ollama API!";

            // Return the answer from the json object
            return jsonObject.response;
        }
        else
        {
            throw new Exception($"Error calling Ollama API: {response.StatusCode}");
        }
    }
}

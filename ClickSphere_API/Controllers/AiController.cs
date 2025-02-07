using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClickSphere_API.Services;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Models;
using ClickSphere_API.Tools;
namespace ClickSphere_API.Controllers;

/// <summary>
/// Interface to the Ollama AI service.
/// </summary>
[ApiController]
public class AiController(IAiService AiService, IDbService DbService, IRagService RagService) : ControllerBase
{
    /// <summary>
    /// Get available models from Ollama
    /// </summary>
    /// <returns>List of available models</returns>
    [Route("/getModels")]
    [HttpGet]
    public async Task<IList<string>> GetModels()
    {
        // Call the Ollama API
        IList<string> response = await AiService.GetModelsAsync();
        return response;
    }

    /// <summary>
    /// Pull a specific model from Ollama
    /// </summary>
    /// <param name="model">The model to pull</param>
    /// <returns>The model</returns>
    [Route("/pullModel")]
    [HttpGet]
    public async Task<string> PullModel(string model)
    {
        // Call the Ollama API
        Result response = await AiService.PullModelAsync(model);
        return response.Output;
    }

    /// <summary>
    /// Ask the AI a question. Call the Ollama API
    /// </summary>
    /// <param name="request">The request to ask a question.</param>
    /// <returns>The answer to the question.</returns>
    [Authorize]
    [Route("/ask")]
    [HttpPost]
    public async Task<string> Ask([FromBody] TextRequest request)
    {
        string question = request.Text;
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question is empty");
        }

        // Call the Ollama API
        string response = await AiService.Ask(question);
        return response;
    }

    /// <summary>
    /// Use the AI to translate a text into English.
    /// </summary>
    /// <param name="request">The request to translate a text.</param>
    /// <returns>The translated text.</returns>
    [Route("/translate")]
    [HttpPost]
    public async Task<string> Translate([FromBody] TextRequest request)
    {
        string text = request.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is empty");
        }

        // Call the Ollama API
        string response = await AiService.Translate(text);
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
    public AiConfig GetAiConfig(string type)
    {
        // Call the Ollama API
        AiConfig response = DbService.GetAiConfig(type);
        return response;
    }

    /// <summary>
    /// Set the system configuration.
    /// </summary>
    /// <param name="request">The request to set the system configuration.</param>
    /// <returns>The system configuration.</returns>
    [Authorize]
    [Route("/setAiConfig")]
    [HttpPost]
    public async Task SetAiConfig(SetAIConfigRequest request)
    {
        if (request.Config == null)
        {
            throw new ArgumentException("Config is empty");
        }

        string type = request.Type;
        AiConfig config = request.Config;

        // Call the Ollama API
        await DbService.SetAiConfig(type, config);
    }
}

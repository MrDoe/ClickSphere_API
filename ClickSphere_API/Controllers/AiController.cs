using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClickSphere_API.Services;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Models;
namespace ClickSphere_API.Controllers;

/// <summary>
/// Interface to the Ollama AI service.
/// </summary>
[ApiController]
public class AiController(IAiService AiService) : ControllerBase
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
        if(request.Question == null || request.Database == null || request.Table == null)
        {
            return "Invalid request";
        }

        // Call the Ollama API
        string response = await AiService.GenerateQuery(request.Question, request.Database, request.Table);        
        return response;
    }

    /// <summary>
    /// Generate a SQL query and execute it on the specified database.
    /// </summary>
    /// <param name="question" example="Calculate the average traveling time (in minutes) for all trips with a pickup date between 2015-01-01 and 2015-12-31.">The question to ask.</param>
    /// <param name="database" example="default">The database to execute the query on.</param>
    /// <param name="table" example="trips">The table to ask the question about.</param>
    /// <returns>The result of the query execution.</returns>
    [Authorize]
    [Route("/generateAndExecuteQuery")]
    [HttpPost]
    public async Task<string> GenerateAndExecuteQuery(string question, string database, string table)
    {
        // Call the Ollama API
        string response = await AiService.GenerateAndExecuteQuery(question, database, table);
        return response;
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
        IDictionary<string, string> response = await AiService.GetColumnDescriptions(database, table);
        return response;
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
}

using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
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
    /// <param name="database" example="default">The database to execute the query on.</param>
    /// <param name="table" example="trips">The table to ask the question about.</param>
    /// <param name="question" example="Calculate the average traveling time (in minutes) for all trips with a pickup date between 2015-01-01 and 2015-12-31.">The question to ask.</param>
    /// <returns>The answer to the question.</returns>
    [Route("/generateQuery")]
    [HttpPost]
    public async Task<string> GenerateQuery(string question, string database, string table)
    {
        // Call the Ollama API
        string response = await AiService.GenerateQuery(question, database, table);
        return response;
    }

    /// <summary>
    /// Generate a SQL query and execute it on the specified database.
    /// </summary>
    /// <param name="question" example="Calculate the average traveling time (in minutes) for all trips with a pickup date between 2015-01-01 and 2015-12-31.">The question to ask.</param>
    /// <param name="database" example="default">The database to execute the query on.</param>
    /// <param name="table" example="trips">The table to ask the question about.</param>
    /// <returns>The result of the query execution.</returns>
    [Route("/generateAndExecuteQuery")]
    [HttpPost]
    public async Task<string> GenerateAndExecuteQuery(string question, string database, string table)
    {
        // Call the Ollama API
        string response = await AiService.GenerateAndExecuteQuery(question, database, table);
        return response;
    }
}

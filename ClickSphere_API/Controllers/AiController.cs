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
    /// <param name="question" example="How can I create a new view?">The question to ask.</param>
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
    /// Ask the AI a question. Call the Ollama API
    /// </summary>
    /// <param name="table" example="trips">The table to ask the question about.</param>
    /// <param name="question" example="Calculate the average traveling time in minutes for each row.">The question to ask.</param>
    /// <returns>The answer to the question.</returns>
    [Route("/generateQuery")]
    [HttpPost]
    public async Task<string> GenerateQuery(string question, string table)
    {
        // Call the Ollama API
        string response = await AiService.GenerateQuery(question, table);
        return response;
    }
}

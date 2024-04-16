using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
namespace ClickSphere_API.Controllers;

[ApiController]
public class AiController : ControllerBase
{
    /// <summary>
    /// Ask the AI a question. Call the Ollama API
    /// </summary>
    /// <param name="question">The question to ask the AI.</param>
    /// <returns>The answer to the question.</returns>
    [Route("/ask")]
    [HttpPost]
    public async Task<string> Ask([FromBody] string question)
    {
        // Call the Ollama API
        string response = await AiService.Ask(question);
        return response;
    }
}

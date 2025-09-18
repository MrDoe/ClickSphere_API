namespace ClickSphere_API.Models;
/// <summary>
/// AiConfig is a model class that holds the configuration for the system.
/// </summary>
public class AiConfig
{
    /// <summary>
    /// The URL of the Ollama API.
    /// </summary>
    public string? OllamaUrl { get; set; }

    /// <summary>
    /// The LLM to use for Ollama.
    /// </summary>
    public string? OllamaModel { get; set; }

    /// <summary>
    /// The system prompt to use for Ollama.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// The thinking mode for reasoning models.
    /// </summary>
    public bool? Think { get; set; }

    /// <summary>
    /// Auto refine input for the AI search.
    /// </summary>
    public bool AutoRefine { get; set; } = false;
}

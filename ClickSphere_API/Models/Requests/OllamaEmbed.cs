namespace ClickSphere_API.Models.Requests;

/// <summary>
/// Request object for the Ollama Embed endpoint.
/// </summary>
public class OllamaEmbed
{
    /// <summary>
    /// name of the model to generate embeddings from.
    /// </summary>
    public string model { get; set; } = "";

    /// <summary>
    /// text or list of text to generate embeddings for.
    /// </summary>
    public string input { get; set; } = "";

    /// <summary>
    /// truncates the end of each input to fit within context length. Returns error if false and context length is exceeded. Defaults to true.
    /// </summary>
    public bool truncate { get; set; } = true;

    /// <summary>
    /// additional model parameters
    /// </summary>
    public OllamaRequestOptions options { get; set; } = new OllamaRequestOptions();

    /// <summary>
    /// controls how long the model will stay in memory after the last request. Defaults to 30 minutes.
    /// </summary>
    public string keep_alive { get; set; } = "30m";
}
namespace ClickSphere_API.Models.Requests;

/// <summary>
/// Represents the response from the Ollama API.
/// </summary>
public class OllamaEmbedResponse
{
    /// <summary>
    /// The LLM used by Ollama.
    /// </summary>
    public string? model { get; set; }

    /// <summary>
    /// The resulting vector of embeddings.
    /// </summary>
    public List<List<float>>? embeddings { get; set; }

    /// <summary>
    /// The time it took to generate the embeddings.
    /// </summary>
    public long? total_duration { get; set; }

    /// <summary>
    ///  The time it took to load the model.
    /// </summary>
    public long? load_duration { get; set; }

    /// <summary>
    /// Prompt evaluation count.
    /// </summary>
    public int? prompt_eval_count { get; set; }
}
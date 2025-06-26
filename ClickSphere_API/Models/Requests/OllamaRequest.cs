namespace ClickSphere_API.Models.Requests;

/// <summary>
/// Represents a request to the Ollama API.
/// </summary>
public class OllamaRequest
{
    /// <summary>
    /// The model name (required)
    /// </summary>
    public required string model { get; set; }

    /// <summary>
    /// Gets or sets the prompt to generate the response for.
    /// </summary>
    public string? prompt { get; set; }

    /// <summary>
    /// The format to return a response in. Currently the only accepted value is json.
    /// </summary>
    public string? format { get; set; }

    /// <summary>
    /// Additional model parameters listed in the documentation for the Modelfile such as temperature.
    /// </summary>
    public OllamaRequestOptions? options { get; set; }

    /// <summary>
    /// System message to (overrides what is defined in the Modelfile).
    /// </summary>
    public string? system { get; set; }

    /// <summary>
    /// The prompt template to use (overrides what is defined in the Modelfile).
    /// </summary>
    public string? template { get; set; }

    /// <summary>
    /// The context paramter returned from a previous request to /generate, this can be used to keep a short conversational memory.
    /// </summary>
    public string? context { get; set; }

    /// <summary>
    /// If false the response will be returned as a single response object, rather than a stream of objects.
    /// </summary>
    public bool? stream { get; set; }

    /// <summary>
    /// If true no formatting will be applied to the prompt. You may choose to use the raw parameter if you are specifying a full templated prompt in your request to the API
    /// </summary>
    public bool? raw { get; set; }

    /// <summary>
    /// Controls how long the model will stay loaded into memory following the request (default: 5m)
    /// </summary>
    public string? keep_alive { get; set; }
}

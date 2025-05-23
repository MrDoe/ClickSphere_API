using System.Collections.Generic;
namespace ClickSphere_API.Models.Requests;

/// <summary>
/// Represents the request for Ollama models.
/// </summary>
public class OllamaModelRequest
{
    /// <summary>
    /// The models available in Ollama.
    /// </summary>
    public List<OllamaModels> models { get; set; } = new();
}

/// <summary>
/// Represents the details of an Ollama model.
/// </summary>
public class OllamaModelDetails
{
    /// <summary>
    /// The format of the model.
    /// </summary>
    public string? format { get; set; }
    
    /// <summary>
    /// The family of the model.
    /// </summary>
    public string? family { get; set; }

    /// <summary>
    /// The families of the model.
    /// </summary>
    public string[]? families { get; set; }
    
    /// <summary>
    /// The size of the model.
    /// </summary>
    public string? parameter_size { get; set; }

    /// <summary>
    /// The quantization level of the model.
    /// </summary>
    public string? quantization_level { get; set; }
}

/// <summary>
/// Represents the Ollama models.
/// </summary>
public class OllamaModels
{
    /// <summary>
    /// The name of the model.
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// The modified date of the model.
    /// </summary>
    public string? modified_at { get; set; }

    /// <summary>
    /// The size of the model.
    /// </summary>
    public long size { get; set; }

    /// <summary>
    /// The digest of the model.
    /// </summary>
    public string? digest { get; set; }

    /// <summary>
    /// The details of the model.
    /// </summary>
    public OllamaModelDetails details { get; set; } = new OllamaModelDetails();
}

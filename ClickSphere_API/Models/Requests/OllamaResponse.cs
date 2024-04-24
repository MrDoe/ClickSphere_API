namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the response from the Ollama API.
/// </summary>
public class OllamaResponse
{
    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public required string model { get; set; }
    
    /// <summary>
    /// Gets or sets the creation date and time.
    /// </summary>
    public required DateTime created_at { get; set; }
    
    /// <summary>
    /// Gets or sets the response.
    /// </summary>
    public required string response { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the response is done.
    /// </summary>
    public required bool done { get; set; }
}

/// <summary>
/// Represents the response from the Ollama API.
/// </summary>
public class Response
{
    /// <summary>
    /// Gets or sets the query.
    /// </summary>
    public string? query { get; set; }
    
    /// <summary>
    /// Gets or sets the answer.
    /// </summary>
    public string? answer { get; set; }
    
    /// <summary>
    /// Gets or sets the explanation.
    /// </summary>
    public string? explanation { get; set; }
    
    /// <summary>
    /// Gets or sets the SQL statement.
    /// </summary>
    public string? sql { get; set; }
}
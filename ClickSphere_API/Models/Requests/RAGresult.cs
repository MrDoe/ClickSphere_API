using System.Text.Json.Serialization;
namespace ClickSphere_API.Models.Requests;

/// <summary>
/// Result type for the RAG queries
/// </summary>
public class RAGresult
{
    /// <summary>
    /// The session id of the respective RAG query
    /// </summary>
    [JsonPropertyName("sessionId")]
    public required string SessionId { get; set; }

    /// <summary>
    /// The result count of the respective RAG query
    /// </summary>
    [JsonPropertyName("resultCount")]
    public required long ResultCount { get; set; }
}

using System.Text.Json.Serialization;
namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to translate a text.
/// </summary>
public class SetAIConfigRequest
{
    /// <summary>
    /// Gets or sets the text to translate.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    /// <summary>
    /// Gets or sets the AI configuration.
    /// </summary>
    [JsonPropertyName("config")]
    public AiConfig Config { get; set; } = default!;
}

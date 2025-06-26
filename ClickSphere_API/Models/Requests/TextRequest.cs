using System.Text.Json.Serialization;
namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to translate a text.
/// </summary>
public class TextRequest
{
    /// <summary>
    /// Gets or sets the text to translate.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets the thinking mode for reasoning models.
    /// </summary>
    [JsonPropertyName("aiconfig")]
    public string AiConfig { get; set; } = "Text2SQLConfig";
}

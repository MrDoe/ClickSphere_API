
namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to ask a question.
/// </summary>
public class AskRequest
{
    /// <summary>
    /// Gets or sets the question.
    /// </summary>
    public string? question { get; set; }
    
    /// <summary>
    /// Gets or sets the table.
    /// </summary>
    public string? table { get; set; }
}

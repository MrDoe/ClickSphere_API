namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a document.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the id of the document.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    public string? Filename { get; set; }
    /// <summary>
    /// Gets or sets the content of the document.
    /// </summary>
    public string? Content { get; set; }
}
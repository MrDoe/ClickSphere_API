using System.Text.Json.Serialization;

namespace ClickSphere_API.Models;

/// <summary>
/// Represents a database view.
/// </summary>
public class View
{
    /// <summary>
    /// Gets or sets the ID of the view.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the view.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the view.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the database associated with the view.
    /// </summary>
    [JsonPropertyName("database")]
    public string Database { get; set; }
    
    /// <summary>
    /// Gets or sets the query used by the view.
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the view.
    /// V for standard view, M for materialized view.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the example questions for the view.
    /// </summary>
    [JsonPropertyName("questions")]
    public string Questions { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="View"/> class.
    /// </summary>
    public View()
    {
        Id = "";
        Name = "";
        Database = "";
        Query = "";
        Questions = "";
        Type = "V";
    }
}

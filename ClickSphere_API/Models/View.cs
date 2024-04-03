using System.Text.Json.Serialization;

namespace ClickSphere_API.Models;
public class View
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("database")]
    public string Database { get; set; }
    
    [JsonPropertyName("query")]
    public string Query { get; set; }
    
    public View()
    {
        Id = "";
        Name = "";
        Database = "";
        Query = "";
    }
}

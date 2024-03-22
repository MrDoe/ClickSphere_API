using System.Text.Json.Serialization;
namespace ClickSphere_API.Models;
public class UserRole
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

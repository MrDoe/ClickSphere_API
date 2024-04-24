using System.Text.Json.Serialization;
namespace ClickSphere_API.Models;

/// <summary>
/// Represents a user role.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Gets or sets the unique identifier of the user role.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the user role.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

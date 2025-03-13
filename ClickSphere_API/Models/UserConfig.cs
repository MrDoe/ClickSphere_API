using System.Text.Json.Serialization;
namespace ClickSphere_API.Models;

/// <summary>
/// Represents a user configuration object.
/// </summary>
public class UserConfig
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    [JsonPropertyName("userName")]
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the LDAP user of the user.
    /// </summary>
    [JsonPropertyName("ldapUser")]
    public string? LDAP_User { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the department of the user.
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the role of the user.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the language preference of the user.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

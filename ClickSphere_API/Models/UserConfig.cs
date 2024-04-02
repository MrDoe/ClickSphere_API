using System.Text.Json.Serialization;
namespace ClickSphere_API.Models;
public class UserConfig
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [JsonPropertyName("userName")]
    public required string Username { get; set; }

    [JsonPropertyName("ldapUser")]
    public string? LDAP_User { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

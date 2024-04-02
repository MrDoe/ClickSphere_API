namespace ClickSphere_API.Models.Requests;
public class CreateUserRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? LDAP_User { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
}

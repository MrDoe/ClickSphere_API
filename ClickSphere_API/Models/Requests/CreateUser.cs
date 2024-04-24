namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to create a user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the LDAP user of the user.
    /// </summary>
    public string? LDAP_User { get; set; }

    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the department of the user.
    /// </summary>
    public string? Department { get; set; }
}

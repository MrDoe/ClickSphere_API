namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a login request.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public required string Password { get; set; }
}


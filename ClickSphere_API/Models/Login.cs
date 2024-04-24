namespace ClickSphere_API.Models;

/// <summary>
/// Represents a login object containing the username, token, and role.
/// </summary>
public class Login
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public string? Role { get; set; }
}

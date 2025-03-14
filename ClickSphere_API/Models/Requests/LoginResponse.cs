namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the response for a login request.
/// </summary>
public class LoginRequestResponse
{
    /// <summary>
    /// Gets or sets the type of the access token.
    /// </summary>
    public string? tokenType { get; set; }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string? accessToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the access token in seconds.
    /// </summary>
    public int expiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string? refreshToken { get; set; }

    /// <summary>
    /// Gets or sets the language of the user.
    /// </summary>
    /// <value>The language of the user.</value>
    public string? language { get; set; }
}
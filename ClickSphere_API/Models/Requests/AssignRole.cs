namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to assign a role to a user.
/// </summary>
public class AssignRoleRequest
{
    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public required string UserName { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public required string RoleName { get; set; }
}

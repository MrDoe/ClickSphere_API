namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to add a view to a role.
/// </summary>
public class AddViewToRoleRequest
{
    /// <summary>
    /// Gets or sets the ID of the view.
    /// </summary>
    public required string ViewId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public required string RoleName { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public required string Database { get; set; }
}

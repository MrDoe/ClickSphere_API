namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a request to get views for a role.
/// </summary>
public class GetViewsForRoleRequest
{
    /// <summary>
    /// Gets or sets the ID of the view.
    /// </summary>
    public string? ViewId { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string? Database { get; set; }
}

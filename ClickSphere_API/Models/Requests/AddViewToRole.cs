namespace ClickSphere_API.Models.Requests;
public class AddViewToRoleRequest
{
    public required string ViewId { get; set; }
    public required string RoleName { get; set; }
    public required string Database { get; set; }
}

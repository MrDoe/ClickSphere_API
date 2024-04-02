namespace ClickSphere_API.Models.Requests;
public class AssignRoleRequest
{
    public required string UserName { get; set; }
    public required string RoleName { get; set; }
}

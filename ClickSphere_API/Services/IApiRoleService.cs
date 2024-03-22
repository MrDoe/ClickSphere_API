using ClickSphere_API.Tools;
using ClickSphere_API.Models;
namespace ClickSphere_API.Services;
public interface IApiRoleService
{
    Task<Result> CreateRole(string roleName);
    Task<Result> DeleteRole(string roleName);
    Task<List<UserRole>> GetRoles();
    Task<UserRole?> GetRoleFromUser(string userName);
    Task<Result> AssignRoleToUser(string userName, string roleName);
    Task<Result> RemoveRoleFromUser(string userName, string roleName);
}
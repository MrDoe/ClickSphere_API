using System.Security.Claims;
using ClickSphere_API.Models;

namespace ClickSphere_API.Services
{
    public interface IUserService
    {
        Task<bool> CreateUser(string username, string password);
        Task<ClaimsPrincipal?> CheckLogin(string username, string password);
        Task<List<UserConfig>> GetUsers();
        Task<List<Role>> GetRoles();
        Task<Role?> GetUserRole(string userName);
        Task<bool> DeleteUser(string userName);
        Task<bool> AssignRole(string userName, string roleName);
        Task<bool> RemoveRole(string userName, string roleName);
        Task<bool> UpdatePassword(string userName, string newPassword);
        Task<UserConfig?> GetUserConfig(string userName);
    }
}
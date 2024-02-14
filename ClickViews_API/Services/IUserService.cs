using System.Security.Claims;
using ClickViews_API.Models;

namespace ClickViews_API.Services
{
    public interface IUserService
    {
        Task<bool> CreateUser(string username, string password);
        Task<ClaimsPrincipal?> CheckLogin(string username, string password);
        Task<List<User>> GetUsers();
        Task<List<Role>> GetRoles();
        Task<Role?> GetUserRole(string userName);
        Task<bool> DeleteUser(string userName);
        Task<bool> AssignRole(string userName, string roleName);
        Task<bool> RemoveRole(string userName, string roleName);
    }
}
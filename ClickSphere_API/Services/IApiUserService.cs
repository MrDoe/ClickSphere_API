using System.Security.Claims;
using ClickSphere_API.Models;
using ClickSphere_API.Tools;
namespace ClickSphere_API.Services
{
    public interface IApiUserService
    {
        Task<Result> CreateUser(string username, string password);
        Task<ClaimsPrincipal?> CheckLogin(string username, string password);
        Task<List<UserConfig>> GetUsers();
        Task<Result> DeleteUser(string userName);
        Task<Result> UpdatePassword(string userName, string newPassword);
        Task<UserConfig?> GetUserConfig(Guid userId);
        Task<Result> UpdateUser(UserConfig user);
    }
}
using System.Security.Claims;
using ClickSphere_API.Models;
using ClickSphere_API.Tools;

namespace ClickSphere_API.Services;

/// <summary>
/// Represents the interface for managing API users.
/// </summary>
public interface IApiUserService
{
    /// <summary>
    /// Creates a new user with the specified username and password.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> CreateUser(string username, string password);

    /// <summary>
    /// Checks the login credentials of the user with the specified username and password.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the claims principal if the login is successful; otherwise, null.</returns>
    Task<ClaimsPrincipal?> CheckLogin(string username, string password);

    /// <summary>
    /// Retrieves a list of all user configurations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the list of user configurations.</returns>
    Task<List<UserConfig>> GetUsers();

    /// <summary>
    /// Deletes the user with the specified username.
    /// </summary>
    /// <param name="userName">The username of the user to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> DeleteUser(string userName);

    /// <summary>
    /// Updates the password of the user with the specified username.
    /// </summary>
    /// <param name="userName">The username of the user.</param>
    /// <param name="newPassword">The new password for the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> UpdatePassword(string userName, string newPassword);

    /// <summary>
    /// Retrieves the user configuration for the user with the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the user configuration if found; otherwise, null.</returns>
    Task<UserConfig?> GetUserConfig(Guid userId);

    /// <summary>
    /// Updates the user configuration for the specified user.
    /// </summary>
    /// <param name="user">The updated user configuration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> UpdateUser(UserConfig user);
}
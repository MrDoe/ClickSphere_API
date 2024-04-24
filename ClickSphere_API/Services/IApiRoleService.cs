using ClickSphere_API.Tools;
using ClickSphere_API.Models;
namespace ClickSphere_API.Services;
/// <summary>
/// Represents an interface for managing API roles.
/// </summary>
public interface IApiRoleService
{
    /// <summary>
    /// Creates a new role with the specified name.
    /// </summary>
    /// <param name="roleName">The name of the role to create.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> CreateRole(string roleName);

    /// <summary>
    /// Deletes the role with the specified name.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> DeleteRole(string roleName);

    /// <summary>
    /// Gets a list of all user roles.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation and contains the list of user roles.</returns>
    Task<List<UserRole>> GetRoles();

    /// <summary>
    /// Gets the role with the specified ID.
    /// </summary>
    /// <param name="roleId">The ID of the role to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation and contains the role with the specified ID, or null if not found.</returns>
    Task<UserRole?> GetRoleById(Guid roleId);

    /// <summary>
    /// Gets the role associated with the specified user.
    /// </summary>
    /// <param name="userName">The name of the user.</param>
    /// <returns>A task that represents the asynchronous operation and contains the role associated with the specified user, or null if not found.</returns>
    Task<UserRole?> GetRoleFromUser(string userName);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userName">The name of the user.</param>
    /// <param name="roleName">The name of the role to assign.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> AssignRoleToUser(string userName, string roleName);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userName">The name of the user.</param>
    /// <param name="roleName">The name of the role to remove.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> RemoveRoleFromUser(string userName, string roleName);

    /// <summary>
    /// Gets a list of views associated with the specified role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>A task that represents the asynchronous operation and contains the list of views associated with the specified role.</returns>
    Task<List<View>> GetViewsForRole(string roleName);

    /// <summary>
    /// Adds a view to a role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="viewID">The ID of the view to add.</param>
    /// <param name="database">The name of the database.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> AddViewToRole(string roleName, string viewID, string database);

    /// <summary>
    /// Removes a view from a role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="viewID">The ID of the view to remove.</param>
    /// <param name="database">The name of the database.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> RemoveViewFromRole(string roleName, string viewID, string database);

    /// <summary>
    /// Revokes all roles from a view.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="viewId">The ID of the view.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result of the operation.</returns>
    Task<Result> RevokeRolesFromView(string database, string viewId);
}
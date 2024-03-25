using ClickSphere_API.Models;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Services;
using ClickSphere_API.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClickSphere_API.Controllers;

[ApiController]
public class RoleController(IApiRoleService RoleService) : ControllerBase
{
    /// <summary>
    /// Create a new role in ClickHouse database system
    /// </summary>
    /// <param name="roleName">The role to create</param>
    /// <returns>The created role</returns>
    [Authorize]
    [HttpPost]
    [Route("/createRole")]
    public async Task<IResult> CreateRole([FromBody] string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return Results.BadRequest("Role name is required");

        Result result = await RoleService.CreateRole(roleName);
        if (result.IsSuccessful)
            return Results.Ok();
        else
            return Results.BadRequest(result.Output);
    }

    /// <summary>
    /// Delete a role from the ClickHouse database system
    /// </summary>
    /// <param name="roleName">The name of the role to delete</param>
    /// <returns>True if the role was deleted, otherwise false</returns>
    [Authorize]
    [HttpDelete]
    [Route("/deleteRole")]
    public async Task<IResult> DeleteRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return Results.BadRequest("Role name is required");

        Result result = await RoleService.DeleteRole(roleName);
        if (result.IsSuccessful)
            return Results.Ok();
        else
            return Results.BadRequest(result.Output);
    }

    /// <summary>
    /// Retrieve all roles from the database system
    /// </summary>
    /// <returns>A list of strings representing all roles</returns>
    [Authorize]
    [HttpGet]
    [Route("/getRoles")]
    public async Task<IResult> GetRoles()
    {
        List<UserRole> roles = await RoleService.GetRoles();
        return Results.Ok(roles);
    }
    
    /// <summary>
    /// Assign a role to a user
    /// </summary>
    /// <param name="request">The assignment request containing the user name and role name</param>
    /// <returns>True if the role was assigned, otherwise false</returns>
    [Authorize]
    [HttpPost]
    [Route("/assignRoleToUser")]
    public async Task<IResult> AssignRoleToUser([FromBody] AssignRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.RoleName))
            return Results.BadRequest("User name and role name are required");

        Result result = await RoleService.AssignRoleToUser(request.UserName, request.RoleName);
        if (result.IsSuccessful)
            return Results.Ok();
        else
            return Results.BadRequest(result.Output);
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    /// <param name="request">The assignment request containing the user name and role name</param>
    /// <returns>True if the role was removed, otherwise false</returns>
    [Authorize]
    [HttpPost]
    [Route("/removeRoleFromUser")]
    public async Task<IResult> RemoveRole(AssignRoleRequest request)
    {
        if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.RoleName))
            return Results.BadRequest("User name and role name are required");

        Result result = await RoleService.RemoveRoleFromUser(request.UserName, request.RoleName);
        if (result.IsSuccessful)
            return Results.Ok();
        else
            return Results.BadRequest(result.Output);
    }

    /// <summary>
    /// Retrieve the roles associated with a user
    /// </summary>
    /// <param name="userName">The user name of the user</param>
    /// <returns>A list of strings representing the roles associated with the user</returns>
    [Authorize]
    [HttpGet]
    [Route("/getUserRole")]
    public async Task<UserRole?> GetUserRole(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return null;

        return await RoleService.GetRoleFromUser(userName) ?? null;
    }

    /// <summary>
    /// Get role by its id
    /// </summary>
    /// <param name="id">The id of the role</param>
    /// <returns>The role with the given id</returns>
    [Authorize]
    [HttpGet]
    [Route("/getRoleById")]
    public async Task<UserRole?> GetRoleById(Guid id)
    {
        return await RoleService.GetRoleById(id) ?? null;
    }

    /// <summary>
    /// Get views for role
    /// </summary>
    /// <param name="roleName">The name of the role</param>
    /// <returns>The views for the role</returns>
    [Authorize]
    [HttpGet]
    [Route("/getViewsForRole")]
    public async Task<List<ViewsForRole>> GetViewsForRole(string roleName)
    {
        return await RoleService.GetViewsForRole(roleName) ?? null;
    }
}
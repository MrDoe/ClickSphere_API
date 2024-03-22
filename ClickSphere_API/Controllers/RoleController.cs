using System;
using System.Collections.Generic;
using ClickSphere_API.Models;
using ClickSphere_API.Services;
using ClickSphere_API.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ClickSphere_API.Controllers;

[ApiController]
public class RoleController(IApiRoleService RoleService) : ControllerBase
{
    /**
    * Create a new role in ClickHouse database system
    * @param role The role to create
    * @return The created role
    */
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

    /**
    * Delete a role from the ClickHouse database system
    * @param roleName The name of the role to delete
    * @return True if the role was deleted, otherwise false
    */
    [Authorize]
    [HttpPost]
    [Route("/deleteRole")]
    public async Task<IResult> DeleteRole([FromBody] string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            return Results.BadRequest("Role name is required");

        Result result = await RoleService.DeleteRole(roleName);
        if (result.IsSuccessful)
            return Results.Ok();
        else
            return Results.BadRequest(result.Output);
    }

    /**
    * Retrieve all roles from the database system
    * @return A list of strings representing all roles
    */
    [Authorize]
    [HttpGet]
    [Route("/getRoles")]
    public async Task<IResult> GetRoles()
    {
        List<UserRole> roles = await RoleService.GetRoles();
        return Results.Ok(roles);
    }
    
    /**
    * Assign a role to a user
    * @param userName The user name of the user
    * @param roleName The name of the role to assign
    * @return True if the role was assigned, otherwise false
    */
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

    /**
    * Remove a role from a user
    * @param userName The user name of the user
    * @param roleName The name of the role to remove
    * @return True if the role was removed, otherwise false
    */
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

    /**
    * Retrieve the roles associated with a user
    * @param userName The user name of the user
    * @return A list of strings representing the roles associated with the user
    */
    [Authorize]
    [HttpGet]
    [Route("/getUserRole")]
    public async Task<UserRole> GetUserRole(string userName)
    {
        return await RoleService.GetRoleFromUser(userName);
    }
}

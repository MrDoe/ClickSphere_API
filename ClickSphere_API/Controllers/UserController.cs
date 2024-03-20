using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Models;
using ClickSphere_API.Services;
using ClickSphere_API.Tools;
using Microsoft.AspNetCore.Authentication;

namespace ClickSphere_API.Controllers
{
    /**
     * The base class for ClickSphere API controllers.
     */
    [ApiController]
    public class UserController(IApiUserService userService) : ControllerBase
    {
        private readonly IApiUserService _userService = userService;

        /**
          * This method is used to log in a user
          * @param model The user to log in
          * @return The result of the login
          */
        [AllowAnonymous]
        [HttpPost]
        [Route("/login")]
        public async Task<IResult> Login([FromBody] LoginRequest user)
        {
            if (user == null)
                return Results.BadRequest("User is required");
            if (user.Username == null || user.Password == null)
                return Results.BadRequest("Username and password are required");

            var claimsPrincipal = await _userService.CheckLogin(user.Username, user.Password);

            if (claimsPrincipal != null)
            {
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                return Results.SignIn(claimsPrincipal, authProperties);
            }
            else
                return Results.BadRequest("Could not verify username and password");
        }

        /**
        * This method is used to log out a user
        * @return The result of the logout
        */
        [Authorize]
        [HttpPost]
        [Route("/logout")]
        public async Task<IResult> Logout()
        {
            return Results.SignOut();
        }

        /**
        * Create new user
        * @param model The user to create
        * @return The result of the creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createUser")]
        public async Task<IResult> CreateUser([FromBody] CreateUserRequest user)
        {
            if (user == null)
                return Results.BadRequest("User is required");

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return Results.BadRequest("Username and password are required");

            Result result = await _userService.CreateUser(user.Username, user.Password);
            if (result.IsSuccessful)
                return Results.Ok();
            else
                return Results.BadRequest(result.Output);
        }

        /**
        * Delete user
        * @param model The user to delete^
        * @return The result of the deletion
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteUser")]
        public async Task<IResult> DeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return Results.BadRequest("Username is required");

            Result result = await _userService.DeleteUser(username);
            if (result.IsSuccessful)
                return Results.Ok();
            else
                return Results.BadRequest(result.Output);
        }

        /**
        * Get all users from the Users table
        * @return The list of users
        */
        [Authorize]
        [HttpGet]
        [Route("/getUsers")]
        public async Task<List<UserConfig>> GetUsers()
        {
            return await _userService.GetUsers();
        }

        /**
        * Get all roles from the Roles table
        * @return The list of roles
        */
        [Authorize]
        [HttpGet]
        [Route("/getRoles")]
        public async Task<List<Role>> GetRoles()
        {
            return await _userService.GetRoles();
        }

        /**
        * Get role from a specific user
        * @param userName The user name of the user
        * @return The role of the user
        */
        [Authorize]
        [HttpGet]
        [Route("/getUserRole")]
        public async Task<Role?> GetUserRole(string userName)
        {
            return await _userService.GetUserRole(userName);
        }

        /**
        * Assign role to a user
        * @param model The user and role to assign
        * @return The result of the assignment
        */
        [Authorize]
        [HttpPost]
        [Route("/assignRole")]
        public async Task<IResult> AssignRole([FromBody] AssignRoleRequest model)
        {
            if (model == null)
                return Results.BadRequest("User and role are required");

            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.RoleName))
                return Results.BadRequest("User and role are required");

            Result result = await _userService.AssignRole(model.Username, model.RoleName);
            if (result.IsSuccessful)
                return Results.Ok();
            else
                return Results.BadRequest(result.Output);
        }

        /**
        * Get user configuration from ClickSphere.users
        * @param userId The user id of the user
        * @return The user configuration
        */
        [Authorize]
        [HttpGet]
        [Route("/getUserConfig")]
        public async Task<IResult?> GetUserConfig(string userId)
        {
            if(Guid.TryParse(userId, out Guid uid))
            {
                var result = await _userService.GetUserConfig(uid);
                if (result != null)
                    return Results.Ok(result);
                else
                    return Results.BadRequest("User does not exist");
            }
            else
            {
                return Results.BadRequest("User ID has to be a valid GUID");
            }
        }

        /**
        * Update user configuration
        * @param model The user configuration to update
        * @return The result of the update
        */
        [Authorize]
        [HttpPost]
        [Route("/updateUser")]
        public async Task<IResult> UpdateUser([FromBody] UserConfig user)
        {
            if (user == null)
                return Results.BadRequest("User is required");

            Result result = await _userService.UpdateUser(user);
            if (result.IsSuccessful)
                return Results.Ok();
            else
                return Results.BadRequest(result.Output);
        }
    }
}

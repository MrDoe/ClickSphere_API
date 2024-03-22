using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Models;
using ClickSphere_API.Services;
using ClickSphere_API.Tools;
using Microsoft.AspNetCore.Authentication;

namespace ClickSphere_API.Controllers
{
    /// <summary>
    /// The base class for ClickSphere API controllers.
    /// </summary>
    [ApiController]
    public class UserController(IApiUserService userService) : ControllerBase
    {
        private readonly IApiUserService _userService = userService;

        /// <summary>
        /// This method is used to log in a user.
        /// </summary>
        /// <param name="model">The user to log in.</param>
        /// <returns>The result of the login.</returns>
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

        /// <summary>
        /// This method is used to log out a user.
        /// </summary>
        /// <returns>The result of the logout.</returns>
        [Authorize]
        [HttpPost]
        [Route("/logout")]
        public IResult Logout()
        {
            return Results.SignOut();
        }

        /// <summary>
        /// Create new user in the ClickSphere users table
        /// </summary>
        /// <param name="model">The user to create</param>
        /// <returns>The result of the creation</returns>
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

        /// <summary>
        /// Delete a user from the ClickSphere users table
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all users from the Users table
        /// </summary>
        /// <returns>The list of users</returns>
        [Authorize]
        [HttpGet]
        [Route("/getUsers")]
        public async Task<List<UserConfig>> GetUsers()
        {
            return await _userService.GetUsers();
        }

        /// <summary>
        /// Get user configuration from ClickSphere.users
        /// </summary>
        /// <param name="userId">The user id of the user</param>
        /// <returns>The user configuration</returns>
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

        /// <summary>
        /// Update user configuration.
        /// </summary>
        /// <param name="user">The user configuration to update.</param>
        /// <returns>The result of the update.</returns>
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

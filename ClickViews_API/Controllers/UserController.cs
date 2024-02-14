using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickViews_API.Models;
using ClickViews_API.Services;

namespace ClickViews_API.Controllers
{
    /**
     * The base class for ClickViews API controllers.
     */
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private IUserService _userService = userService;

        /**
          * This method is used to log in a user
          * @param model The user to log in
          * @return The result of the login
          */
        [AllowAnonymous]
        [HttpPost]
        [Route("/login")]
        public async Task<IResult> Login([FromBody] LoginModel user)
        {
            if (user == null)
                return Results.BadRequest("User is required");
            if (user.UserName == null || user.Password == null)
                return Results.BadRequest("Username and password are required");

            var claimsPrincipal = await _userService.CheckLogin(user.UserName, user.Password);

            if (claimsPrincipal != null)
                return Results.SignIn(claimsPrincipal);
            else
                return Results.BadRequest("Could not verify username and password");
        }

        /**
        * Create new user
        * @param model The user to create
        * @return The result of the creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createUser")]
        public async Task<IResult> CreateUser([FromBody] LoginModel user)
        {
            if (user == null)
                return Results.BadRequest("User is required");

            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
                return Results.BadRequest("Username and password are required");

            bool result = await _userService.CreateUser(user.UserName, user.Password);
            if (result)
                return Results.Ok();
            else
                return Results.BadRequest("Could not create user: User already exists.");
        }

        /**
        * Delete user
        * @param model The user to delete^
        * @return The result of the deletion
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteUser")]
        public async Task<IResult> DeleteUser([FromBody] LoginModel user)
        {
            if (user == null)
                return Results.BadRequest("User is required");

            if (string.IsNullOrEmpty(user.UserName))
                return Results.BadRequest("Username is required");

            bool result = await _userService.DeleteUser(user.UserName);
            if (result)
                return Results.Ok();
            else
                return Results.BadRequest("Could not delete user: User does not exist.");
        }

        /**
        * Get all users from the Users table
        * @return The list of users
        */
        [Authorize]
        [HttpGet]
        [Route("/getUsers")]
        public async Task<IEnumerable<User>> GetUsers()
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

            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.RoleName))
                return Results.BadRequest("User and role are required");

            bool result = await _userService.AssignRole(model.UserName, model.RoleName);
            if (result)
                return Results.Ok();
            else
                return Results.BadRequest("Could not assign role: User or role does not exist.");
        }
    }
}

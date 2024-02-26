using System.Security.Claims;
using ClickSphere_API.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ClickSphere_API.Services
{
    /**
     * This class is used to manage user accounts
     */
    public class UserService(IDbService dbService) : IUserService
    {
        private readonly IDbService _dbService = dbService;

        /**
        * This method creates a new user
        * @param username The username of the user
        * @param password The password of the user
        * @return True if the user was created, otherwise false
        */
        public async Task<bool> CreateUser(string username, string password)
        {
            // check if user exists
            string query = $"SELECT name FROM system.users WHERE name = '{username}'";
            var result = await _dbService.ExecuteScalar(query);
            if (result != null)
                return false;
            
            // create the user
            query = $"CREATE USER {username} IDENTIFIED BY '{password}'";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method is used to check if the login credentials are valid
        * @param username The username of the user
        * @param password The password of the user
        * @return A ClaimsPrincipal object that contains the user's claims if the login credentials are valid, otherwise null
        */
        public async Task<ClaimsPrincipal?> CheckLogin(string username, string password)
        {
            if (await _dbService.CheckLogin(username, password))
            {
                var claimsPrincipal = new ClaimsPrincipal(
                  new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, username)],
                    BearerTokenDefaults.AuthenticationScheme
                  ));
                return claimsPrincipal;
            }
            return null;
        }

        /**
        * This method retrieves all users from the Users table
        * @return A list of User objects representing the users in the table
        */
        public async Task<List<CreatUserRequest>> GetUsers()
        {
            string query = "SELECT id, name FROM system.users";
            var result = await _dbService.ExecuteQueryDictionary(query);

            List<CreatUserRequest> users = [];
            foreach (var row in result)
            {
                CreatUserRequest user = new()
                {
                    UserId = Guid.Parse(row["id"].ToString()!),
                    UserName = row["name"].ToString()!
                };

                users.Add(user);
            }

            return users;
        }

        /**
        * This method retrieves all roles from the UserRoles table
        * @return A list of strings representing all roles
        */
        public async Task<List<Role>> GetRoles()
        {
            string query = $"SELECT id, name FROM system.roles";
            var result = await _dbService.ExecuteQueryDictionary(query);
            
            List<Role> roles = result.Select(row => new Role
            {
                RoleId = Guid.Parse(row["id"].ToString()!),
                RoleName = row["name"].ToString()!
            }).ToList();

            return roles;
        }

        /**
        * This method retrieves the roles associated with a user from the UserRoles table
        * @param userName The user name of the user
        * @return A list of strings representing the roles associated with the user
        */
        public async Task<Role?> GetUserRole(string userName)
        {
            string query = $"SELECT granted_role_name, granted_role_id from system.role_grants where user_name = '{userName}'";
            var result = await _dbService.ExecuteQueryDictionary(query);
            var role = result.Select(row => new Role
            {
                RoleId = Guid.Parse(row["granted_role_id"].ToString()!),
                RoleName = row["granted_role_name"].ToString()!
            }).FirstOrDefault();

            return role;
        }

        /**
        * This method assigns a role to a user
        * @param userName The user name of the user
        * @param roleName The name of the role to assign
        * @return True if the role was assigned, otherwise false
        */
        public async Task<bool> AssignRole(string userName, string roleName)
        {
            string query = $"GRANT ROLE {roleName} TO USER {userName}";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method removes a role from a user
        * @param userName The user name of the user
        * @param roleName The name of the role to remove
        * @return True if the role was removed, otherwise false
        */
        public async Task<bool> RemoveRole(string userName, string roleName)
        {
            string query = $"REVOKE ROLE {roleName} FROM USER {userName}";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method deletes a user
        * @param userName The user name of the user
        * @return True if the user was deleted, otherwise false
        */
        public async Task<bool> DeleteUser(string userName)
        {
            string query = $"DROP USER {userName}";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method updates the password of a user
        * @param userName The user name of the user
        * @param newPassword The new password of the user
        * @return True if the password was updated, otherwise false
        */
        public async Task<bool> UpdatePassword(string userName, string newPassword)
        {
            string query = $"ALTER USER {userName} IDENTIFIED BY '{newPassword}'";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method gets the user configuration from ClickSphere.Users
        * @param userName The user name of the user
        * @return The user configuration
        */
        public async Task<UserConfig?> GetUserConfig(string userName)
        {
            string query = $"SELECT * FROM ClickSphere.User WHERE name = '{userName}'";
            var result = await _dbService.ExecuteQueryDictionary(query);
            var userConfig = result.Select(row => new UserConfig
            {
                UserName = row["Name"].ToString()!,
                LDAP_User = row["LDAP_User"].ToString()!
            }).FirstOrDefault();

            return userConfig;
        }
    }
}
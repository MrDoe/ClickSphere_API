using System.Security.Claims;
using ClickSphere_API.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;
using ClickSphere_API.Tools;

namespace ClickSphere_API.Services
{
    /// <summary>
    /// This class is used to manage user accounts
    /// </summary>
    public class ApiUserService(IDbService dbService) : IApiUserService
    {
        private readonly IDbService _dbService = dbService;

        /// <summary>
        /// This method creates a new user
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="password">The password of the user</param>
        /// <returns>True if the user was created, otherwise false</returns>
        public async Task<Result> CreateUser(string username, string password)
        {
            // check if user exists
            string query = $"SELECT UserName FROM ClickSphere.Users WHERE UserName = '{username}'";
            var result = await _dbService.ExecuteScalar(query);
            if (result is not DBNull)
                return Result.BadRequest("User already exists");

            // create the user
            try
            {
                query = $"CREATE USER {username} IDENTIFIED BY '{password}'";
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
                // continue
            }

            // get the new user's id
            query = $"SELECT id FROM system.users WHERE name = '{username}'";
            result = await _dbService.ExecuteScalar(query);
            if (result is DBNull)
                return Result.BadRequest("Could not create user");
            
            var userId = result!.ToString();

            // create the user's configuration
            query = $"INSERT INTO ClickSphere.Users (Id, UserName, LDAP_User, Email, FirstName, LastName, Phone, Department) VALUES ('{userId}', '{username}', '', '', '', '', '', '')";
            await _dbService.ExecuteNonQuery(query);

            // check if insert was successful
            query = $"SELECT UserName FROM ClickSphere.Users WHERE UserName = '{username}'";
            result = await _dbService.ExecuteScalar(query);
            if (result is DBNull)
                return Result.BadRequest("Could not create user");

            // assign the default role to the user
            query = $"GRANT ROLE default TO USER {username}";
            try
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
                return Result.BadRequest("Could not assign default role");
            }
            return Result.Ok();
        }

        /// <summary>
        /// This method is used to check if the login credentials are valid
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="password">The password of the user</param>
        /// <returns>A ClaimsPrincipal object that contains the user's claims if the login credentials are valid, otherwise null</returns>
        public async Task<ClaimsPrincipal?> CheckLogin(string username, string password)
        {
            if (await _dbService.CheckLogin(username, password))
            {
                // get user's role
                string query = $"SELECT granted_role_name FROM system.role_grants WHERE user_name = '{username}'";
                var result = await _dbService.ExecuteScalar(query);
                string? role = result?.ToString()!;
                if (role == null)
                    return null;

                var claimsPrincipal = new ClaimsPrincipal(
                  new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, username)],
                    BearerTokenDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    role
                  ));
                return claimsPrincipal;
            }
            return null;
        }

        /// <summary>
        /// This method retrieves all users from the Users table
        /// </summary>
        /// <returns>A list of User objects representing the users in the table</returns>
        public async Task<List<UserConfig>> GetUsers()
        {
            string query = "SELECT toString(Id) as Id, UserName, LDAP_User, Email, FirstName, LastName, Phone, Department, Role from ClickSphere.Users";
            var result = await _dbService.ExecuteQueryDictionary(query);

            List<UserConfig> users = [];
            foreach (var row in result)
            {
                UserConfig user = new()
                {
                    Id = Guid.Parse(row["Id"].ToString()!),
                    Username = row["UserName"].ToString()!,
                    LDAP_User = row["LDAP_User"]?.ToString()!,
                    Email = row["Email"]?.ToString()!,
                    FirstName = row["FirstName"]?.ToString()!,
                    LastName = row["LastName"]?.ToString()!,
                    Phone = row["Phone"]?.ToString()!,
                    Department = row["Department"]?.ToString()!,
                    Role = row["Role"]?.ToString()!
                };

                users.Add(user);
            }
            return users;
        }

        /// <summary>
        /// This method deletes a user
        /// </summary>
        /// <param name="userName">The user name of the user</param>
        /// <returns>True if the user was deleted, otherwise false</returns>
        public async Task<Result> DeleteUser(string username)
        {
            string query = $"DROP USER {username}";
            try 
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
            }

            query = $"DELETE FROM ClickSphere.Users WHERE UserName = '{username}'";

            try 
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
            }

            // check if user was deleted
            query = $"SELECT name FROM system.users WHERE name = '{username}'";
            var result = await _dbService.ExecuteScalar(query);
            if (result is not DBNull)
                return Result.BadRequest("Could not delete user");
            else
                return Result.Ok();
        }

        /// <summary>
        /// This method updates the password of a user
        /// </summary>
        /// <param name="userName">The user name of the user</param>
        /// <param name="newPassword">The new password of the user</param>
        /// <returns>True if the password was updated, otherwise false</returns>
        public async Task<Result> UpdatePassword(string userName, string newPassword)
        {
            string query = $"ALTER USER {userName} IDENTIFIED BY '{newPassword}'";
            var result = await _dbService.ExecuteNonQuery(query);
            return result > 0 ? Result.Ok() : Result.BadRequest("Could not update password");
        }

        /// <summary>
        /// This method gets the user configuration from ClickSphere.Users
        /// </summary>
        /// <param name="userName">The user name of the user</param>
        /// <returns>The user configuration</returns>
        public async Task<UserConfig?> GetUserConfig(Guid userId)
        {
            string query = $"SELECT * FROM ClickSphere.Users WHERE Id = '{userId}'";
            var result = await _dbService.ExecuteQueryDictionary(query);
            var userConfig = result.Select(row => new UserConfig
            {
                Id = Guid.Parse(row["Id"].ToString()!),
                Username = row["UserName"].ToString()!,
                LDAP_User = row["LDAP_User"].ToString()!,
                FirstName = row["FirstName"].ToString()!,
                LastName = row["LastName"].ToString()!,
                Email = row["Email"].ToString()!,
                Phone = row["Phone"].ToString()!,
                Department = row["Department"].ToString()!,
                Role = row["Role"].ToString()!
            }).FirstOrDefault();

            return userConfig;
        }

        /// <summary>
        /// This method updates the user configuration in ClickSphere.Users
        /// </summary>
        /// <param name="user">The user configuration to update</param>
        /// <returns>True if the user configuration was updated, otherwise false</returns>
        public async Task<Result> UpdateUser(UserConfig user)
        {
            string query = $"ALTER TABLE ClickSphere.Users UPDATE " +
                           $"UserName = '{user.Username}'," +
                           $"LDAP_User = '{user.LDAP_User}'," +
                           $"FirstName = '{user.FirstName}'," + 
                           $"LastName = '{user.LastName}'," +
                           $"Email = '{user.Email}'," +
                           $"Phone = '{user.Phone}'," +
                           $"Department = '{user.Department}'," +
                           $"Role = '{user.Role}'" +
                           $"WHERE Id = '{user.Id}'";
            var nReturn = await _dbService.ExecuteNonQuery(query);

            // get previous role
            query = $"SELECT Role FROM ClickSphere.Users WHERE Id = '{user.Id}'";
            var result = await _dbService.ExecuteScalar(query);
            string? previousRole = result?.ToString()!;

            // remove previous role
            query = $"REVOKE {previousRole} FROM {user.Username}";
            try
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
                // don't catch exception
            }

            // update role
            query = $"GRANT {user.Role} TO {user.Username}";
            try
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
                return Result.BadRequest("Could not update role");
            }

            query = $"SET DEFAULT ROLE {user.Role} TO {user.Username}";
            try
            {
                await _dbService.ExecuteNonQuery(query);
            }
            catch(Exception)
            {
                return Result.BadRequest("Could not update role");
            }
            return nReturn == 0 ? Result.Ok() : Result.BadRequest("Could not update user");
        }
    }
}
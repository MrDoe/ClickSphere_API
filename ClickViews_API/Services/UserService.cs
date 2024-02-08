using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ClickViews_API.Services
{
    /**
     * This class is used to manage user accounts
     */
    public class UserService
    {
        private readonly DbService _dbService;

        /**
        * This is the constructor for the UserService class
        */
        public UserService(DbService dbService)
        {
            _dbService = dbService;
        }

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
            query = $"CREATE USER IF NOT EXISTS {username} IDENTIFIED BY '{password}'";
            await _dbService.ExecuteNonQuery(query);
            return true;
        }

        /**
        * This method is used to check if the login credentials are valid
        * @param username The username of the user
        * @param password The password of the user
        * @return A ClaimsPrincipal object that contains the user's claims if the login credentials are valid, otherwise null
        */
        public ClaimsPrincipal? CheckLogin(string username, string password)
        {
            if (username == "admin" && password == "password")
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
    }
}
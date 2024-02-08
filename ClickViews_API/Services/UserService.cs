using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ClickViews_API.Services
{
    /**
     * This class is used to manage user accounts
     */
    public class UserService
    {
        private readonly IConfiguration _configuration;
        private readonly DbService _dbService;
        private readonly string _key;

        /**
        * This is the constructor for the UserService class
        */
        public UserService(IConfiguration configuration, DbService dbService)
        {
            _configuration = configuration;
            _dbService = dbService;
            _key = _configuration["Jwt:Key"] ?? throw new NullReferenceException("Jwt:Key");
        }

        /**
        * This method is used to generate a token
        * @param username The username of the user
        * @return The token
        */
        public string GenerateToken(string username)
        {
            var key = Encoding.ASCII.GetBytes(_key!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /**
        * This method is used to validate a token
        * @param token The token
        * @return True if the token is valid, otherwise false
        */
        public bool ValidateToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /**
        * This method is used to get the username from a token
        * @param token The token
        * @return The username of the user
        */
        public string GetUsernameFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken ?? throw new Exception("Invalid token");
            return securityToken.Subject;
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
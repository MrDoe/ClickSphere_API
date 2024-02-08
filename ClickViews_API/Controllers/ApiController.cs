using System.Text;
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
    public class ApiController(DbService dbService, UserService userService) : ControllerBase
    {
        private readonly DbService _dbService = dbService;
        private UserService _userService = userService;

        /**
        * This method is used to log in a user
        * @param model The user to log in
        * @return The result of the login
        */
        [AllowAnonymous]
        [HttpPost]
        [Route("/login")]
        public IResult Login([FromBody] UserModel model)
        {
            var claimsPrincipal = _userService.CheckLogin(model.Username, model.Password);

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
        public async Task<IResult> CreateUser([FromBody] UserModel model)
        {
            bool result = await _userService.CreateUser(model.Username, model.Password);
            if (result)
                return Results.Ok();
            else
                return Results.BadRequest("Could not create user: User already exists.");
        }

        /**
        * Get the version of the ClickHouse server
        * @return The version of the server
        */
        [Authorize]
        [HttpGet]
        [Route("/version")]
        public async Task<string> GetVersion()
        {
            var version = await _dbService.ExecuteScalar("SELECT version()");
            return $"ClickHouse version: {version}";
        }

        /**
        * Get the databases of the ClickHouse server
        * @return The databases of the server
        */
        [Authorize]
        [HttpGet]
        [Route("/getDatabases")]
        public async Task<IEnumerable<string>> GetDatabases()
        {
            return await _dbService.ExecuteQuery("SHOW DATABASES");
        }

        /**
        * Get the tables of a database
        * @param database The database to get the tables from
        * @return The tables of the database
        */
        [Authorize]
        [HttpGet]
        [Route("/getTables")]
        public async Task<IEnumerable<string>> GetTables(string database)
        {
            return await _dbService.ExecuteQuery($"SHOW TABLES FROM {database}");
        }

        /**
        * Get the views of a database
        * @param database The database to get the views from
        * @return The views of the database
        */
        [Authorize]
        [HttpGet]
        [Route("/getViews")]
        public async Task<IEnumerable<string>> GetViews(string database)
        {
           return await _dbService.ExecuteQuery($"SELECT name FROM system.tables where database = '{database}' and engine = 'View'");
        }

        /**
        * Get the columns of a table
        * @param database The database where the table is located
        * @param table The table to get the columns from
        * @return The columns of the table
        */
        [Authorize]
        [HttpGet]
        [Route("/getColumns")]
        public async Task<IEnumerable<string>> GetColumns(string database, string table)
        {
            return await _dbService.ExecuteQuery($"DESCRIBE {database}.{table}");
        }

        /**
        * Get the first n rows from a table
        * @param database The database where the table is located
        * @param table The table to get the rows from
        * @param limit The number of rows to get
        * @return The first n rows from the table
        */
        [Authorize]
        [HttpGet]
        [Route("/getRows")]
        public async Task<IEnumerable<Dictionary<string, object>>> GetRows(string? database, string table, uint limit)
        {
            var sql = $"SELECT Titel, Name, Vorname, toString(Geburtsdatum), Geschlecht, BMBH_PID, UKH_PID FROM {database}.{table} LIMIT {limit}";
            return await _dbService.ExecuteQueryDictionary(sql);
        }

        /**
        * Execute custom query with SQL string in base64 format
        * @param query The query to be executed
        * @return The result of the query
        */
        [Authorize]
        [HttpGet]
        [Route("/customQuery")]
        public async Task<IEnumerable<Dictionary<string, object>>> CustomQuery(string query)
        {
            // decode base64 string
            string decodedQuery = Encoding.UTF8.GetString(Convert.FromBase64String(query));

            // abort if query contains data modifying statements
            if (decodedQuery.Contains("INSERT INTO") || decodedQuery.Contains("UPDATE") || decodedQuery.Contains("DELETE") || decodedQuery.Contains("DROP") ||
                decodedQuery.Contains("GRANT") || decodedQuery.Contains("REVOKE") || decodedQuery.Contains("CREATE") || decodedQuery.Contains("ALTER") ||
                decodedQuery.Contains("TRUNCATE") || decodedQuery.Contains("RENAME"))
            {
                return [new Dictionary<string, object> { { "error", "Data modifying statements are not allowed" } }];
            }

            return await _dbService.ExecuteQueryDictionary(decodedQuery);
        }
    }
}
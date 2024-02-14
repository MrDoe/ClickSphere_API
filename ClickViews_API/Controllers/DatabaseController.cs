using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickViews_API.Services;

namespace ClickViews_API.Controllers
{
    /**
     * The base class for ClickViews API controllers.
     */
    [ApiController]
    public class DatabaseController(IDbService dbService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

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
        * Create a new database
        * @param database The name of the database to create
        * @return The result of the database creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createDatabase")]
        public async Task<IResult> CreateDatabase(string database)
        {
            int result = await _dbService.ExecuteNonQuery($"CREATE DATABASE {database}");
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not create database");
        }

        /**
        * Drop a database
        * @param database The name of the database to drop
        * @return The result of the database drop
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteDatabase")]
        public async Task<IResult> DeleteDatabase(string database)
        {
            int result = await _dbService.ExecuteNonQuery($"DROP DATABASE {database}");
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not drop database");
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
    }
}
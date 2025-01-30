using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;

namespace ClickSphere_API.Controllers
{
    /// <summary>
    /// The base class for ClickSphere API controllers. 
    /// </summary>
    [ApiController]
    public class DatabaseController(IDbService dbService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

        /// <summary>
        /// Get the databases of the ClickHouse server.
        /// </summary>
        /// <returns>The databases of the server.</returns>
        [Authorize]
        [HttpGet]
        [Route("/getDatabases")]
        public async Task<IEnumerable<string>> GetDatabases()
        {
            return await _dbService.ExecuteQuery("SHOW DATABASES");
        }

        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="database" example="ClickSphere">The name of the database to create.</param>
        /// <returns>The result of the database creation.</returns>
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

        /// <summary>
        /// Drop a database.
        /// </summary>
        /// <param name="database">The name of the database to drop.</param>
        /// <returns>The result of the database drop.</returns>
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

        /// <summary>
        /// Get the tables of a database.
        /// </summary>
        /// <param name="database">The database to get the tables from.</param>
        /// <returns>The tables of the database.</returns>
        [Authorize]
        [HttpGet]
        [Route("/getTables")]
        public async Task<IEnumerable<string>> GetTables(string database)
        {
            return await _dbService.ExecuteQuery($"SHOW TABLES FROM {database}");
        }
    }
}
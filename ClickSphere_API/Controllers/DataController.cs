using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;

namespace ClickSphere_API.Controllers
{
    /**
     * The base class for ClickSphere API controllers.
     */
    [ApiController]
    public class DataController(IDbService dbService, ISqlParser sqlParser) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

        /**
        * Execute custom query with SQL string in base64 format
        * @param query The query to be executed
        * @return The result of the query
        */
        [Authorize]
        [HttpPost]
        [Route("/customQuery")]
        public async Task<IEnumerable<Dictionary<string, object>>> CustomQuery([FromBody] string query)
        {
            // decode base64 string
            query = Encoding.UTF8.GetString(Convert.FromBase64String(query));

            // validate and sanitize the input
            var parsedQuery = sqlParser.Parse(query);
            if (!parsedQuery.IsValid || parsedQuery.SanitizedQuery == null)
            {
                return [new Dictionary<string, object> { { "error", "Invalid SQL query" } }];
            }

            // execute the sanitized query
            try 
            {
                return await _dbService.ExecuteQueryDictionary(parsedQuery.SanitizedQuery);
            }
            catch (Exception e)
            {
                return [new Dictionary<string, object> { { "Error", e.Message } }];
            }
        }
    }
}
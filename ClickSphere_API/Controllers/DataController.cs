using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;

namespace ClickSphere_API.Controllers
{
    /// <summary>
    /// The base class for ClickSphere API controllers.
    /// </summary>
    [ApiController]
    public class DataController(IDbService dbService, ISqlParser sqlParser) : ControllerBase
    {
        /// <summary>
        /// Execute custom query with SQL string in base64 format.
        /// </summary>
        /// <param name="query">The query to be executed.</param>
        /// <returns>The result of the query.</returns>
        //[Authorize]
        [HttpPost]
        [Route("/customQuery")]
        public async Task<IEnumerable<Dictionary<string, object>>> CustomQuery([FromBody] string query)
        {
            // decode base64 string
            try 
            {
                query = Encoding.UTF8.GetString(Convert.FromBase64String(query));
            }
            catch (Exception e)
            {
                return [new Dictionary<string, object> { { "error", "Invalid base64 string\n" + e.Message } }];
            }

            // validate and sanitize the input
            var parsedQuery = sqlParser.Parse(query);
            if (!parsedQuery.IsValid || parsedQuery.SanitizedQuery == null)
            {
                return [new Dictionary<string, object> { { "error", "Invalid SQL query" } }];
            }

            // execute the sanitized query
            try 
            {
                return await dbService.ExecuteQueryDictionary(parsedQuery.SanitizedQuery);
            }
            catch (Exception e)
            {
                return [new Dictionary<string, object> { { "Error", e.Message } }];
            }
        }
    }
}
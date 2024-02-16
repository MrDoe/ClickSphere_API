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
    public class DataController(IDbService dbService) : ControllerBase
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

            // abort if query contains data modifying statements
            if (query.Contains("INSERT INTO") || query.Contains("UPDATE") || query.Contains("DELETE") || query.Contains("DROP") ||
                query.Contains("GRANT") || query.Contains("REVOKE") || query.Contains("CREATE") || query.Contains("ALTER") ||
                query.Contains("TRUNCATE") || query.Contains("RENAME"))
            {
                return [new Dictionary<string, object> { { "error", "Data modifying statements are not allowed" } }];
            }

            return await _dbService.ExecuteQueryDictionary(query);
        }
   }
}
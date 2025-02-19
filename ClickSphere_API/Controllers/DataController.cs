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
        /// <param name="b64Query">The query to be executed in Base64 encoding.</param>
        /// <returns>The result of the query.</returns>
        //[Authorize]
        [HttpGet]
        [Route("/customQuery")]
        public async IAsyncEnumerable<Dictionary<string, object>> CustomQuery(string b64Query)
        {
            Dictionary<string, object>? errorResult = null;

            // Decode base64
            string query = "";
            try
            {
                query = Encoding.UTF8.GetString(Convert.FromBase64String(b64Query));
            }
            catch (Exception e)
            {
                errorResult = new Dictionary<string, object> { { "error", "Invalid base64 string\n" + e.Message } };
            }

            // Validate query
            var parsedQuery = sqlParser.Parse(query);
            if (!parsedQuery.IsValid || parsedQuery.SanitizedQuery == null)
            {
                errorResult = new Dictionary<string, object> { { "error", "Invalid query" } };
            }

            if (errorResult != null)
            {
                yield return errorResult;
                yield break;
            }

            // read data
            using var connection = dbService.CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetValue(i).ToString() == "NaN")
                        row[reader.GetName(i)] = "NaN";
                    else
                        row[reader.GetName(i)] = reader.GetValue(i);
                }
                yield return row;
            }
        }
    }
}
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using System.Text.RegularExpressions;

namespace ClickSphere_API.Controllers
{
    /// <summary>
    /// The base class for ClickSphere API controllers.
    /// </summary>
    [ApiController]
    public class DataController(IDbService dbService, ISqlParser sqlParser) : ControllerBase
    {
        /// <summary>
        /// Get row count for custom query with SQL string in base64 format.
        /// </summary>
        /// <param name="b64Query">The query to be executed in Base64 encoding.</param>
        /// <returns>The row count of the query.</returns>
        //[Authorize]
        [HttpGet]
        [Route("/customQuery/count")]
        public async Task<int> CustomQueryCount(string b64Query)
        {
            // Decode base64
            string query = Encoding.UTF8.GetString(Convert.FromBase64String(b64Query));

            // replace SELECT statement until FROM by 'SELECT COUNT(*)' (case sensitive)
            query = Regex.Replace(query, @"^SELECT.*FROM", "SELECT COUNT(*) FROM", RegexOptions.Singleline);

            // remove any GROUP BY clause including aliases and fields
            query = Regex.Replace(query, @"GROUP\s+BY\s+.*\n", "", RegexOptions.IgnoreCase);

            // remove any LIMIT clause
            query = Regex.Replace(query, @"LIMIT\s+.*?(\s|$)", "", RegexOptions.IgnoreCase);

            // remove any ORDER BY clause with ASC or DESC
            query = Regex.Replace(query, @"ORDER\s+BY\s+.*?ASC(\s|$)", "", RegexOptions.IgnoreCase);
            query = Regex.Replace(query, @"ORDER\s+BY\s+.*?DESC(\s|$)", "", RegexOptions.IgnoreCase);

            // Validate query
            var parsedQuery = sqlParser.Parse(query);
            if (!parsedQuery.IsValid || parsedQuery.SanitizedQuery == null)
            {
                return -1;
            }

            // read data
            using var connection = dbService.CreateConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand(query);
            var reader = await command.ExecuteScalarAsync();
            return Convert.ToInt32(reader);
        }


        /// <summary>
        /// Execute custom query with SQL string in base64 format.
        /// </summary>
        /// <param name="b64Query">The query to be executed in Base64 encoding.</param>
        /// <param name="from">The starting index of the data to retrieve.</param>
        /// <param name="to">The ending index of the data to retrieve.</param>
        /// <returns>The result of the query.</returns>
        //[Authorize]
        [HttpGet]
        [Route("/customQuery")]
        public async IAsyncEnumerable<Dictionary<string, object>> CustomQuery(string b64Query, int from, int to)
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

            // append pagination
            if(from != -1 || to != -1)
            {
                query += $" LIMIT {from}, {to}";
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
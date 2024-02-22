using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClickSphere_API.Services;
using ClickSphere_API.Models;

namespace ClickSphere_API.Controllers
{
    /**
     * The base class for ClickSphere API controllers.
     */
    [ApiController]
    public class TableController(IDbService dbService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

                /**
        * Create a new table in the specified database
        * @param database The database where the table should be created
        * @param tableName The name of the table to create
        * @param columns The columns of the table in the format "column1 datatype, column2 datatype, ..."
        * @return The result of the table creation
        */
        [Authorize]
        [HttpPost]
        [Route("/createTable")]
        public async Task<IResult> CreateTable(CreateTableRequest request)
        {
            string query = $"CREATE TABLE {request.Database}.{request.Table} " +
                           $"({request.Columns}) " +
                           $"ENGINE = {request.Engine}() ";
            
            // add order by only if defined
            if(!string.IsNullOrEmpty(request.OrderBy))
            {
                query += $"ORDER BY ({request.OrderBy})";
            }

            query += ";";
            
            int result = await _dbService.ExecuteNonQuery(query);
            
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not create table");
        }

        /**
        * Delete a table from a database
        * @param database The database where the table is located
        * @param table The table to delete
        * @return The result of the table deletion
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteTable")]
        public async Task<IResult> DeleteTable(string database, string table)
        {
            int result = await _dbService.ExecuteNonQuery($"DROP TABLE {database}.{table}");
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not drop table");
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
        [HttpPost]
        [Route("/getRows")]
        public async Task<IEnumerable<Dictionary<string, object>>> CustomQuery([FromBody] GetRowsRequest request)
        {
            var sql = $"SELECT {request.Columns} FROM {request.Database}.{request.Table} ";
            
            if(!string.IsNullOrEmpty(request.OrderBy))
            {
                sql += $"ORDER BY {request.OrderBy} ";
            }

            if(!string.IsNullOrEmpty(request.Limit))
            {
                sql += $"LIMIT {request.Limit}";
            }

            sql += ";";

            return await _dbService.ExecuteQueryDictionary(sql);
        }

        /**
        * Insert a row into a table
        * @param database The database where the table is located
        * @param table The table to insert the row into
        * @param values The values of the row in the format "value1, value2, ..."
        * @return The result of the insertion
        */
        [Authorize]
        [HttpPost]
        [Route("/insertRow")]
        public async Task<IResult> InsertRow(InsertRowRequest request)
        {
            string query = $"INSERT INTO {request.Database}.{request.Table} " +
                           $"VALUES ({request.Values});";
            
            int result = await _dbService.ExecuteNonQuery(query);
            
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not insert row");
        }

        /**
        * Update a row in a table
        * @param database The database where the table is located
        * @param table The table to update the row in
        * @param values The new values of the row in the format "column1 = value1, column2 = value2, ..."
        * @param condition The condition that the row must satisfy
        * @return The result of the update
        */
        [Authorize]
        [HttpPost]
        [Route("/updateRow")]
        public async Task<IResult> UpdateRow(UpdateRowRequest request)
        {
            string query = $"UPDATE {request.Database}.{request.Table} " +
                           $"SET {request.Values} " +
                           $"WHERE {request.Condition};";
            
            int result = await _dbService.ExecuteNonQuery(query);
            
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not update row");
        }

        /**
        * Delete a row from a table
        * @param database The database where the table is located
        * @param table The table to delete the row from
        * @param condition The condition that the row must satisfy
        * @return The result of the deletion
        */
        [Authorize]
        [HttpDelete]
        [Route("/deleteRow")]
        public async Task<IResult> DeleteRow(DeleteRowRequest request)
        {
            string query = $"DELETE FROM {request.Database}.{request.Table} " +
                           $"WHERE {request.Condition};";
            
            int result = await _dbService.ExecuteNonQuery(query);
            
            if (result == 0)
                return Results.Ok();
            else
                return Results.BadRequest("Could not delete row");
        }
    }
}